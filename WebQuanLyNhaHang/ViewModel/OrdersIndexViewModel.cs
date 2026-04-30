namespace WebQuanLyNhaHang.ViewModel;

public class OrdersIndexViewModel
{
    public string AdminDisplayName { get; set; } = "Admin";

    public string AdminAccount { get; set; } = "admin";

    public string AdminRoleLabel { get; set; } = "Quản trị vận hành";

    public string Initials { get; set; } = "AD";

    public List<OrderIndexRowViewModel> Orders { get; set; } = new();

    public int TotalOrders => Orders.Count;

    public int PendingOrders => Orders.Count(order => order.StatusKey == "pending");

    public int ProcessingOrders => Orders.Count(order =>
        order.StatusKey == "processing" ||
        order.StatusKey == "preparing" ||
        order.StatusKey == "shipping");

    public int CompletedOrders => Orders.Count(order =>
        order.StatusKey == "completed" ||
        order.StatusKey == "delivered");
}

public class OrderIndexRowViewModel
{
    public int OrderId { get; set; }

    public string OrderCode { get; set; } = string.Empty;

    public string CustomerName { get; set; } = "Khách lẻ";

    public string CustomerPhone { get; set; } = string.Empty;

    public string TableLabel { get; set; } = "Mang về";

    public DateTime? OrderTime { get; set; }

    public decimal TotalAmount { get; set; }

    public string PaymentLabel { get; set; } = "Tiền mặt";

    public string StatusKey { get; set; } = "pending";

    public string StatusLabel { get; set; } = "Chờ xử lý";

    public string StatusCssClass { get; set; } = "is-pending";

    public string SearchText { get; set; } = string.Empty;

    public string DateValue { get; set; } = string.Empty;
}
