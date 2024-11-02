using System.ComponentModel.DataAnnotations;

namespace WebQuanLyNhaHang.Models
{
    public class ProductConditions
    {
        [Key]
        public int ProductConditionId { get; set; }
        public int? ProductId { get; set; }
        public string? Condition { get; set; }
        public virtual Product? Product { get; set; }
    }
}
