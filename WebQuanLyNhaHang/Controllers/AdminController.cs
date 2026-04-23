using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Text.Json;
using WebQuanLyNhaHang.Filters;
using WebQuanLyNhaHang.Models;
using WebQuanLyNhaHang.ViewModel;

namespace WebQuanLyNhaHang.Controllers
{
    public class AdminController : Controller
    {
        private static readonly CultureInfo VietnameseCulture = new("vi-VN");
        private readonly QlnhaHangBtlContext _context;

        public AdminController(QlnhaHangBtlContext context)
        {
            _context = context;
        }

        [AdminSessionAuthorize]
        public async Task<IActionResult> Index()
        {
            var orders = await _context.DonHangs
                .AsNoTracking()
                .Select(order => new DashboardOrderSnapshot(
                    order.DhId,
                    order.GioVao ?? order.GioRa,
                    order.BanId.HasValue,
                    order.KhId,
                    order.TongTien ?? 0m))
                .ToListAsync();

            var lineItems = await _context.ChiTietHoaDons
                .AsNoTracking()
                .Select(item => new DashboardLineSnapshot(
                    item.DhId,
                    item.Dh.GioVao ?? item.Dh.GioRa,
                    item.Dh.BanId.HasValue,
                    item.Dh.KhId,
                    item.SoLuong ?? 0,
                    item.ThanhTien ?? 0m,
                    item.Product != null ? item.Product.GiaTien ?? 0m : 0m,
                    item.Product != null ? item.Product.TenSanPham ?? "Mon chua dat ten" : "Mon chua dat ten"))
                .ToListAsync();

            var customerCount = await _context.KhachHangs.AsNoTracking().CountAsync();
            var employeeCount = await _context.NhanViens.AsNoTracking().CountAsync();
            var productCount = await _context.Products.AsNoTracking().CountAsync();
            var tableCount = await _context.Bans.AsNoTracking().CountAsync();

            var lineRevenueByOrder = lineItems
                .GroupBy(item => item.OrderId)
                .ToDictionary(group => group.Key, group => group.Sum(GetResolvedRevenue));

            var orderFacts = orders
                .Where(order => order.OrderDate.HasValue)
                .Select(order => new DashboardOrderFact(
                    order.OrderDate!.Value.Date,
                    order.IsDineIn,
                    order.CustomerId,
                    ResolveOrderRevenue(order, lineRevenueByOrder)))
                .ToList();

            var productFacts = lineItems
                .Where(item => item.OrderDate.HasValue && item.Quantity > 0)
                .Select(item => new DashboardProductFact(
                    item.OrderDate!.Value.Date,
                    item.IsDineIn,
                    item.ProductName,
                    item.Quantity,
                    GetResolvedRevenue(item)))
                .ToList();

            var anchorDate = orderFacts.Count > 0
                ? orderFacts.Max(item => item.OrderDate)
                : DateTime.Today;

            var currentMonthStart = new DateTime(anchorDate.Year, anchorDate.Month, 1);
            var nextMonthStart = currentMonthStart.AddMonths(1);
            var previousMonthStart = currentMonthStart.AddMonths(-1);

            var currentMonthOrders = orderFacts
                .Where(order => order.OrderDate >= currentMonthStart && order.OrderDate < nextMonthStart)
                .ToList();

            var previousMonthOrders = orderFacts
                .Where(order => order.OrderDate >= previousMonthStart && order.OrderDate < currentMonthStart)
                .ToList();

            var currentMonthProducts = productFacts
                .Where(item => item.OrderDate >= currentMonthStart && item.OrderDate < nextMonthStart)
                .ToList();

            var previousMonthProducts = productFacts
                .Where(item => item.OrderDate >= previousMonthStart && item.OrderDate < currentMonthStart)
                .ToList();

            var currentRevenue = currentMonthOrders.Sum(order => order.Revenue);
            var previousRevenue = previousMonthOrders.Sum(order => order.Revenue);
            var currentOrderCount = currentMonthOrders.Count;
            var previousOrderCount = previousMonthOrders.Count;
            var currentProductSold = currentMonthProducts.Sum(item => item.Quantity);
            var previousProductSold = previousMonthProducts.Sum(item => item.Quantity);
            var activeCustomersCurrent = currentMonthOrders
                .Where(order => order.CustomerId.HasValue)
                .Select(order => order.CustomerId!.Value)
                .Distinct()
                .Count();

            var dineInRevenue = currentMonthOrders
                .Where(order => order.IsDineIn)
                .Sum(order => order.Revenue);

            var takeAwayRevenue = currentMonthOrders
                .Where(order => !order.IsDineIn)
                .Sum(order => order.Revenue);

            var averageOrderValue = currentOrderCount > 0
                ? currentRevenue / currentOrderCount
                : 0m;

            var payload = new DashboardPayload
            {
                SnapshotLabel = $"Du lieu chot den {anchorDate:dd/MM/yyyy}",
                RevenueSeries = BuildRevenueSeries(orderFacts, anchorDate),
                OrderSeries = BuildOrderSeries(orderFacts, anchorDate),
                RevenueComparisonSeries = BuildRevenueComparisonSeries(orderFacts, anchorDate),
                TopProducts = BuildTopProducts(productFacts, anchorDate)
            };

            var model = new AdminDashboardViewModel
            {
                AdminDisplayName = HttpContext.Session.GetString("NhanVienName") ?? "Admin",
                AdminAccount = HttpContext.Session.GetString("NhanVienTaiKhoan") ?? "admin",
                AdminRoleLabel = "Quan tri van hanh",
                Initials = BuildInitials(HttpContext.Session.GetString("NhanVienName") ?? "Admin"),
                SnapshotLabel = payload.SnapshotLabel,
                RevenueMetric = BuildCurrencyMetric(
                    "Doanh thu thang",
                    currentRevenue,
                    previousRevenue,
                    "so voi thang truoc",
                    "Tong hop tu chi tiet hoa don va doanh thu thuc te."),
                OrderMetric = BuildCountMetric(
                    "Don hang thang",
                    currentOrderCount,
                    previousOrderCount,
                    "so voi thang truoc",
                    "Gom don tai cho va mang ve trong cung ky."),
                CustomerMetric = BuildNeutralMetric(
                    "Khach hang he thong",
                    FormatCount(customerCount),
                    $"{FormatCount(activeCustomersCurrent)} dang mua",
                    "trong thang du lieu moi nhat",
                    "Tong ho so khach hang dang co tren he thong."),
                ProductMetric = BuildCountMetric(
                    "San pham da ban",
                    currentProductSold,
                    previousProductSold,
                    "so voi thang truoc",
                    "Tong so luong mon da xuat trong thang."),
                InsightCards = new List<DashboardInsightCard>
                {
                    new()
                    {
                        Label = "Gia tri trung binh / don",
                        Value = FormatCurrency(averageOrderValue),
                        Note = "Lay theo don co doanh thu trong thang neo du lieu.",
                        ToneCssClass = "is-green"
                    },
                    new()
                    {
                        Label = "Doanh thu tai cho",
                        Value = FormatCurrency(dineInRevenue),
                        Note = "Tinh theo cac don co ban phuc vu gan vao hoa don.",
                        ToneCssClass = "is-blue"
                    },
                    new()
                    {
                        Label = "Doanh thu mang ve",
                        Value = FormatCurrency(takeAwayRevenue),
                        Note = "Ap dung cho cac don khong gan ban trong he thong.",
                        ToneCssClass = "is-purple"
                    },
                    new()
                    {
                        Label = "Quy mo van hanh",
                        Value = $"{FormatCount(employeeCount)} NV / {FormatCount(tableCount)} ban",
                        Note = $"{FormatCount(productCount)} mon dang kinh doanh trong danh muc.",
                        ToneCssClass = "is-orange"
                    }
                },
                DashboardPayloadJson = JsonSerializer.Serialize(
                    payload,
                    new JsonSerializerOptions
                    {
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                    })
            };

            return View(model);
        }

