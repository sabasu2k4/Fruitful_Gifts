using System;
using System.Collections.Generic;

namespace Fruitful_Gifts.Database;

public partial class NhanVien
{
    public int MaNv { get; set; }

    public string? TenNv { get; set; }

    public string? Sdt { get; set; }

    public string? Email { get; set; }

    public int? TaiKhoanId { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual ICollection<DonHang> DonHangs { get; set; } = new List<DonHang>();

    public virtual ICollection<Luong> Luongs { get; set; } = new List<Luong>();

    public virtual TaiKhoan? TaiKhoan { get; set; }
}
