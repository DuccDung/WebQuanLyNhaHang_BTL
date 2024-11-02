using System;
using System.Collections.Generic;

namespace WebQuanLyNhaHang.Models;

public partial class PhanQuyen
{
    public int PqId { get; set; }

    public string? TenQuyen { get; set; }

    public virtual ICollection<NvPq> NvPqs { get; set; } = new List<NvPq>();
}