        public IActionResult Login()
        {
            if (HttpContext.Session.GetInt32("NhanVienId").HasValue)
            {
                return RedirectToAction(nameof(Index));
            }

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Login(string? name, string? password)
        {
            name = name?.Trim();
            password = password?.Trim();

            if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(password))
            {
                ViewBag.Error = "Tên đăng nhập và mật khẩu không được để trống.";
                return View();
            }

            var nhanVien = _context.NhanViens.FirstOrDefault(e => e.TaiKhoan == name && e.MatKhau == password);
            if (nhanVien == null)
            {
                ViewBag.Error = "Tài khoản hoặc mật khẩu không đúng.";
                return View();
            }

            HttpContext.Session.SetInt32("NhanVienId", nhanVien.NvId);
            HttpContext.Session.SetString("NhanVienName", nhanVien.TenNhanVien ?? nhanVien.TaiKhoan);
            HttpContext.Session.SetString("NhanVienTaiKhoan", nhanVien.TaiKhoan);

            return RedirectToAction(nameof(Index));
        }

        [AdminSessionAuthorize]
        public IActionResult Logout()
        {
            HttpContext.Session.Remove("NhanVienId");
            HttpContext.Session.Remove("NhanVienName");
            HttpContext.Session.Remove("NhanVienTaiKhoan");

            return RedirectToAction(nameof(Login));
        }

