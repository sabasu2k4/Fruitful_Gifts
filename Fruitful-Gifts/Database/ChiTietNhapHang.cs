using System;
using System.Collections.Generic;

namespace Fruitful_Gifts.Database;

public partial class ChiTietNhapHang
{
    public int MaNhap { get; set; }

    public int MaSp { get; set; }

    public int? SoLuong { get; set; }

    public decimal? GiaNhap { get; set; }

    public virtual NhapHang MaNhapNavigation { get; set; } = null!;

    public virtual SanPham MaSpNavigation { get; set; } = null!;
}
