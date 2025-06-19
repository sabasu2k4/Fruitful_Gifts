using System;
using System.Collections.Generic;

namespace Fruitful_Gifts.Database;

public partial class BinhLuan
{
    public int IdBinhLuan { get; set; }

    public int MaKh { get; set; }

    public int? MaSp { get; set; }

    public int? MaGq { get; set; }

    public int? SoSao { get; set; }

    public string? NoiDung { get; set; }

    public DateTime? NgayBinhLuan { get; set; }

    public int? TrangThai { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public int? MaDh { get; set; }

    public virtual DonHang? MaDhNavigation { get; set; }

    public virtual GioQua? MaGqNavigation { get; set; }

    public virtual KhachHang MaKhNavigation { get; set; } = null!;

    public virtual SanPham? MaSpNavigation { get; set; }
}
