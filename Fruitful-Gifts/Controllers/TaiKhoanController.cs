using Fruitful_Gifts.Database;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
            var model = new KhachHang();
            model.TaiKhoan = new TaiKhoan(); // khởi tạo để binding hoạt động
            return View(model);
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

            if (user.TaiKhoan == null)
            {
                ModelState.AddModelError("", "Thông tin tài khoản không hợp lệ.");
                return View(user);
            }

            // Hash mật khẩu
            user.TaiKhoan.MatKhau = BCrypt.Net.BCrypt.HashPassword(user.TaiKhoan.MatKhau);
            user.TaiKhoan.VaiTro = "KhachHang";
            user.TaiKhoan.TrangThai = 1;

            _context.KhachHangs.Add(user);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Đăng ký thành công! Bạn có thể đăng nhập ngay.";
            return RedirectToAction("DangNhap", "TaiKhoan");
        }


        [HttpGet]
        public IActionResult DangNhap()
        {
            return View();
        }
        [HttpPost]
        public IActionResult DangNhap(string TenDangNhap, string MatKhau)
        {
            if (string.IsNullOrEmpty(TenDangNhap) || string.IsNullOrEmpty(MatKhau))
            {
                ViewData["LoginError"] = "Vui lòng nhập đủ thông tin đăng nhập.";
                return View();
            }

            var user = _context.TaiKhoans.FirstOrDefault(u => u.TenDangNhap == TenDangNhap && u.MatKhau == MatKhau && u.TrangThai == 1);
            if (user == null)
            {
                ViewData["LoginError"] = "Tên đăng nhập hoặc mật khẩu không đúng.";
                return View();
            }

            // Lưu thông tin đăng nhập vào session
            HttpContext.Session.SetInt32("TaiKhoanId", user.TaiKhoanId);
            HttpContext.Session.SetString("TenDangNhap", user.TenDangNhap);
            HttpContext.Session.SetString("VaiTro", user.VaiTro);

            return RedirectToAction("Index", "TrangChu");
        }



        public IActionResult DangXuat()
        {
            HttpContext.Session.Clear();
            TempData["SuccessMessage"] = "Đăng xuất thành công!";
            return RedirectToAction("Index", "TrangChu");
        }

    }
}