        [AdminSessionAuthorize]
        public IActionResult Ban()
        {
            ViewModelBan viewModelBan = new ViewModelBan(_context);
            return View(viewModelBan);
        }

        [HttpGet]
        [AdminSessionAuthorize]
        public IActionResult GetFormBuy(int id)
        {
            ViewModelGetFormBuy viewModel = new ViewModelGetFormBuy(_context);
            var donhangs = viewModel.CTDH_Product(id);
            return PartialView("GetFormBuy", donhangs);
        }

        [HttpGet]
        [AdminSessionAuthorize]
        public IActionResult ProcessPayment(int BanId)
        {
            var donHangList = _context.DonHangs.Where(e => e.BanId == BanId).ToList();

            foreach (var donHang in donHangList)
            {
                donHang.BanId = null;
            }

            _context.SaveChanges();
            return NoContent();
        }

        private static Dictionary<string, DashboardValueSeries> BuildRevenueSeries(
            IEnumerable<DashboardOrderFact> orderFacts,
            DateTime anchorDate)
        {
            return new Dictionary<string, DashboardValueSeries>(StringComparer.OrdinalIgnoreCase)
            {
                ["daily"] = BuildValueSeries(
                    BuildDailyBuckets(anchorDate, 14),
                    orderFacts,
                    order => order.OrderDate,
                    order => order.Revenue),
                ["monthly"] = BuildValueSeries(
                    BuildMonthlyBuckets(anchorDate, 12),
                    orderFacts,
                    order => new DateTime(order.OrderDate.Year, order.OrderDate.Month, 1),
                    order => order.Revenue),
                ["yearly"] = BuildValueSeries(
                    BuildYearlyBuckets(anchorDate, 5),
                    orderFacts,
                    order => new DateTime(order.OrderDate.Year, 1, 1),
                    order => order.Revenue)
            };
        }

        private static Dictionary<string, DashboardCountSeries> BuildOrderSeries(
            IEnumerable<DashboardOrderFact> orderFacts,
            DateTime anchorDate)
        {
            return new Dictionary<string, DashboardCountSeries>(StringComparer.OrdinalIgnoreCase)
            {
                ["daily"] = BuildCountSeries(
                    BuildDailyBuckets(anchorDate, 14),
                    orderFacts,
                    order => order.OrderDate),
                ["monthly"] = BuildCountSeries(
                    BuildMonthlyBuckets(anchorDate, 12),
                    orderFacts,
                    order => new DateTime(order.OrderDate.Year, order.OrderDate.Month, 1)),
                ["yearly"] = BuildCountSeries(
                    BuildYearlyBuckets(anchorDate, 5),
                    orderFacts,
                    order => new DateTime(order.OrderDate.Year, 1, 1))
            };
        }

