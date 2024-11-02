using System;
using System.Collections.Generic;

namespace WebQuanLyNhaHang.Models;

public partial class NgayCong
{
    public int NcId { get; set; }

    public DateOnly? NgayCong1 { get; set; }

    public string? CaLam { get; set; }

    public virtual ICollection<NvNc> NvNcs { get; set; } = new List<NvNc>();
}
