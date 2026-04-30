using WebQuanLyNhaHang.Models;

namespace WebQuanLyNhaHang.ViewModel
{
    public sealed class TrangChuMenuPageViewModel
    {
        public string Title { get; init; } = string.Empty;

        public string Subtitle { get; init; } = string.Empty;

        public IReadOnlyList<Category> Categories { get; init; } = Array.Empty<Category>();

        public IReadOnlyDictionary<int, IReadOnlyList<Product>> ProductsByCategory { get; init; }
            = new Dictionary<int, IReadOnlyList<Product>>();
    }
}
