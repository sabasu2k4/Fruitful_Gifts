using System;
using System.Collections.Generic;

namespace Fruitful_Gifts.Database;

public partial class DonHang
{
    public int MaDh { get; set; }

    public int? MaKh { get; set; }

    public int? MaNv { get; set; }

    public DateTime? NgayDatHang { get; set; }

    public decimal? TongTienDonHang { get; set; }

    public decimal? PhiVanChuyenBanHang { get; set; }

    public int? TrangThai { get; set; }

    public int? TrangThaiThanhToan { get; set; }

    public string? DiaChiNhanHang { get; set; }

    public string? PhuongThucBan { get; set; }

    public int? MaPt { get; set; }

    public string? GhiChu { get; set; }

    public string? SoDienThoai { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual ICollection<BinhLuan> BinhLuans { get; set; } = new List<BinhLuan>();

    public virtual ICollection<ChiTietDonHang> ChiTietDonHangs { get; set; } = new List<ChiTietDonHang>();

    public virtual KhachHang? MaKhNavigation { get; set; }

    public virtual NhanVien? MaNvNavigation { get; set; }

    public virtual PhuongThucThanhToan? MaPtNavigation { get; set; }

    public virtual ICollection<ThanhToan> ThanhToans { get; set; } = new List<ThanhToan>();

    public virtual TrangThaiDonHang? TrangThaiNavigation { get; set; }
}
