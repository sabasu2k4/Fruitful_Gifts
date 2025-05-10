using Fruitful_Gifts.Database;
using Microsoft.AspNetCore.Mvc;

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
            var sp = _context.SanPhams
                        .Where(sp => sp.MaDm == 1)
                        .ToList();
            var sp1 = _context.SanPhams
            .Where(sp => sp.MaDm == 2)
            .ToList();

            var sp2 = _context.SanPhams
            .Where(sp => sp.MaDm == 3)
            .ToList();

            var sp3 = _context.SanPhams
            .Where(sp => sp.MaDm == 4)
            .ToList();

            var dm = _context.DanhMucs.ToList();

            ViewBag.SanPham = sp;
            ViewBag.SanPham1 = sp1;
            ViewBag.SanPham2 = sp2;
            ViewBag.SanPham3 = sp3;
            ViewBag.DanhMuc = dm;

            return View();
        }
    }
}
