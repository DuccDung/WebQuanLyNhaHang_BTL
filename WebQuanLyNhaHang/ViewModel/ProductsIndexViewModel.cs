using Microsoft.AspNetCore.Mvc.Rendering;
using WebQuanLyNhaHang.Models;

namespace WebQuanLyNhaHang.ViewModel;

public class ProductsIndexViewModel
{
    public List<Product> Products { get; set; } = new();

    public Product CreateProduct { get; set; } = new();

    public List<SelectListItem> CategoryOptions { get; set; } = new();

    public bool OpenCreateModal { get; set; }
}
