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
        private (string Email, string HoTen) GetUserThongTin()
        {
            var email = HttpContext.Session.GetString("Email") ?? "";
            var hoTen = HttpContext.Session.GetString("HoTen") ?? "";
            return (email, hoTen);
        }

        public IActionResult Index()
        {
            var userInfo = GetUserThongTin();
            ViewBag.Email = userInfo.Email;
            ViewBag.HoTen = userInfo.HoTen;
           
            return View();
        }

        [HttpPost]
        public IActionResult Index(string HO_TEN, string EMAIL, string SDT, string NOI_DUNG)
        {
            var userInfo = GetUserThongTin();
            ViewBag.Email = userInfo.Email;
            ViewBag.HoTen = userInfo.HoTen;

            if (ModelState.IsValid)
            {
                var lienHe = new LienHe
                {
                    HoTen = HO_TEN,
                    Email = EMAIL,
                    Sdt = SDT,
                    NoiDung = NOI_DUNG,
                    ThoiGianGui = DateTime.Now,
                    TrangThai = false
                };

                _context.LienHes.Add(lienHe);
                _context.SaveChanges();

                TempData["GuiLienHeThanhCong"] = "Cảm ơn bạn đã liên hệ với chúng tôi!";
                return RedirectToAction("Index");
            }

            return View();
        }



    }
}
