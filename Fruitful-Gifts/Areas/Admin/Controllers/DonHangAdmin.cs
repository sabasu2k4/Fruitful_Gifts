using Fruitful_Gifts.Database;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using ClosedXML.Excel;
using System.IO;
using Fruitful_Gifts.ViewModels;

namespace Fruitful_Gifts.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class DonHangAdminController : Controller
    {
        private readonly FruitfulGiftsContext _context;

        public DonHangAdminController(FruitfulGiftsContext context)
        {
            _context = context;
        }
        public async Task<IActionResult> ChiTietThanhToan(int id)
        {
            var donHang = await _context.DonHangs
                .Include(d => d.ThanhToans)
                .FirstOrDefaultAsync(d => d.MaDh == id);

            if (donHang == null)
            {
                return NotFound();
            }

            return View(donHang);
        }

        
        // Action xử lý tạo đơn hàng offline

       
    }



 
}