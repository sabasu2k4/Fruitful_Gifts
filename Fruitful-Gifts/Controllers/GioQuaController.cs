using Fruitful_Gifts.Database;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace Fruitful_Gifts.Controllers
{
    public class GioQuaController : Controller
    {
        private readonly FruitfulGiftsContext _context;

        public GioQuaController(FruitfulGiftsContext context)
        {
            _context = context;
        }

        //[HttpGet("gio-qua/{slug}")]
        public IActionResult ChiTietGioQua(string slug)
        {
            var userId = GetLoggedInKhachHangId();

            var gioQua = _context.GioQuas
                .Include(sp => sp.MaDmNavigation)
                .Include(sp => sp.BinhLuans)
                    .ThenInclude(bl => bl.MaKhNavigation)
                .FirstOrDefault(sp => sp.Slug == slug && sp.TrangThai == 1);

            if (gioQua == null)
            {
                return NotFound();
            }

            // Kiểm tra người dùng đã đăng nhập hay chưa
            ViewBag.TrangThaiDangNhap = userId != null;

            // Kiểm tra sản phẩm có trong danh sách yêu thích của người dùng hay không
            var isFavorite = _context.SanPhamYeuThiches
                .Any(spy => spy.MaKh == userId && spy.MaSp == gioQua.MaGq);
            ViewBag.IsFavorite = isFavorite;

            // Kiểm tra người dùng đã từng mua hàng
            bool daMua = _context.ChiTietDonHangs
                .Include(ct => ct.MaDhNavigation)
                .Any(ct => ct.MaSp == gioQua.MaGq &&
                           ct.MaDhNavigation.MaKh == userId &&
                           ct.MaDhNavigation.TrangThai == 4);

            // Tính số sao trung bình
            var danhGiaTB = (gioQua.BinhLuans != null && gioQua.BinhLuans.Any(b => b.TrangThai == 1))
                ? gioQua.BinhLuans.Where(b => b.TrangThai == 1).Average(b => b.SoSao ?? 0)
                : 0;
            ViewBag.TrungBinhSoSao = danhGiaTB;

            // Số lượt yêu thích
            //var soLuotYeuThich = _context.SanPhamYeuThiches
            //    .Count(spy => spy.MaSp == sanPham.MaSp);
            //ViewBag.SoLuotYeuThich = soLuotYeuThich;

            // Sản phẩm liên quan
            //var lienQuan = _context.GioQuas
            //    .Where(p => p.MaDm == sanPham.MaDm && p.MaSp != sanPham.MaSp && p.TrangThai == 1)
            //    .OrderByDescending(p => p.Gia)
            //    .Take(4) //số lượng hiện thị 
            //    .ToList();
            //ViewBag.SanPhamLienQuan = lienQuan;

            ViewBag.SoLuongGioQua = TinhSoLuongTonGioQua(slug);

            ViewBag.DaMua = daMua;
            ViewBag.TrangThaiDangNhap = userId != null;

            return View(gioQua);
        }

        public int TinhSoLuongTonGioQua(string slug)
        {
            var chiTietList = _context.ChiTietGioQuas
                .Where(ct => ct.MaGqNavigation.Slug == slug)
                .Include(ct => ct.MaSpNavigation)
                .ThenInclude(sp => sp.KhoHangs)
                .ToList();

            if (!chiTietList.Any())
                return 0;

            var soLuongTonList = chiTietList.Select(ct =>
            {
                var tongTon = ct.MaSpNavigation.KhoHangs.Sum(k => k.SoLuongTon);
                if (ct.SoLuong == 0)
                    return int.MaxValue;

                return (int)(tongTon / ct.SoLuong);
            });

            return soLuongTonList.Min();
        }

        [HttpGet]
        public IActionResult GetQuantity(string slug)
        {
            var gq = _context.GioQuas.FirstOrDefault(x => x.Slug == slug);
            if (gq == null) return NotFound();

            return Json(new { soLuong = TinhSoLuongTonGioQua(slug) });
        }

        public int? GetLoggedInKhachHangId()
        {
            var khachHangJson = HttpContext.Session.GetString("user");

            if (string.IsNullOrEmpty(khachHangJson))
            {
                return null;
            }

            var thongTinkhachHang = JsonSerializer.Deserialize<KhachHang>(khachHangJson);

            if (thongTinkhachHang != null)
            {
                var customer = _context.KhachHangs.FirstOrDefault(kh => kh.MaKh == thongTinkhachHang.MaKh);

                if (customer != null)
                {
                    return customer.MaKh;
                }
            }

            return null;
        }

    }
}
