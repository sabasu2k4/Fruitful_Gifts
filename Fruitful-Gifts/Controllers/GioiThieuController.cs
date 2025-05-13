using Fruitful_Gifts.Database;
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
            // Tất cả bài viết
            var baiVietList = _context.BaiViets
                .Where(bv => bv.IsHienThi == true)
                .OrderByDescending(bv => bv.CreatedAt)
                .ToList();

            // Lấy 5 bài viết mới nhất
            ViewBag.BaiVietMoi = baiVietList.Take(5).ToList();

            return View(baiVietList);
        }
    }
}
