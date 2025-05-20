using System;
using System.Collections.Generic;

namespace Fruitful_Gifts.Database;

public partial class BinhLuan
{
    public int IdBinhLuan { get; set; }

    public int? MaSp { get; set; }

    public int? MaKh { get; set; }

    public int? SoSao { get; set; }

    public string? NoiDung { get; set; }

    public DateTime? Ngay { get; set; }

    public bool? TrangThai { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public bool? IsHienThi { get; set; }

    public virtual KhachHang? MaKhNavigation { get; set; }

    public virtual SanPham? MaSpNavigation { get; set; }
}
