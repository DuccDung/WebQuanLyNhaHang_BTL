using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Text.RegularExpressions;
using WebQuanLyNhaHang.Models;

namespace WebQuanLyNhaHang.ViewModel
{
    public class ViewModelBan
    {
        private static readonly CultureInfo VietnameseCulture = new("vi-VN");
        private readonly QlnhaHangBtlContext _context;

        public string AdminDisplayName { get; set; } = "Admin";

        public string AdminAccount { get; set; } = "admin";

        public string AdminRoleLabel { get; set; } = "Quản trị vận hành";

        public string Initials { get; set; } = "AD";

        public ViewModelBan(QlnhaHangBtlContext context)
        {
            _context = context;
        }

        public List<Ban> RenBan()
        {
            return _context.Bans
                .AsNoTracking()
                .Where(item => !item.Remove)
                .OrderBy(item => item.BanId)
                .ToList();
        }

        public int DonHangByBan(int id)
        {
            return _context.DonHangs.Count(item => item.BanId == id && !item.Remove);
        }

        public List<TableCardViewModel> TableCards()
        {
            var activeOrders = _context.DonHangs
                .AsNoTracking()
                .Where(order => !order.Remove && order.BanId.HasValue)
                .GroupBy(order => order.BanId!.Value)
                .Select(group => new
                {
                    TableId = group.Key,
                    OrderCount = group.Count(),
                    LatestTime = group.Max(order => order.GioVao ?? order.GioRa),
                    TotalAmount = group.Sum(order => order.TongTien ?? 0m)
                })
                .ToDictionary(order => order.TableId);

            return RenBan()
                .Select(table =>
                {
                    activeOrders.TryGetValue(table.BanId, out var order);

                    var isBusy = order != null && order.OrderCount > 0;
                    var isReserved = !isBusy && IsReserved(table.GhiChu);

                    return new TableCardViewModel
                    {
                        TableId = table.BanId,
                        Seats = table.SoChoNgoi ?? 0,
                        StatusLabel = isBusy ? "Đang Có Đơn Hàng" : isReserved ? "Đã Đặt" : "Trống",
                        StatusClass = isBusy ? "busy" : isReserved ? "reserved" : "empty",
                        CardClass = isBusy ? "is-busy" : isReserved ? "is-reserved" : "is-empty",
                        TimeLabel = isBusy ? FormatTime(order?.LatestTime) : ExtractTime(table.GhiChu),
                        AmountLabel = isBusy ? FormatShortMoney(order?.TotalAmount ?? 0m) : string.Empty,
                        HasActiveOrder = isBusy
                    };
                })
                .ToList();
        }

        private static bool IsReserved(string? note)
        {
            if (string.IsNullOrWhiteSpace(note))
            {
                return false;
            }

            var normalizedNote = note.Trim().ToLower(VietnameseCulture);
            return normalizedNote.Contains("đặt")
                || normalizedNote.Contains("dat")
                || normalizedNote.Contains("reserved");
        }

        private static string ExtractTime(string? note)
        {
            if (string.IsNullOrWhiteSpace(note))
            {
                return string.Empty;
            }

            var match = Regex.Match(note, @"\b([01]?\d|2[0-3]):[0-5]\d\b");
            return match.Success ? match.Value : string.Empty;
        }

        private static string FormatTime(DateTime? time)
        {
            return time.HasValue ? time.Value.ToString("HH:mm", VietnameseCulture) : string.Empty;
        }

        private static string FormatShortMoney(decimal amount)
        {
            if (amount <= 0)
            {
                return string.Empty;
            }

            if (amount >= 1000m)
            {
                return $"{Math.Round(amount / 1000m, MidpointRounding.AwayFromZero).ToString("N0", VietnameseCulture)}k";
            }

            return amount.ToString("N0", VietnameseCulture);
        }
    }

    public class TableCardViewModel
    {
        public int TableId { get; set; }
        public int Seats { get; set; }
        public string StatusLabel { get; set; } = string.Empty;
        public string StatusClass { get; set; } = string.Empty;
        public string CardClass { get; set; } = string.Empty;
        public string TimeLabel { get; set; } = string.Empty;
        public string AmountLabel { get; set; } = string.Empty;
        public bool HasActiveOrder { get; set; }
    }
}
