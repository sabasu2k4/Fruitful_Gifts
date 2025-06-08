using Fruitful_Gifts.Database;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Fruitful_Gifts.ViewModels;
using System.Linq;
using System.Threading.Tasks;
using System;

namespace Fruitful_Gifts.Controllers
{
    public class DonHangController : Controller
    {
        private readonly FruitfulGiftsContext _context;

        public DonHangController(FruitfulGiftsContext context)
        {
            _context = context;
        }

        public int? GetLoggedInKhachHangId()
        {
            int? taiKhoanId = HttpContext.Session.GetInt32("TaiKhoanId");
            if (taiKhoanId == null)
            {
                return null;
            }

            var khachHang = _context.KhachHangs.FirstOrDefault(kh => kh.TaiKhoanId == taiKhoanId);
            return khachHang?.MaKh;
        }

        public IActionResult Index(int? trangThai, int page = 1)
        {
            int pageSize = 3;
            var maKhachHang = GetLoggedInKhachHangId();
            if (maKhachHang == null)
                return RedirectToAction("DangKy", "TaiKhoan");

            var donHangsQuery = _context.DonHangs
                .Include(d => d.MaKhNavigation)
                .Include(d => d.MaPtNavigation)
                .Include(d => d.ChiTietDonHangs)
                    .ThenInclude(ct => ct.MaSpNavigation)
                .Include(d => d.ChiTietDonHangs)
                    .ThenInclude(ct => ct.MaGqNavigation)
                .Where(d => d.MaKh == maKhachHang);

            if (trangThai.HasValue)
            {
                donHangsQuery = donHangsQuery.Where(d => d.TrangThai == trangThai);
            }

            int totalOrders = donHangsQuery.Count();
            var donHangs = donHangsQuery
                .OrderByDescending(d => d.NgayDatHang)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var result = donHangs.Select(d => new DonHangViewModel
            {
                MaDh = d.MaDh,
                TenKhachHang = d.MaKhNavigation != null
                    ? d.MaKhNavigation.HoKh + " " + d.MaKhNavigation.TenKh
                    : "Không xác định",
                NgayDatHang = d.NgayDatHang,
                TongTienDonHang = d.TongTienDonHang,
                PhiVanChuyen = d.PhiVanChuyenBanHang,
                TrangThai = d.TrangThai,
                TrangThaiThanhToan = d.TrangThaiThanhToan,
                PhuongThucThanhToan = d.MaPtNavigation?.TenPt,
                DiaChiNhanHang = d.DiaChiNhanHang,
                SoDienThoai = d.SoDienThoai,
                GhiChu = d.GhiChu,
                SanPhams = d.ChiTietDonHangs.Select(ct => new SanPhamTrongDonHangViewModel
                {
                    // Nếu là sản phẩm thì hiển thị tên, hình, giá của sản phẩm
                    TenSp = ct.MaSpNavigation != null ? ct.MaSpNavigation.TenSp
                            // Nếu là giỏ quà thì hiển thị tên giỏ quà
                            : ct.MaGqNavigation != null ? ct.MaGqNavigation.TenGioQua
                            : "Sản phẩm/Giỏ quà không tồn tại",

                    HinhAnh = ct.MaSpNavigation?.HinhAnh ?? ct.MaGqNavigation?.HinhAnh,

                    Loai = ct.MaSp != null ? "sp" : ct.MaGq != null ? "gq" : null,

                    SoLuong = ct.SoLuong.HasValue ? (int?)Convert.ToInt32(ct.SoLuong.Value) : 0,

                    // Giá bán lấy theo từng loại (sản phẩm hoặc giỏ quà)
                    DonGia = ct.GiaBan ?? 0,

                    TongTien = ct.TongTienTungSanPham,

                    // Lấy số lượng tồn kho nếu là sản phẩm (giỏ quà thường không có tồn kho riêng)
                    SoLuongTon = ct.MaSp != null
                        ? _context.KhoHangs.Where(k => k.MaSp == ct.MaSp).Select(k => k.SoLuongTon).FirstOrDefault()
                        : (int?)null
                }).ToList()
            }).ToList();

            var countByStatus = _context.DonHangs
                .Where(d => d.MaKh == maKhachHang)
                .GroupBy(d => d.TrangThai)
                .Select(g => new { TrangThai = g.Key ?? 0, Count = g.Count() })
                .ToDictionary(g => g.TrangThai, g => g.Count);

            ViewBag.PaymentStatus = new Dictionary<int, string>
            {
                {0, "Chưa thanh toán"},
                {1, "Đã thanh toán"},
                {2, "Thanh toán thất bại"},
                {3, "Đang xử lý thanh toán"}
            };

            ViewBag.CountsByStatus = countByStatus;
            ViewBag.CurrentTrangThai = trangThai;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling(totalOrders / (double)pageSize);

            return View(result);
        }

