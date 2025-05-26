using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Fruitful_Gifts.Models;
using Fruitful_Gifts.Database;
using System.Linq;

namespace Fruitful_Gifts.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class QuanLySanPhamController : Controller
    {
        private readonly FruitfulGiftsContext _context;

        public QuanLySanPhamController(FruitfulGiftsContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var products = _context.SanPhams
                //.Include(p => p.MaDmNavigation)
                .Include(p => p.MaNccNavigation)
                .ToList();

            return View(products);
        }


    }
}
