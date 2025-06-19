using Microsoft.AspNetCore.Mvc;

namespace Fruitful_Gifts.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            var vaiTro = HttpContext.Session.GetString("VaiTro");
            if (vaiTro != "Admin" && vaiTro != "NhanVien")
            {
                return RedirectToAction("DangNhapAdmin", "TaiKhoan");
            }
            ViewBag.UserName = HttpContext.Session.GetString("UserName");
            return View();
        }
    }
}