        private static Dictionary<string, DashboardComparisonSeries> BuildRevenueComparisonSeries(
            IEnumerable<DashboardOrderFact> orderFacts,
            DateTime anchorDate)
        {
            return new Dictionary<string, DashboardComparisonSeries>(StringComparer.OrdinalIgnoreCase)
            {
                ["daily"] = BuildComparisonSeries(BuildDailyBuckets(anchorDate, 14), orderFacts, order => order.OrderDate),
                ["monthly"] = BuildComparisonSeries(
                    BuildMonthlyBuckets(anchorDate, 12),
                    orderFacts,
                    order => new DateTime(order.OrderDate.Year, order.OrderDate.Month, 1)),
                ["yearly"] = BuildComparisonSeries(
                    BuildYearlyBuckets(anchorDate, 5),
                    orderFacts,
                    order => new DateTime(order.OrderDate.Year, 1, 1))
            };
        }

        private static Dictionary<string, List<DashboardTopProductItem>> BuildTopProducts(
            IEnumerable<DashboardProductFact> productFacts,
            DateTime anchorDate)
        {
            return new Dictionary<string, List<DashboardTopProductItem>>(StringComparer.OrdinalIgnoreCase)
            {
                ["daily"] = BuildTopProductList(
                    productFacts.Where(item => item.OrderDate >= anchorDate.AddDays(-13) && item.OrderDate <= anchorDate)),
                ["monthly"] = BuildTopProductList(
                    productFacts.Where(item => item.OrderDate >= new DateTime(anchorDate.Year, anchorDate.Month, 1).AddMonths(-11)
                        && item.OrderDate < new DateTime(anchorDate.Year, anchorDate.Month, 1).AddMonths(1))),
                ["yearly"] = BuildTopProductList(
                    productFacts.Where(item => item.OrderDate >= new DateTime(anchorDate.Year - 4, 1, 1)
                        && item.OrderDate < new DateTime(anchorDate.Year + 1, 1, 1)))
            };
        }

        private static DashboardValueSeries BuildValueSeries(
            IReadOnlyList<DashboardBucket> buckets,
            IEnumerable<DashboardOrderFact> orderFacts,
            Func<DashboardOrderFact, DateTime> bucketSelector,
            Func<DashboardOrderFact, decimal> valueSelector)
        {
            var facts = orderFacts.ToList();
            var values = buckets
                .Select(bucket => facts
                    .Where(order => bucketSelector(order) == bucket.Start)
                    .Sum(valueSelector))
                .ToList();

            var total = values.Sum();
            var peakValue = values.Count > 0 ? values.Max() : 0m;
            var peakIndex = values.FindIndex(value => value == peakValue);
            var peakLabel = peakIndex >= 0 ? buckets[peakIndex].Label : "Chua co du lieu";

            return new DashboardValueSeries
            {
                Labels = buckets.Select(bucket => bucket.Label).ToList(),
                Values = values,
                TotalLabel = $"Tong {FormatCurrency(total)}",
                AverageLabel = $"Trung binh {FormatCurrency(values.Count > 0 ? total / values.Count : 0m)} / moc",
                PeakLabel = peakValue > 0m
                    ? $"Cao nhat {FormatCurrency(peakValue)} tai {peakLabel}"
                    : "Khong co doanh thu trong khoang nay"
            };
        }

        private static DashboardCountSeries BuildCountSeries(
            IReadOnlyList<DashboardBucket> buckets,
            IEnumerable<DashboardOrderFact> orderFacts,
            Func<DashboardOrderFact, DateTime> bucketSelector)
        {
            var facts = orderFacts.ToList();
            var values = buckets
                .Select(bucket => facts.Count(order => bucketSelector(order) == bucket.Start))
                .ToList();

            var total = values.Sum();
            var peakValue = values.Count > 0 ? values.Max() : 0;
            var peakIndex = values.FindIndex(value => value == peakValue);
            var peakLabel = peakIndex >= 0 ? buckets[peakIndex].Label : "Chua co du lieu";

            return new DashboardCountSeries
            {
                Labels = buckets.Select(bucket => bucket.Label).ToList(),
                Values = values,
                TotalLabel = $"Tong {FormatCount(total)} don",
                AverageLabel = $"Trung binh {(values.Count > 0 ? (decimal)total / values.Count : 0m).ToString("N1", VietnameseCulture)} / moc",
                PeakLabel = peakValue > 0
                    ? $"Cao nhat {FormatCount(peakValue)} don tai {peakLabel}"
                    : "Khong co don hang trong khoang nay"
            };
        }

