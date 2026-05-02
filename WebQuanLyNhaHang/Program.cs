using Microsoft.EntityFrameworkCore;
using WebQuanLyNhaHang.Hubs;
using WebQuanLyNhaHang.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
// Cấu hình session
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // Thời gian session tồn tại
    options.Cookie.HttpOnly = true; // Bảo mật cookie chỉ có thể truy cập thông qua HTTP
    options.Cookie.IsEssential = true; // Cho phép session hoạt động kể cả khi GDPR bật
});
//  Đăng ký dịch vụ SignalR
builder.Services.AddSignalR();
//
builder.Services.AddDbContext<QlnhaHangBtlContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("QlnhaHangBtlContext")));
var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseSession();
app.Use(async (context, next) =>
{
    const string customerSessionKey = "CustomerID";
    const string customerCookieName = "CloudyCafeCustomer";
    const string protectorPurpose = "CloudyCafe.CustomerCookie.v1";

    if (!context.Session.GetInt32(customerSessionKey).HasValue &&
        context.Request.Cookies.TryGetValue(customerCookieName, out var protectedCustomerId))
    {
        var protector = context.RequestServices
            .GetRequiredService<Microsoft.AspNetCore.DataProtection.IDataProtectionProvider>()
            .CreateProtector(protectorPurpose);

        try
        {
            var customerIdText = Microsoft.AspNetCore.DataProtection.DataProtectionCommonExtensions.Unprotect(protector, protectedCustomerId);
            if (int.TryParse(customerIdText, out var customerId))
            {
                var db = context.RequestServices.GetRequiredService<QlnhaHangBtlContext>();
                var customerExists = await db.KhachHangs
                    .AnyAsync(customer => customer.KhId == customerId && !customer.Remove);

                if (customerExists)
                {
                    context.Session.SetInt32(customerSessionKey, customerId);
                }
                else
                {
                    context.Response.Cookies.Delete(customerCookieName);
                }
            }
        }
        catch
        {
            context.Response.Cookies.Delete(customerCookieName);
        }
    }
    await next();
});
app.UseCookiePolicy();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=TrangChu}/{action=Index}/{id?}");
app.MapHub<ChatHub>("/chatHub"); // Map hub vào đường dẫn /chatHub
app.Run();
