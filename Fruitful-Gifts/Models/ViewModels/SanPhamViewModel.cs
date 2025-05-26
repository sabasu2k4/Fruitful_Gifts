using Fruitful_Gifts.Database;
using Microsoft.AspNetCore.Mvc;

namespace Fruitful_Gifts.Models.ViewModels
{
    public class SanPhamViewModel
    {
        public SanPham SanPham { get; set; }
        public int SoLuongTonKho { get; set; } 
        public bool TrangThaiDangNhap { get; set; }
        public bool IsFavorite { get; set; }
        public bool DaMua { get; set; }
        public double TrungBinhSoSao { get; set; }
        public int SoLuotYeuThich { get; set; }
        public decimal GiaSauKhiGiam { get; set; }
        public List<SanPham> SanPhamLienQuan { get; set; }
    }


}

