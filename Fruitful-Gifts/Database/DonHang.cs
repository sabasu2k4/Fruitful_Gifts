using System;
using System.Collections.Generic;

namespace Fruitful_Gifts.Database;

public partial class DonHang
{
    public int MaDh { get; set; }

    public int? MaKh { get; set; }

    public DateTime? NgayDatHang { get; set; }

    public decimal? TongTienDonHang { get; set; }

    public int? TrangThai { get; set; }

    public int? TrangThaiThanhToan { get; set; }

    public string? DiaChiNhanHang { get; set; }

    public string? PhuongThucBan { get; set; }

    public int? MaPt { get; set; }

    public string? GhiChu { get; set; }

    public string? SoDienThoai { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual ICollection<ChiTietDonHang> ChiTietDonHangs { get; set; } = new List<ChiTietDonHang>();

    public virtual KhachHang? MaKhNavigation { get; set; }

    public virtual PhuongThucThanhToan? MaPtNavigation { get; set; }
}