using Microsoft.AspNetCore.Mvc;

namespace Fruitful_Gifts.ViewModels
{
    public class SanPhamTrongDonHangViewModel
    {
        public string? TenSp { get; set; }
        public int? SoLuong { get; set; }
        public decimal? TongTien { get; set; }
        public string? HinhAnh { get; set; }
        public string? Loai { get; set; }
        public decimal? DonGia { get; set; } 
        public int? SoLuongTon { get; set; }
    }

    public class DonHangViewModel
    {
        public int MaDh { get; set; }
        public string TenKhachHang { get; set; }
        public DateTime? NgayDatHang { get; set; }
        public decimal? TongTienDonHang { get; set; }

        public decimal? PhiVanChuyen { get; set; } 

        public int? TrangThai { get; set; }
        public int? TrangThaiThanhToan { get; set; }
        public string? PhuongThucThanhToan { get; set; }
        public string? DiaChiNhanHang { get; set; }
        public string? SoDienThoai { get; set; }

        public string? GhiChu { get; set; } 

        public List<SanPhamTrongDonHangViewModel> SanPhams { get; set; }
    }
}


