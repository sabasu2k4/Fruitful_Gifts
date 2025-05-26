//using Fruitful_Gifts.Database;
//using Microsoft.AspNetCore.Mvc;
//using System.Text.Json;
//using Microsoft.EntityFrameworkCore;
//using Fruitful_Gifts.ViewModels;
//using System.Linq;
//using System.Threading.Tasks;
//using System;

//namespace Fruitful_Gifts.Controllers
//{
//    public class DonHangController : Controller
//    {
//        private readonly FruitfulGiftsContext _context;

//        public DonHangController(FruitfulGiftsContext context)
//        {
//            _context = context;
//        }

//        public int? GetLoggedInKhachHangId()
//        {
//            var khachHangJson = HttpContext.Session.GetString("user");

//            if (string.IsNullOrEmpty(khachHangJson))
//            {
//                return null;
//            }

//            var thongTinkhachHang = JsonSerializer.Deserialize<KhachHang>(khachHangJson);

//            if (thongTinkhachHang != null)
//            {
//                var customer = _context.KhachHangs.FirstOrDefault(kh => kh.MaKh == thongTinkhachHang.MaKh);

//                if (customer != null)
//                {
//                    return customer.MaKh;
//                }
//            }

//            return null;
//        }

//        public IActionResult Index(int? trangThai, int page = 1)
//        {
//            int pageSize = 4;
//            var maKhachHang = GetLoggedInKhachHangId();
//            if (maKhachHang == null)
//            {
//                return RedirectToAction("DangKy", "TaiKhoan");
//            }

//            var donHangsQuery = _context.DonHangs
//                .Include(d => d.MaKhNavigation)
//                .Include(d => d.ChiTietDonHangs)
//                    .ThenInclude(ct => ct.MaSpNavigation)
//                .Where(d => d.MaKh == maKhachHang);

//            if (trangThai.HasValue)
//            {
//                donHangsQuery = donHangsQuery.Where(d => d.TrangThai == trangThai);
//            }

//            // Đếm tổng số đơn hàng cho phân trang
//            int totalOrders = donHangsQuery.Count();

//            // Lấy dữ liệu cho trang hiện tại
//            var result = donHangsQuery
//                .OrderByDescending(d => d.NgayDatHang)
//                .Skip((page - 1) * pageSize)
//                .Take(pageSize)
//                .Select(d => new DonHangViewModel
//                {
//                    MaDh = d.MaDh,
//                    TenKhachHang = d.MaKhNavigation != null ? d.MaKhNavigation.HoKh + " " + d.MaKhNavigation.TenKh : "Không xác định",
//                    NgayDatHang = d.NgayDatHang,
//                    TongTienDonHang = d.TongTienDonHang,
//                    TrangThai = d.TrangThai,
//                    SanPhams = d.ChiTietDonHangs.Select(ct => new SanPhamTrongDonHangViewModel
//                    {
//                        TenSp = ct.MaSpNavigation.TenSp,
//                        SoLuong = (int)ct.SoLuong,
//                        TongTien = ct.TongTienTungSanPham
//                    }).ToList()
//                })
//                .ToList();

//            var countByStatus = _context.DonHangs
//                .Where(d => d.MaKh == maKhachHang)
//                .GroupBy(d => d.TrangThai)
//                .Select(g => new { TrangThai = g.Key ?? 0, Count = g.Count() })
//                .ToDictionary(g => g.TrangThai, g => g.Count);

//            ViewBag.CountsByStatus = countByStatus;
//            ViewBag.CurrentTrangThai = trangThai;
//            ViewBag.CurrentPage = page;
//            ViewBag.TotalPages = (int)Math.Ceiling(totalOrders / (double)pageSize);

//            return View(result);
//        }



//        // Xem chi tiết đơn hàng
//        [HttpGet]
//        public async Task<IActionResult> DetailsDonHang(int id)
//        {
//            var maKhachHang = GetLoggedInKhachHangId();
//            if (maKhachHang == null)
//            {
//                return RedirectToAction("DangKy", "TaiKhoan");
//            }

//            var donHang = await _context.DonHangs
//                .Include(d => d.MaKhNavigation)
//                .Include(d => d.ChiTietDonHangs)
//                    .ThenInclude(ct => ct.MaSpNavigation)
//                .FirstOrDefaultAsync(d => d.MaDh == id && d.MaKh == maKhachHang);

