using Fruitful_Gifts.Database;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Fruitful_Gifts.Controllers
{
    public class LayoutController : Controller
    {
        private readonly FruitfulGiftsContext _context;

        public LayoutController(FruitfulGiftsContext context)
        {
            _context = context;
        }
        public IActionResult Index()
        {
            var footers = _context.Footers.Where(f => f.TrangThai == 1).ToList();

            ViewBag.Footers = footers;

            return View();
        }
    }
}
