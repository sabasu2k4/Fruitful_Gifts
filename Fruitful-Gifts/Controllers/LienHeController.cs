using Fruitful_Gifts.Database;
using Microsoft.AspNetCore.Mvc;

namespace Fruitful_Gifts.Controllers
{
    public class LienHeController : Controller
    {
        private readonly FruitfulGiftsContext _context;

        public LienHeController(FruitfulGiftsContext context)
        {
            _context = context;
        }
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Index(string HO_TEN, string EMAIL, string SDT, string NOI_DUNG)
        {
            if (ModelState.IsValid)
            {
                var lienHe = new LienHe
                {
                    HoTen = HO_TEN,
                    Email = EMAIL,
                    Sdt = SDT,
                    NoiDung = NOI_DUNG,
                    ThoiGianGui = DateTime.Now,
                    TrangThai = false  // Mặc định đang xử lý
                };

                _context.LienHes.Add(lienHe);
                _context.SaveChanges();

                // Lưu thông báo thành công vào TempData
                TempData["GuiLienHeThanhCong"] = "Cảm ơn bạn đã liên hệ với chúng tôi!";
                return RedirectToAction("Index");
            }

            // Nếu dữ liệu không hợp lệ, hiển thị lại form
            return View();
        }

    }
}
