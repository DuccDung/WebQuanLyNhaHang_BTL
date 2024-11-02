using Microsoft.AspNetCore.Mvc;
using WebQuanLyNhaHang.ViewModel;

namespace WebQuanLyNhaHang.Controllers
{
    public class ChartDataController : Controller
    {
       public IActionResult Index()
        {
            var chartData = new List<ChartData>
        {
            new ChartData { Name = "Jane", Data = new int[] { 3, 2, 1, 3, 4 } },
            new ChartData { Name = "John", Data = new int[] { 4, 3, 5, 7, 6 } },
            new ChartData { Name = "Joe", Data = new int[] { 3, 4, 2, 9, 5 } }
        };

            return View(chartData);
        }
    }


}
