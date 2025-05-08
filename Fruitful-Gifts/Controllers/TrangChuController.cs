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
            var dm = _context.DanhMucs.ToList();

            ViewBag.SanPham = sp;
            ViewBag.DanhMuc = dm;

            return View();
        }
    }
}
