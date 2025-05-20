using Microsoft.AspNetCore.Mvc;

namespace Fruitful_Gifts.Models.ViewModels
{
    public class SanPhamViewModel
    {
        public Fruitful_Gifts.Database.SanPham SanPham { get; set; } = null!;
        public decimal GiaSauKhiGiam { get; set; }
    }
}

