﻿using Fruitful_Gifts.Database;
using Microsoft.AspNetCore.Mvc;

namespace Fruitful_Gifts.Controllers
{
    public class GioiThieuController : Controller
    {
        private readonly FruitfulGiftsContext _context;

        public GioiThieuController(FruitfulGiftsContext context)
        {
            _context = context;
        }
        public IActionResult Index()
        {
            var gioiThieu = _context.BaiViets
                .FirstOrDefault(bv => bv.Slug == "gioi-thieu-ve-chung-toi" && bv.TrangThai == 1);

            var baiVietList = _context.BaiViets
                .Where(bv => bv.Slug != "gioi-thieu-ve-chung-toi" && bv.TrangThai == 1)
                .OrderByDescending(bv => bv.CreatedAt)
                .ToList();

            // Lấy 5 bài viết mới nhất từ danh sách đã lọc
            ViewBag.BaiVietMoi = baiVietList.Take(5).ToList();

            ViewBag.BaiVietGioiThieu = gioiThieu;

            return View(baiVietList);
        }

    }
}