//            if (donHang == null)
//            {
//                return NotFound();
//            }
//            if (donHang.TongTienDonHang == null || donHang.TongTienDonHang == 0)
//            {
//                donHang.TongTienDonHang = donHang.ChiTietDonHangs
//                    .Sum(ct => (ct.SoLuong ?? 0) * (ct.MaSpNavigation?.Gia ?? 0));
//            }
//            return View(donHang); // View chi tiết dùng model DonHang
//        }

//        // Hủy đơn hàng
//        [HttpPost]
//        public async Task<IActionResult> CancelOrder(int id)
//        {
//            var maKhachHang = GetLoggedInKhachHangId();
//            if (maKhachHang == null)
//            {
//                return RedirectToAction("DangKy", "TaiKhoan");
//            }

//            var donHang = await _context.DonHangs
//                .FirstOrDefaultAsync(d => d.MaDh == id && d.MaKh == maKhachHang);

//            if (donHang == null)
//            {
//                return NotFound();
//            }

//            // Kiểm tra trạng thái cho phép hủy (1 hoặc 5)
//            if (donHang.TrangThai != 1 && donHang.TrangThai != 5)
//            {
//                TempData["ErrorMessage"] = "Đơn hàng không thể hủy ở trạng thái hiện tại.";
//                return RedirectToAction("DetailsDonHang", new { id });
//            }

//            donHang.TrangThai = 3; // Đã hủy
//            //donHang.NgayCapNhat = DateTime.Now;

//            try
//            {
//                await _context.SaveChangesAsync();
//                TempData["SuccessMessage"] = "Hủy đơn hàng thành công.";
//            }
//            catch (Exception)
//            {
//                TempData["ErrorMessage"] = "Có lỗi xảy ra khi hủy đơn hàng.";
//            }

//            return RedirectToAction("DetailsDonHang", new { id });
//        }

//        // Mua lại đơn hàng
//        [HttpPost]
//        public async Task<IActionResult> RepurchaseOrder(int id)
//        {
//            var maKhachHang = GetLoggedInKhachHangId();
//            if (maKhachHang == null)
//            {
//                return RedirectToAction("DangKy", "TaiKhoan");
//            }

//            // Bao gồm ChiTietDonHangs và MaSpNavigation để tránh lỗi null
//            var donHangCu = await _context.DonHangs
//                .Include(d => d.ChiTietDonHangs)
//                    .ThenInclude(ct => ct.MaSpNavigation)
//                .FirstOrDefaultAsync(d => d.MaDh == id && d.MaKh == maKhachHang);

//            if (donHangCu == null)
//            {
//                return NotFound();
//            }

//            // Chỉ cho phép mua lại nếu trạng thái là Đã hủy (3) hoặc Giao thất bại (5)
//            if (donHangCu.TrangThai != 3 && donHangCu.TrangThai != 5)
//            {
//                TempData["ErrorMessage"] = "Chỉ có thể mua lại đơn hàng đã hủy hoặc giao thất bại.";
//                return RedirectToAction("DetailsDonHang", new { id });
//            }

//            // Tạo đơn hàng mới từ đơn cũ
//            var donHangMoi = new DonHang
//            {
//                MaKh = maKhachHang.Value,
//                NgayDatHang = DateTime.Now,
//                TrangThai = 1, // Đang xử lý
//                TongTienDonHang = donHangCu.TongTienDonHang,
//                DiaChiNhanHang = donHangCu.DiaChiNhanHang,
//                SoDienThoai = donHangCu.SoDienThoai,
//                ChiTietDonHangs = donHangCu.ChiTietDonHangs.Select(ct => new ChiTietDonHang
//                {
//                    MaSp = ct.MaSp,
//                    SoLuong = ct.SoLuong,
//                    TongTienTungSanPham = ct.SoLuong * (ct.MaSpNavigation?.Gia ?? 0)
//                }).ToList()
//            };

//            _context.DonHangs.Add(donHangMoi);

//            try
//            {
//                await _context.SaveChangesAsync();
//                TempData["SuccessMessage"] = "Mua lại đơn hàng thành công.";
//                return RedirectToAction("DetailsDonHang", new { id = donHangMoi.MaDh });
//            }
//            catch (Exception)
//            {
//                TempData["ErrorMessage"] = "Có lỗi xảy ra khi mua lại đơn hàng.";
//                return RedirectToAction("DetailsDonHang", new { id });
//            }
//        }

//    }
//}
