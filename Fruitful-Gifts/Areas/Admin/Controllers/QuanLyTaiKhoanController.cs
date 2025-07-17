using Fruitful_Gifts.Database;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Fruitful_Gifts.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class QuanLyTaiKhoanController : Controller
    {
        private readonly FruitfulGiftsContext _context;

        public QuanLyTaiKhoanController(FruitfulGiftsContext context)
        {
            _context = context;
        }
        [HttpGet]
        public IActionResult DangNhapAdmin()
        {
            return View();
        }
        //// 123456aA@
        [HttpPost]
        public IActionResult DangNhapAdmin(string tenDangNhap, string matKhau)
        {
            if (string.IsNullOrEmpty(tenDangNhap) || string.IsNullOrEmpty(matKhau))
            {
                ViewData["Error"] = "Vui lòng nhập đầy đủ thông tin.";
                return View();
            }

            var taiKhoan = _context.TaiKhoans
                .Include(tk => tk.NhanVien)
                .FirstOrDefault(tk => tk.TenDangNhap == tenDangNhap);

            if (taiKhoan == null || !BCrypt.Net.BCrypt.Verify(matKhau, taiKhoan.MatKhau))
            {
                ViewData["Error"] = "Tên đăng nhập hoặc mật khẩu không đúng.";
                return View();
            }

            if (taiKhoan.TrangThai == 0)
            {
                ViewData["Error"] = "Tài khoản đã bị khóa.";
                return View();
            }

            if (taiKhoan.VaiTro != "Admin" && taiKhoan.VaiTro != "NhanVien")
            {
                ViewData["Error"] = "Bạn không có quyền truy cập trang quản trị.";
                return View();
            }

            HttpContext.Session.SetInt32("TaiKhoanId", taiKhoan.TaiKhoanId);
            HttpContext.Session.SetString("VaiTro", taiKhoan.VaiTro);
            HttpContext.Session.SetString("UserName", taiKhoan.TenDangNhap);
            HttpContext.Session.SetString("Quyen", taiKhoan.Quyen);

            if (taiKhoan.NhanVien != null)
            {
                HttpContext.Session.SetInt32("MaNv", taiKhoan.NhanVien.MaNv);
                HttpContext.Session.SetString("TenNv", taiKhoan.NhanVien.TenNv);
                HttpContext.Session.SetString("ChucVu", taiKhoan.NhanVien.ChucVu);
            }

            return RedirectToAction("Index", "Home"); // chuyển sang trang admin chính
        }
        public IActionResult DangXuatAdmin()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("DangNhapAdmin", "QuanLyTaiKhoan");
        }
    }
}
