using Fruitful_Gifts.Database;
using Microsoft.AspNetCore.Mvc;

namespace Fruitful_Gifts.Models.ViewModels
{
    public class SanPhamViewModel
    {
        // === Dữ liệu Sản phẩm ===
        public SanPham? SanPham { get; set; }
        public int SoLuongTonKho { get; set; }
        public bool TrangThaiDangNhap { get; set; }
        public bool IsFavorite { get; set; }
        public bool DaMua { get; set; }
        public double TrungBinhSoSao { get; set; }
        public int SoLuotYeuThich { get; set; }
        public decimal GiaSauKhiGiam { get; set; }
        public List<SanPham>? SanPhamLienQuan { get; set; }

        // === Dữ liệu Giỏ quà ===
        public GioQua? GioQua { get; set; }
        public double GioQua_TBSoSao { get; set; }
        public bool IsFavorite_GioQua { get; set; }

        // Loại hiển thị: "sanpham" hoặc "gioqua"
        public string Loai { get; set; } = "sanpham";
    }
}
