namespace WebQuanLyNhaHang.ViewModel;

public class CustomersIndexViewModel
{
    public string AdminDisplayName { get; set; } = "Admin";

    public string AdminAccount { get; set; } = "admin";

    public string AdminRoleLabel { get; set; } = "Quản trị vận hành";

    public string Initials { get; set; } = "AD";

    public List<CustomerIndexRowViewModel> Customers { get; set; } = new();

    public int TotalCustomers => Customers.Count;

    public int VipCustomers => Customers.Count(customer => customer.TierKey == "vip");

    public int RegularCustomers => Customers.Count(customer => customer.TierKey == "regular");

    public int NewCustomers => Customers.Count(customer => customer.TierKey == "new");
}

public class CustomerIndexRowViewModel
{
    public int CustomerId { get; set; }

    public string CustomerCode { get; set; } = string.Empty;

    public string CustomerName { get; set; } = "Khách lẻ";

    public string Initial { get; set; } = "K";

    public string Phone { get; set; } = "Chưa có SĐT";

    public string Email { get; set; } = "Chưa có email";

    public string Address { get; set; } = "Chưa cập nhật địa chỉ";

    public DateTime? LastPurchase { get; set; }

    public int OrderCount { get; set; }

    public decimal TotalSpent { get; set; }

    public string TierKey { get; set; } = "new";

    public string TierLabel { get; set; } = "Mới";

    public string TierCssClass { get; set; } = "is-new";

    public string SearchText { get; set; } = string.Empty;
}
