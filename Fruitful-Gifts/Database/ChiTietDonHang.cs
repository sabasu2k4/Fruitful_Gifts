using System;
using System.Collections.Generic;

namespace Fruitful_Gifts.Database;

public partial class ChiTietDonHang
{
    public int MaDh { get; set; }

    public int MaSp { get; set; }

    public int? SoLuong { get; set; }

    public decimal? TongTienTungSanPham { get; set; }

    public virtual DonHang MaDhNavigation { get; set; } = null!;

    public virtual SanPham MaSpNavigation { get; set; } = null!;
}