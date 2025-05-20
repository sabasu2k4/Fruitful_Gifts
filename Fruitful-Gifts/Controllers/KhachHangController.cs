using Fruitful_Gifts.Database;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Text.Json;

namespace Fruitful_Gifts.Controllers
{
    public class KhachHangController : Controller
    {
        private readonly FruitfulGiftsContext _context;
        public KhachHangController(FruitfulGiftsContext context)
        {
            _context = context;
        }
        public IActionResult Index()
        {
            return View();
        }
        //Thông tin tài khoản khách hàng
        public IActionResult Profile()
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
                Debug.WriteLine("========: " + customer);

                if (customer != null)
                {
                    return View(customer);
                }
            }

            return View();
        }


        // Sửa thông tin tài khoản 

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

        // Sửa thông tin tài khoản 

        [HttpPost]
        public IActionResult EditProfile(string hoKH, string tenKH, string gioiTinh, string email, string sdt, string diaChi)
        {
            try
            {
                var khachHangJson = HttpContext.Session.GetString("user");

                if (string.IsNullOrEmpty(khachHangJson))
                {
                    TempData["ErrorMessage"] = "Vui lòng đăng nhập lại.";
                    return RedirectToAction("Index", "TrangChu");
                }

                var thongTinkhachHang = JsonSerializer.Deserialize<KhachHang>(khachHangJson);

                if (thongTinkhachHang != null)
                {
                    var customer = _context.KhachHangs.FirstOrDefault(c => c.MaKh == thongTinkhachHang.MaKh);

                    if (customer != null)
                    {
                        // Kiểm tra email đã tồn tại
                        var existingAccount = _context.KhachHangs.FirstOrDefault(kh => kh.Email == email && kh.MaKh != thongTinkhachHang.MaKh);
                        if (existingAccount != null)
                        {
                            TempData["ErrorMessage"] = "Email đã tồn tại.";
                            return RedirectToAction("EditProfile");
                        }

                        // Cập nhật thông tin
                        customer.HoKh = hoKH;
                        customer.TenKh = tenKH;
                        customer.GioiTinh = gioiTinh;
                        customer.Email = email;
                        customer.Sdt = sdt;
                        customer.DiaChi = diaChi;

                        _context.KhachHangs.Update(customer);
                        _context.SaveChanges();

                        // Lưu thông tin mới vào Session
                        HttpContext.Session.SetString("user", JsonSerializer.Serialize(customer));

                        TempData["SuccessMessage"] = "Cập nhật thông tin thành công!";
                        return RedirectToAction("Profile");
                    }
                }

                TempData["ErrorMessage"] = "Không tìm thấy thông tin khách hàng.";
                return RedirectToAction("EditProfile");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Có lỗi xảy ra: " + ex.Message;
                return RedirectToAction("EditProfile");
            }
        }


    }
}
