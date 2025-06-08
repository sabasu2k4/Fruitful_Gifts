using System;
using System.Collections.Generic;

namespace Fruitful_Gifts.Database;

public partial class ChiTietGioHang
{
    public int Id { get; set; }

    public int MaKh { get; set; }

    public int? MaSp { get; set; }

    public int? MaGq { get; set; }

    public int? SoLuong { get; set; }

    public int? TrangThai { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual GioQua? MaGqNavigation { get; set; }

    public virtual KhachHang MaKhNavigation { get; set; } = null!;

    public virtual SanPham? MaSpNavigation { get; set; }
}
