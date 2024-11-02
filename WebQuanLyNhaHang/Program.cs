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
app.UseCookiePolicy();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapHub<ChatHub>("/chatHub"); // Map hub vào đường dẫn /chatHub
app.Run();
