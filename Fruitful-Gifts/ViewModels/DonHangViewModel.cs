using Microsoft.AspNetCore.Mvc;

namespace Fruitful_Gifts.ViewModels
{
    public class SanPhamTrongDonHangViewModel
    {
        public string? TenSp { get; set; }
        public int? SoLuong { get; set; }
        public decimal? TongTien { get; set; }
    }

    public class DonHangViewModel
    {
        public int MaDh { get; set; }
        public string? TenKhachHang { get; set; }
        public DateTime? NgayDatHang { get; set; }
        public decimal? TongTienDonHang { get; set; }
        public int? TrangThai { get; set; }
        public List<SanPhamTrongDonHangViewModel>? SanPhams { get; set; }
    }
}

