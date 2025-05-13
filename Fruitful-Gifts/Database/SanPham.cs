using System;
using System.Collections.Generic;

namespace Fruitful_Gifts.Database;

public partial class SanPham
{
    public int MaSp { get; set; }

    public string? TenSp { get; set; }

    public int? MaDm { get; set; }

    public int? MaNcc { get; set; }

    public decimal? Gia { get; set; }

    public int? SoLuong { get; set; }

    public string? MoTa { get; set; }

    public string? Slug { get; set; }

    public bool? TrangThai { get; set; }

    public string? HinhAnh { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public bool? IsHienThi { get; set; }

    public virtual ICollection<BinhLuan> BinhLuans { get; set; } = new List<BinhLuan>();

    public virtual ICollection<ChiTietDonHang> ChiTietDonHangs { get; set; } = new List<ChiTietDonHang>();

    public virtual ICollection<ChiTietGioHang> ChiTietGioHangs { get; set; } = new List<ChiTietGioHang>();

    public virtual ICollection<KhuyenMai> KhuyenMais { get; set; } = new List<KhuyenMai>();

    public virtual DanhMuc? MaDmNavigation { get; set; }

    public virtual NhaCungCap? MaNccNavigation { get; set; }

    public virtual ICollection<SanPhamYeuThich> SanPhamYeuThiches { get; set; } = new List<SanPhamYeuThich>();
}
