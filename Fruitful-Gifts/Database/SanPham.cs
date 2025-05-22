using System;
using System.Collections.Generic;

namespace Fruitful_Gifts.Database;

public partial class SanPham
{
    public int MaSp { get; set; }

    public string? TenSp { get; set; }

    public int? MaLoai { get; set; }

    public int? MaDm { get; set; }

    public int? MaNcc { get; set; }

    public int? MaXuatXu { get; set; }

    public int? MaChatLieu { get; set; }

    public int? MaDvt { get; set; }

    public decimal? Gia { get; set; }

    public int? SoLuong { get; set; }

    public string? MoTa { get; set; }

    public string? Slug { get; set; }

    public int? TrangThai { get; set; }

    public string? HinhAnh { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual ICollection<BinhLuan> BinhLuans { get; set; } = new List<BinhLuan>();

    public virtual ICollection<ChiTietDonHang> ChiTietDonHangs { get; set; } = new List<ChiTietDonHang>();

    public virtual ICollection<ChiTietGioHang> ChiTietGioHangs { get; set; } = new List<ChiTietGioHang>();

    public virtual ICollection<ChiTietNhapHang> ChiTietNhapHangs { get; set; } = new List<ChiTietNhapHang>();

    public virtual ICollection<ChiTietSanPham> ChiTietSanPhamMaSpChaNavigations { get; set; } = new List<ChiTietSanPham>();

    public virtual ICollection<ChiTietSanPham> ChiTietSanPhamMaSpConNavigations { get; set; } = new List<ChiTietSanPham>();

    public virtual ICollection<KhoHang> KhoHangs { get; set; } = new List<KhoHang>();

    public virtual ICollection<KhuyenMai> KhuyenMais { get; set; } = new List<KhuyenMai>();

    public virtual ChatLieu? MaChatLieuNavigation { get; set; }

    public virtual DanhMuc? MaDmNavigation { get; set; }

    public virtual DonViTinh? MaDvtNavigation { get; set; }

    public virtual LoaiSanPham? MaLoaiNavigation { get; set; }

    public virtual NhaCungCap? MaNccNavigation { get; set; }

    public virtual XuatXu? MaXuatXuNavigation { get; set; }

    public virtual ICollection<SanPhamYeuThich> SanPhamYeuThiches { get; set; } = new List<SanPhamYeuThich>();
}
