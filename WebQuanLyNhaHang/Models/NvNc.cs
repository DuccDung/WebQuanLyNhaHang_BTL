using System;
using System.Collections.Generic;

namespace WebQuanLyNhaHang.Models;

public partial class NvNc
{
    public int NvncId { get; set; }

    public int? NvId { get; set; }

    public int? NcId { get; set; }

    public bool KiemTra { get; set; }

    public virtual NgayCong? Nc { get; set; }

    public virtual NhanVien? Nv { get; set; }
}
