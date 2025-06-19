using System;
using System.Collections.Generic;

namespace Fruitful_Gifts.Database;

public partial class GioQua
{
    public int MaGq { get; set; }

    public string TenGioQua { get; set; } = null!;

    public decimal GiaBan { get; set; }

    public string? MoTa { get; set; }

    public string? HinhAnh { get; set; }

    public string? Slug { get; set; }

    public int? TrangThai { get; set; }

    public int? MaDm { get; set; }

    public virtual ICollection<BinhLuan> BinhLuans { get; set; } = new List<BinhLuan>();

    public virtual ICollection<ChiTietDonHang> ChiTietDonHangs { get; set; } = new List<ChiTietDonHang>();

    public virtual ICollection<ChiTietGioHang> ChiTietGioHangs { get; set; } = new List<ChiTietGioHang>();

    public virtual ICollection<ChiTietGioQua> ChiTietGioQuas { get; set; } = new List<ChiTietGioQua>();

    public virtual ICollection<KhuyenMai> KhuyenMais { get; set; } = new List<KhuyenMai>();

    public virtual DanhMucGioQua? MaDmNavigation { get; set; }

    public virtual ICollection<SanPhamYeuThich> SanPhamYeuThiches { get; set; } = new List<SanPhamYeuThich>();
}