        private static DashboardComparisonSeries BuildComparisonSeries(
            IReadOnlyList<DashboardBucket> buckets,
            IEnumerable<DashboardOrderFact> orderFacts,
            Func<DashboardOrderFact, DateTime> bucketSelector)
        {
            var facts = orderFacts.ToList();
            var dineInValues = buckets
                .Select(bucket => facts
                    .Where(order => bucketSelector(order) == bucket.Start && order.IsDineIn)
                    .Sum(order => order.Revenue))
                .ToList();

            var takeAwayValues = buckets
                .Select(bucket => facts
                    .Where(order => bucketSelector(order) == bucket.Start && !order.IsDineIn)
                    .Sum(order => order.Revenue))
                .ToList();

            var dineInTotal = dineInValues.Sum();
            var takeAwayTotal = takeAwayValues.Sum();
            var combinedTotal = dineInTotal + takeAwayTotal;

            var dineInShare = combinedTotal > 0m ? dineInTotal / combinedTotal * 100m : 0m;
            var takeAwayShare = combinedTotal > 0m ? takeAwayTotal / combinedTotal * 100m : 0m;

            return new DashboardComparisonSeries
            {
                Labels = buckets.Select(bucket => bucket.Label).ToList(),
                DineInValues = dineInValues,
                TakeAwayValues = takeAwayValues,
                DineInLabel = $"Tai cho {FormatCurrency(dineInTotal)}",
                TakeAwayLabel = $"Mang ve {FormatCurrency(takeAwayTotal)}",
                TotalLabel = $"Ty trong {dineInShare.ToString("N1", VietnameseCulture)}% / {takeAwayShare.ToString("N1", VietnameseCulture)}%"
            };
        }

        private static List<DashboardTopProductItem> BuildTopProductList(IEnumerable<DashboardProductFact> productFacts)
        {
            var facts = productFacts.ToList();
            var totalRevenue = facts.Sum(item => item.Revenue);

            return facts
                .GroupBy(item => item.ProductName)
                .Select(group => new DashboardTopProductItem
                {
                    Name = group.Key,
                    Quantity = group.Sum(item => item.Quantity),
                    Revenue = group.Sum(item => item.Revenue),
                    ShareLabel = totalRevenue > 0m
                        ? $"{(group.Sum(item => item.Revenue) / totalRevenue * 100m).ToString("N1", VietnameseCulture)}%"
                        : "0.0%"
                })
                .OrderByDescending(item => item.Quantity)
                .ThenByDescending(item => item.Revenue)
                .Take(5)
                .ToList();
        }

        private static IReadOnlyList<DashboardBucket> BuildDailyBuckets(DateTime anchorDate, int days)
        {
            var startDate = anchorDate.Date.AddDays(-(days - 1));
            return Enumerable.Range(0, days)
                .Select(index =>
                {
                    var bucketDate = startDate.AddDays(index);
                    return new DashboardBucket(bucketDate, bucketDate.ToString("dd/MM", VietnameseCulture));
                })
                .ToList();
        }

        private static IReadOnlyList<DashboardBucket> BuildMonthlyBuckets(DateTime anchorDate, int months)
        {
            var anchorMonth = new DateTime(anchorDate.Year, anchorDate.Month, 1);
            var startMonth = anchorMonth.AddMonths(-(months - 1));
            return Enumerable.Range(0, months)
                .Select(index =>
                {
                    var bucketDate = startMonth.AddMonths(index);
                    return new DashboardBucket(bucketDate, bucketDate.ToString("MM/yyyy", VietnameseCulture));
                })
                .ToList();
        }

        private static IReadOnlyList<DashboardBucket> BuildYearlyBuckets(DateTime anchorDate, int years)
        {
            var startYear = anchorDate.Year - (years - 1);
            return Enumerable.Range(0, years)
                .Select(index =>
                {
                    var bucketDate = new DateTime(startYear + index, 1, 1);
                    return new DashboardBucket(bucketDate, bucketDate.ToString("yyyy", VietnameseCulture));
                })
                .ToList();
        }

