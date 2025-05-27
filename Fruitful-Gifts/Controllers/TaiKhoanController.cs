using Fruitful_Gifts.Database;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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
        public IActionResult DangKy(KhachHang kh)
        {
            if (kh.TaiKhoan == null || string.IsNullOrEmpty(kh.TaiKhoan.TenDangNhap) || string.IsNullOrEmpty(kh.TaiKhoan.MatKhau))
            {
                ModelState.AddModelError("TaiKhoan.TenDangNhap", "Vui lòng nhập đầy đủ tên đăng nhập");
                ModelState.AddModelError("TaiKhoan.MatKhau", "Vui lòng nhập mật khẩu");
                return View(kh);
            }

            string tenDangNhap = kh.TaiKhoan.TenDangNhap;
            string matKhau = kh.TaiKhoan.MatKhau;
            string email = kh.Email;

            bool exist = _context.TaiKhoans.Any(tk => tk.TenDangNhap == tenDangNhap);
            if (exist)
            {
                ModelState.AddModelError("TaiKhoan.TenDangNhap", "Tên đăng nhập đã tồn tại");
                return View(kh);
            }

            // Kiểm tra email đã tồn tại (dựa vào bảng KhachHang)
            bool existEmail = _context.KhachHangs.Any(k => k.Email == email);
            if (existEmail)
            {
                ModelState.AddModelError("Email", "Email đã được sử dụng");
                return View(kh);
            }

            string hashedPassword = BCrypt.Net.BCrypt.HashPassword(matKhau);

            var taiKhoan = new TaiKhoan
            {
                TenDangNhap = tenDangNhap,
                MatKhau = hashedPassword,
                VaiTro = "KhachHang",
                TrangThai = 1
            };

            // GÁN trực tiếp tài khoản cho khách hàng
            kh.TaiKhoan = taiKhoan;

            _context.KhachHangs.Add(kh);
            _context.SaveChanges(); 

            TempData["SuccessMessage"] = "Đăng ký thành công! Vui lòng đăng nhập.";
            return RedirectToAction("DangNhap");
        }


        [HttpGet]
        public IActionResult DangNhap()
        {
            return View();
        }

        [HttpPost]
        public IActionResult DangNhap(string tenDangNhap, string matKhau)
        {
            if (string.IsNullOrEmpty(tenDangNhap) || string.IsNullOrEmpty(matKhau))
            {
                ViewData["Error"] = "Vui lòng nhập đầy đủ tên đăng nhập và mật khẩu";
                return View();
            }

            var taiKhoan = _context.TaiKhoans
                .Include(tk => tk.KhachHang)
                .FirstOrDefault(tk => tk.TenDangNhap == tenDangNhap
                                   || (tk.KhachHang != null && tk.KhachHang.Email == tenDangNhap));

            if (taiKhoan == null || !BCrypt.Net.BCrypt.Verify(matKhau, taiKhoan.MatKhau))
            {
                ViewData["Error"] = "Tên đăng nhập hoặc mật khẩu không đúng";
                return View();
            }

            if (taiKhoan.TrangThai == 0)
            {
                ViewData["Error"] = "Tài khoản đã bị khóa";
                return View();
            }

            if (taiKhoan.VaiTro != "KhachHang")
            {
                ViewData["Error"] = "Tài khoản không hợp lệ";
                return View();
            }

            HttpContext.Session.SetInt32("TaiKhoanId", taiKhoan.TaiKhoanId);
            HttpContext.Session.SetString("VaiTro", taiKhoan.VaiTro);
            HttpContext.Session.SetString("UserName", taiKhoan.TenDangNhap);

            if (taiKhoan.KhachHang != null)
            {
                HttpContext.Session.SetInt32("MaKh", taiKhoan.KhachHang.MaKh);
            }

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
