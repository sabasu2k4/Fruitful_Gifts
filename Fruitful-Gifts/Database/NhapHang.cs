using System;
using System.Collections.Generic;

namespace Fruitful_Gifts.Database;

public partial class NhapHang
{
    public int MaNhap { get; set; }

    public int? MaNcc { get; set; }

    public DateTime? NgayNhap { get; set; }

    public decimal? TongTien { get; set; }

    public decimal? PhiVanChuyenNhapHang { get; set; }

    public string? GhiChu { get; set; }

    public int? TrangThai { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual ICollection<ChiTietNhapHang> ChiTietNhapHangs { get; set; } = new List<ChiTietNhapHang>();

    public virtual NhaCungCap? MaNccNavigation { get; set; }
}
