using System;
using System.Collections.Generic;

namespace Fruitful_Gifts.Database;

public partial class ChiTietGioQua
{
    public int MaGq { get; set; }

    public int MaSp { get; set; }

    public double SoLuong { get; set; }

    public virtual GioQua MaGqNavigation { get; set; } = null!;

    public virtual SanPham MaSpNavigation { get; set; } = null!;
}
