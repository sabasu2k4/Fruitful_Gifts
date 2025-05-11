using System;
using System.Collections.Generic;

namespace Fruitful_Gifts.Database;

public partial class KhachHang
{
    public int MaKh { get; set; }

    public string? HoKh { get; set; }

    public string? TenKh { get; set; }

    public string? GioiTinh { get; set; }

    public string? Email { get; set; }

    public string? Sdt { get; set; }

    public string? DiaChi { get; set; }

    public string? TenNguoiDung { get; set; }

    public string? MatKhau { get; set; }

    public bool? TrangThai { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual ICollection<BinhLuan> BinhLuans { get; set; } = new List<BinhLuan>();

    public virtual ICollection<ChiTietGioHang> ChiTietGioHangs { get; set; } = new List<ChiTietGioHang>();

    public virtual ICollection<DonHang> DonHangs { get; set; } = new List<DonHang>();

    public virtual ICollection<SanPham> MaSps { get; set; } = new List<SanPham>();
}