        private static DashboardMetricCard BuildCurrencyMetric(
            string label,
            decimal currentValue,
            decimal previousValue,
            string comparisonText,
            string hintText)
        {
            return new DashboardMetricCard
            {
                Label = label,
                Value = FormatCurrency(currentValue),
                TrendText = FormatSignedPercent(currentValue, previousValue),
                ComparisonText = comparisonText,
                TrendCssClass = GetTrendCssClass(currentValue, previousValue),
                HintText = hintText
            };
        }

        private static DashboardMetricCard BuildCountMetric(
            string label,
            int currentValue,
            int previousValue,
            string comparisonText,
            string hintText)
        {
            return new DashboardMetricCard
            {
                Label = label,
                Value = FormatCount(currentValue),
                TrendText = FormatSignedPercent(currentValue, previousValue),
                ComparisonText = comparisonText,
                TrendCssClass = GetTrendCssClass(currentValue, previousValue),
                HintText = hintText
            };
        }

        private static DashboardMetricCard BuildNeutralMetric(
            string label,
            string value,
            string trendText,
            string comparisonText,
            string hintText)
        {
            return new DashboardMetricCard
            {
                Label = label,
                Value = value,
                TrendText = trendText,
                ComparisonText = comparisonText,
                TrendCssClass = "is-neutral",
                HintText = hintText
            };
        }

        private static string BuildInitials(string fullName)
        {
            var parts = fullName
                .Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Take(2)
                .Select(part => part[0]);

            return string.Concat(parts).ToUpperInvariant();
        }

        private static decimal ResolveOrderRevenue(
            DashboardOrderSnapshot order,
            IReadOnlyDictionary<int, decimal> lineRevenueByOrder)
        {
            if (lineRevenueByOrder.TryGetValue(order.OrderId, out var lineRevenue) && lineRevenue > 0m)
            {
                return lineRevenue;
            }

            return order.StoredTotal;
        }

        private static decimal GetResolvedRevenue(DashboardLineSnapshot item)
        {
            if (item.LineTotal > 0m)
            {
                return item.LineTotal;
            }

            return item.Quantity * item.UnitPrice;
        }

        private static string FormatCurrency(decimal value)
        {
            return $"{value.ToString("N0", VietnameseCulture)}đ";
        }

        private static string FormatCount(int value)
        {
            return value.ToString("N0", VietnameseCulture);
        }

        private static string FormatSignedPercent(decimal currentValue, decimal previousValue)
        {
            var delta = CalculatePercentageChange(currentValue, previousValue);
            return $"{delta:+0.0;-0.0;0.0}%";
        }

        private static string GetTrendCssClass(decimal currentValue, decimal previousValue)
        {
            if (currentValue == previousValue)
            {
                return "is-neutral";
            }

            return currentValue > previousValue ? "is-up" : "is-down";
        }

        private static decimal CalculatePercentageChange(decimal currentValue, decimal previousValue)
        {
            if (previousValue == 0m)
            {
                return currentValue > 0m ? 100m : 0m;
            }

            return (currentValue - previousValue) / previousValue * 100m;
        }

        private sealed record DashboardOrderSnapshot(
            int OrderId,
            DateTime? OrderDate,
            bool IsDineIn,
            int? CustomerId,
            decimal StoredTotal);

        private sealed record DashboardLineSnapshot(
            int OrderId,
            DateTime? OrderDate,
            bool IsDineIn,
            int? CustomerId,
            int Quantity,
            decimal LineTotal,
            decimal UnitPrice,
            string ProductName);

        private sealed record DashboardOrderFact(
            DateTime OrderDate,
            bool IsDineIn,
            int? CustomerId,
            decimal Revenue);

        private sealed record DashboardProductFact(
            DateTime OrderDate,
            bool IsDineIn,
            string ProductName,
            int Quantity,
            decimal Revenue);

        private sealed record DashboardBucket(DateTime Start, string Label);
    }
}
