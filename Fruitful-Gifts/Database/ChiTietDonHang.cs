using System;
using System.Collections.Generic;

namespace Fruitful_Gifts.Database;

public partial class ChiTietDonHang
{
    public int Id { get; set; }

    public int MaDh { get; set; }

    public int? MaSp { get; set; }

    public int? MaGq { get; set; }

    public decimal? SoLuong { get; set; }

    public decimal? GiaBan { get; set; }

    public decimal? TongTienTungSanPham { get; set; }

    public virtual DonHang MaDhNavigation { get; set; } = null!;

    public virtual GioQua? MaGqNavigation { get; set; }

    public virtual SanPham? MaSpNavigation { get; set; }
}
