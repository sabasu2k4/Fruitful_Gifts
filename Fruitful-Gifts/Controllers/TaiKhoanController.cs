using Fruitful_Gifts.Database;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace Fruitful_Gifts.Controllers
{
    public class TaiKhoanController : Controller
    {
        private readonly FruitfulGiftsContext _context;

        public TaiKhoanController(FruitfulGiftsContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult DangKy()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> DangKy(KhachHang user)
        {
            if (!ModelState.IsValid)
                return View(user);

            if (_context.KhachHangs.Any(u => u.Email == user.Email))
            {
                ModelState.AddModelError("Email", "Email đã được đăng ký.");
                return View(user);
            }

            user.MatKhau = BCrypt.Net.BCrypt.HashPassword(user.MatKhau);

            _context.KhachHangs.Add(user);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Đăng ký thành công! Bạn có thể đăng nhập ngay.";

            //return RedirectToAction("DangNhap", "TaiKhoan");
            return RedirectToAction("DangKy", "TaiKhoan");
        }

        [HttpGet]
        public IActionResult DangNhap()
        {
            return View();
        }

        [HttpPost]
        public IActionResult DangNhap(KhachHang u)
        {
            var loginName = Request.Form["LoginName"].ToString();

            if (string.IsNullOrEmpty(loginName) || string.IsNullOrEmpty(u.MatKhau))
            {
                ViewData["LoginNameError"] = "Vui lòng nhập tên đăng nhập hoặc email";
                if (string.IsNullOrEmpty(u.MatKhau))
                    ModelState.AddModelError("MatKhau", "Vui lòng nhập mật khẩu");
                return View(u);
            }

            var user = _context.KhachHangs.FirstOrDefault(kh =>
              kh.TenNguoiDung == loginName || kh.Email == loginName);

            if (user != null)
            {
                // Kiểm tra mật khẩu sử dụng BCrypt
                bool passwordMatch = BCrypt.Net.BCrypt.Verify(u.MatKhau, user.MatKhau);

                if (passwordMatch)
                {
                    // Lưu thông tin user vào session
                    var userJson = JsonSerializer.Serialize(user);
                    HttpContext.Session.SetString("user", userJson);
                    HttpContext.Session.SetString("TenNguoiDung", user.TenNguoiDung);
                    HttpContext.Session.SetInt32("MaKh", user.MaKh); // thêm MaKh nếu cần

                    TempData["SuccessMessage"] = "Đăng nhập thành công!";
                    return RedirectToAction("Index", "TrangChu");
                }
            }
            ViewData["LoginNameError"] = "Tên đăng nhập hoặc mật khẩu không đúng.";
            return View(u);
        }

        public IActionResult DangXuat()
        {
            HttpContext.Session.Clear();
            TempData["SuccessMessage"] = "Đăng xuất thành công!";
            return RedirectToAction("Index", "TrangChu");
        }

    }
}