        [HttpGet]
        public async Task<IActionResult> DetailsDonHang(int id)
        {
            var maKhachHang = GetLoggedInKhachHangId();
            if (maKhachHang == null)
                return RedirectToAction("DangKy", "TaiKhoan");

            var donHang = await _context.DonHangs
                .Include(d => d.MaKhNavigation)
                .Include(d => d.MaNvNavigation)
                .Include(d => d.MaPtNavigation)
                .Include(d => d.ChiTietDonHangs)
                    .ThenInclude(ct => ct.MaSpNavigation)
                .Include(d => d.ChiTietDonHangs)
                    .ThenInclude(ct => ct.MaGqNavigation)
                .FirstOrDefaultAsync(d => d.MaDh == id && d.MaKh == maKhachHang);

            if (donHang == null)
                return NotFound();

            if (donHang.TongTienDonHang == null || donHang.TongTienDonHang == 0)
            {
                donHang.TongTienDonHang = donHang.ChiTietDonHangs
                    .Sum(ct => (ct.SoLuong ?? 0) * (ct.GiaBan ?? 0)) + (donHang.PhiVanChuyenBanHang ?? 0);
            }

            ViewBag.TrangThaiLabels = new Dictionary<int, string>
            {
                  {1, "⏳ Chờ xác nhận"},
                  {2, "✅ Đã xác nhận"},
                  {3, "🚚 Đang giao hàng"},
                  {4, "✔️ Đã giao hàng"},
                  {5, "🔄 Hoàn hàng"},
                  {6, "❌ Đã hủy"},
                  {7, "⚠️ Giao hàng thất bại"},
                  {8, "🚫 Từ chối"}
            };

            ViewBag.PaymentStatus = new Dictionary<int, string>
            {
                {0, "Chưa thanh toán"},
                {1, "Đã thanh toán"},
                {2, "Thanh toán thất bại"},
                {3, "Đang xử lý thanh toán"}
            };

            return View(donHang);
        }

        [HttpPost]
        public async Task<IActionResult> CancelOrder(int id)
        {
            var maKhachHang = GetLoggedInKhachHangId();
            if (maKhachHang == null)
                return RedirectToAction("DangKy", "TaiKhoan");

            var donHang = await _context.DonHangs
                .FirstOrDefaultAsync(d => d.MaDh == id && d.MaKh == maKhachHang);

            if (donHang == null)
                return NotFound();

            if (donHang.TrangThai != 1 && donHang.TrangThai != 5)
            {
                TempData["ErrorMessage"] = "Đơn hàng không thể hủy ở trạng thái hiện tại.";
                return RedirectToAction("DetailsDonHang", new { id });
            }

            donHang.TrangThai = 6; // Mã trạng thái "Đã hủy" (theo model của bạn có TrangThaiNavigation)

            try
            {
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Hủy đơn hàng thành công.";
            }
            catch (Exception)
            {
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi hủy đơn hàng.";
            }

            return RedirectToAction("DetailsDonHang", new { id });
        }

        [HttpPost]
        public async Task<IActionResult> RepurchaseOrder(int id)
        {
            var maKhachHang = GetLoggedInKhachHangId();
            if (maKhachHang == null)
                return RedirectToAction("DangKy", "TaiKhoan");

            var donHangCu = await _context.DonHangs
                .Include(d => d.ChiTietDonHangs)
                    .ThenInclude(ct => ct.MaSpNavigation)
                .Include(d => d.ChiTietDonHangs)
                    .ThenInclude(ct => ct.MaGqNavigation)
                .FirstOrDefaultAsync(d => d.MaDh == id && d.MaKh == maKhachHang);

            if (donHangCu == null)
                return NotFound();

            if (donHangCu.TrangThai != 6 && donHangCu.TrangThai != 7) // 6: hủy, 7: giao thất bại
            {
                TempData["ErrorMessage"] = "Chỉ có thể mua lại đơn hàng đã hủy hoặc giao thất bại.";
                return RedirectToAction("DetailsDonHang", new { id });
            }

            var donHangMoi = new DonHang
            {
                MaKh = maKhachHang.Value,
                NgayDatHang = DateTime.Now,
                PhiVanChuyenBanHang = donHangCu.PhiVanChuyenBanHang,
                TrangThai = 1, // chờ xác nhận
                TrangThaiThanhToan = 0,
                DiaChiNhanHang = donHangCu.DiaChiNhanHang,
                SoDienThoai = donHangCu.SoDienThoai,
                GhiChu = donHangCu.GhiChu,
                MaPt = donHangCu.MaPt
            };

            _context.DonHangs.Add(donHangMoi);
            await _context.SaveChangesAsync();

            foreach (var ct in donHangCu.ChiTietDonHangs)
            {
                var chiTietMoi = new ChiTietDonHang
                {
                    MaDh = donHangMoi.MaDh,
                    MaSp = ct.MaSp,
                    MaGq = ct.MaGq,
                    SoLuong = ct.SoLuong,
                    GiaBan = ct.GiaBan,
                    TongTienTungSanPham = (ct.SoLuong ?? 0) * (ct.GiaBan ?? 0)
                };
                _context.ChiTietDonHangs.Add(chiTietMoi);
            }
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Đặt lại đơn hàng thành công.";
            return RedirectToAction("Index");
        }
    }
}
