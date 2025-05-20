using Fruitful_Gifts.Database;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;

namespace Fruitful_Gifts.Controllers
{
    public class TrangChuController : Controller
    {

        private readonly FruitfulGiftsContext _context;

        public TrangChuController(FruitfulGiftsContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var san_pham_hotdeal = _context.SanPhams
                .Include(sp => sp.MaDmNavigation)
                .Where(sp => sp.MaDmNavigation != null && sp.MaDmNavigation.DanhMucChaId == 1)
                .ToList();

            var qua_dip_le = _context.SanPhams
                .Include(sp => sp.MaDmNavigation)
                .Where(sp => sp.MaDmNavigation != null && sp.MaDmNavigation.DanhMucChaId == 1)
                .ToList();

            var qua_gia_dinh_ca_nha = _context.SanPhams
            .Include(sp => sp.MaDmNavigation)
            .Where(sp => sp.MaDmNavigation != null && sp.MaDmNavigation.DanhMucChaId == 2)
            .ToList();

            var qua_thuong_mai = _context.SanPhams
            .Include(sp => sp.MaDmNavigation)
            .Where(sp => sp.MaDmNavigation != null && sp.MaDmNavigation.DanhMucChaId == 3)
            .ToList();

            var baiVietList = _context.BaiViets
                .Where(bv => bv.Slug != "gioi-thieu-ve-chung-toi" && bv.IsHienThi == true)
                .OrderByDescending(bv => bv.CreatedAt);

            ViewBag.TinTuc = baiVietList.Take(8).ToList();

            var danhMucCons = _context.DanhMucs
                .Where(dm => dm.DanhMucChaId != null)
                .ToList();

            ViewBag.SanPham = san_pham_hotdeal;
            ViewBag.SanPham1 = qua_dip_le;
            ViewBag.SanPham2 = qua_gia_dinh_ca_nha;
            ViewBag.SanPham3 = qua_thuong_mai;
            ViewBag.DanhMuc = danhMucCons;

            return View();
        }

    }
}
