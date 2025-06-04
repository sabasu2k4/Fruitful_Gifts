using Fruitful_Gifts.Database;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.CodeAnalysis.Elfie.Diagnostics;
namespace Fruitful_Gifts.Controllers
{
    public class KhachHangController : Controller
    {
        private readonly FruitfulGiftsContext _context;
        private readonly ILogger<KhachHangController> _logger;
        public KhachHangController(FruitfulGiftsContext context, ILogger<KhachHangController> logger)
        {
            _context = context;
            _logger = logger;
        }
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Profile()
        {
            var taiKhoanId = HttpContext.Session.GetInt32("TaiKhoanId");

            if (taiKhoanId == null)
            {
                return RedirectToAction("Index", "TrangChu");
            }

            var khachHang = _context.KhachHangs.FirstOrDefault(kh => kh.TaiKhoanId == taiKhoanId);

            if (khachHang != null)
            {
                return View(khachHang);
            }

            return View();
        }

        public IActionResult EditProfile()
        {
            var khachHangJson = HttpContext.Session.GetString("user");

            if (string.IsNullOrEmpty(khachHangJson))
            {
                return RedirectToAction("Index", "TrangChu");
            }

            var thongTinkhachHang = JsonSerializer.Deserialize<KhachHang>(khachHangJson);

            if (thongTinkhachHang != null)
            {
                var customer = _context.KhachHangs.FirstOrDefault(c => c.MaKh == thongTinkhachHang.MaKh);

                if (customer != null)
                {
                    return View(customer);
                }
            }


            return View();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditProfile([Bind("HoKH,TenKH,gioiTinh,Email,SDT,DiaChi")] KhachHangEditModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Vui lòng kiểm tra lại thông tin nhập vào.";
                return RedirectToAction(nameof(Profile));
            }

            try
            {
                // ✅ Lấy MaKh (id khách hàng) từ session đã set khi đăng nhập
                int? taiKhoanId = HttpContext.Session.GetInt32("TaiKhoanId");
                if (taiKhoanId == null)
                {
                    TempData["ErrorMessage"] = "Phiên đăng nhập đã hết hạn. Vui lòng đăng nhập lại.";
                    return RedirectToAction("DangNhap", "TaiKhoan");
                }

                // ✅ Tìm mã khách hàng từ bảng TaiKhoan
                var taiKhoan = _context.TaiKhoans
                    .Include(tk => tk.KhachHang)
                    .FirstOrDefault(tk => tk.TaiKhoanId == taiKhoanId);

                if (taiKhoan?.KhachHang == null)
                {
                    TempData["ErrorMessage"] = "Không tìm thấy thông tin khách hàng.";
                    return RedirectToAction(nameof(Profile));
                }

                var customer = taiKhoan.KhachHang;

                // Kiểm tra email đã tồn tại chưa
                if (_context.KhachHangs.Any(kh => kh.Email == model.Email && kh.MaKh != customer.MaKh))
                {
                    TempData["ErrorMessage"] = "Email này đã được sử dụng bởi tài khoản khác.";
                    return RedirectToAction(nameof(Profile));
                }

                // Cập nhật thông tin
                customer.HoKh = model.HoKH;
                customer.TenKh = model.TenKH;
                customer.GioiTinh = model.gioiTinh;
                customer.Email = model.Email;
                customer.Sdt = model.SDT;
                customer.DiaChi = model.DiaChi;

                _context.Update(customer);
                _context.SaveChanges();

                // ✅ Không cần dùng Session.SetString("user") nữa
                TempData["SuccessMessage"] = "Cập nhật thông tin thành công!";
                return RedirectToAction(nameof(Profile));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi cập nhật thông tin khách hàng");
                TempData["ErrorMessage"] = "Đã xảy ra lỗi khi cập nhật thông tin. Vui lòng thử lại.";
                return RedirectToAction(nameof(Profile));
            }
        }

        // Model mới để binding
        public class KhachHangEditModel
        {
            [Required(ErrorMessage = "Họ không được để trống")]
            public string HoKH { get; set; }

            [Required(ErrorMessage = "Tên không được để trống")]
            public string TenKH { get; set; }

            [Required(ErrorMessage = "Giới tính không được để trống")]
            public string gioiTinh { get; set; }

            [Required(ErrorMessage = "Email không được để trống")]
            [EmailAddress(ErrorMessage = "Email không hợp lệ")]
            public string Email { get; set; }

            [Required(ErrorMessage = "Số điện thoại không được để trống")]
            [RegularExpression(@"^\d{10}$", ErrorMessage = "Số điện thoại phải có 10 chữ số")]
            public string SDT { get; set; }

            public string DiaChi { get; set; }
        }

    }
}
