using System.Text.Json;
using System.Text.Json.Serialization;
using WebQuanLyNhaHang.Models;

namespace WebQuanLyNhaHang.ViewModel
{
    public class ViewModelGetFormBuy
    {
        private readonly QlnhaHangBtlContext _qlnhaHangBtlContext;

        public ViewModelGetFormBuy(QlnhaHangBtlContext qlnhaHangBtlContext)
        {
            _qlnhaHangBtlContext = qlnhaHangBtlContext;
        }

        public List<CTDH_Product> CTDH_Product(int BanID)
        {
            var rows = (from ban in _qlnhaHangBtlContext.Bans
                        join dh in _qlnhaHangBtlContext.DonHangs
                        on ban.BanId equals dh.BanId
                        join cthd in _qlnhaHangBtlContext.ChiTietHoaDons
                        on dh.DhId equals cthd.DhId
                        join product in _qlnhaHangBtlContext.Products
                        on cthd.ProductId equals product.ProductId
                        where ban.BanId == BanID && !ban.Remove && !dh.Remove && !cthd.Remove && !product.Remove
                        orderby dh.GioRa, dh.DhId, cthd.CthdId
                        select new
                        {
                            dh.DhId,
                            dh.GhiChu,
                            dh.TongTien,
                            product.PathPhoto,
                            cthd.SoLuong,
                            product.TenSanPham,
                            cthd.ThanhTien,
                            DonGia = product.GiaTien
                        })
                .ToList();

            return rows.Select(row =>
            {
                var customer = ResolveDineInCustomer(row.GhiChu);

                return new CTDH_Product
                {
                    DhId = row.DhId,
                    PathPhoto = row.PathPhoto,
                    SoLuong = row.SoLuong,
                    TenSanPham = row.TenSanPham,
                    ThanhTien = row.ThanhTien,
                    DonGia = row.DonGia,
                    TongTien = row.TongTien,
                    BanId = BanID,
                    CustomerGroupKey = customer.Id ?? $"order-{row.DhId}",
                    CustomerName = customer.Name
                };
            }).ToList();
        }

        private static DineInCustomerInfo ResolveDineInCustomer(string? value)
        {
            if (string.IsNullOrWhiteSpace(value) || !value.TrimStart().StartsWith("{", StringComparison.Ordinal))
            {
                return new DineInCustomerInfo(null, "Khách tại bàn");
            }

            try
            {
                var metadata = JsonSerializer.Deserialize<DineInOrderMetadata>(value);
                if (metadata == null || !string.Equals(metadata.Type, "dinein", StringComparison.OrdinalIgnoreCase))
                {
                    return new DineInCustomerInfo(null, "Khách tại bàn");
                }

                var guestName = metadata.GuestName;
                var guestId = metadata.GuestId;
                var name = string.IsNullOrWhiteSpace(guestName)
                    ? "Khách tại bàn"
                    : guestName.Trim();
                var id = string.IsNullOrWhiteSpace(guestId) ? null : guestId.Trim();

                return new DineInCustomerInfo(id, name);
            }
            catch (JsonException)
            {
                return new DineInCustomerInfo(null, "Khách tại bàn");
            }
        }

        private sealed record DineInCustomerInfo(string? Id, string Name);

        private sealed class DineInOrderMetadata
        {
            [JsonPropertyName("t")]
            public string? Type { get; set; }

            [JsonPropertyName("g")]
            public string? GuestId { get; set; }

            [JsonPropertyName("n")]
            public string? GuestName { get; set; }
        }
    }
}
