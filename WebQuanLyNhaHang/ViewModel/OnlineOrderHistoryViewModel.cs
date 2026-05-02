using WebQuanLyNhaHang.Models;

namespace WebQuanLyNhaHang.ViewModel;

public sealed class OnlineOrderHistoryViewModel
{
    public List<OnlineOrderHistoryRowViewModel> Orders { get; set; } = new();
}

public sealed class OnlineOrderHistoryRowViewModel
{
    public int OrderId { get; set; }
    public string OrderCode { get; set; } = string.Empty;
    public DateTime? SubmittedAt { get; set; }
    public decimal TotalAmount { get; set; }
    public string StatusKey { get; set; } = OnlineOrderMetadata.StatusPending;
    public string StatusLabel { get; set; } = string.Empty;
    public string StatusCssClass { get; set; } = string.Empty;
    public string? RecipientName { get; set; }
    public string? Phone { get; set; }
    public string? Address { get; set; }
    public string? Note { get; set; }
    public List<OnlineOrderHistoryItemViewModel> Items { get; set; } = new();
}

public sealed class OnlineOrderHistoryItemViewModel
{
    public string ProductName { get; set; } = string.Empty;
    public string? ProductPhoto { get; set; }
    public int Quantity { get; set; }
    public decimal LineTotal { get; set; }
    public string? Note { get; set; }
}
