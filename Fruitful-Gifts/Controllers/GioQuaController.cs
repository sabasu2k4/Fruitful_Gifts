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

        [HttpGet("gio-qua/{slug}")]
        public IActionResult ChiTietGioQua(string slug, int page = 1, int pageSize = 5)
        {
            var userId = GetLoggedInKhachHangId();

            var gioQua = _context.GioQuas
                .Include(gq => gq.MaDmNavigation)
                .Include(gq => gq.BinhLuans)
                    .ThenInclude(bl => bl.MaKhNavigation)
                .FirstOrDefault(gq => gq.Slug == slug && gq.TrangThai == 1);

            if (gioQua == null)
                return NotFound();

            bool isLoggedIn = userId != null;
            ViewBag.TrangThaiDangNhap = isLoggedIn;

            bool isFavorite = false;
            if (isLoggedIn)
            {
                isFavorite = _context.SanPhamYeuThiches
                    .Any(spy => spy.MaKh == userId && spy.MaGq == gioQua.MaGq);
            }
            ViewBag.IsFavorite = isFavorite;

            bool daMua = false;
            if (isLoggedIn)
            {
                daMua = _context.ChiTietDonHangs
                    .Include(ct => ct.MaDhNavigation)
                    .Any(ct => ct.MaSp == gioQua.MaGq &&
                               ct.MaDhNavigation.MaKh == userId &&
                               ct.MaDhNavigation.TrangThai == 4);
            }
            ViewBag.DaMua = daMua;

            bool daDanhGia = false;
            if (isLoggedIn)
            {
                daDanhGia = _context.BinhLuans
                    .Any(bl => bl.MaGq == gioQua.MaGq && bl.MaKh == userId);
            }
            ViewBag.DaDanhGia = daDanhGia;


            List<int> donHangChuaDanhGia = new();
            if (isLoggedIn)
            {
                donHangChuaDanhGia = _context.ChiTietDonHangs
                    .Include(ct => ct.MaDhNavigation)
                    .Where(ct => ct.MaGq == gioQua.MaGq && 
                                 ct.MaDhNavigation.MaKh == userId &&
                                 ct.MaDhNavigation.TrangThai == 4 &&
                                 !_context.BinhLuans.Any(bl => bl.MaGq == gioQua.MaGq &&
                                                               bl.MaKh == userId &&
                                                               bl.MaDh == ct.MaDh))
                    .Select(ct => ct.MaDh)
                    .Distinct()
                    .ToList();
            }
            ViewBag.DonHangChuaDanhGia = donHangChuaDanhGia;


            double trungBinhSoSao = gioQua.BinhLuans?
                .Where(bl => bl.TrangThai == 1 && bl.SoSao != null)
                .Select(bl => bl.SoSao.Value)
                .DefaultIfEmpty(0)
                .Average() ?? 0;
            ViewBag.TrungBinhSoSao = trungBinhSoSao;

            int soLuotYeuThich = _context.SanPhamYeuThiches
                .Count(spy => spy.MaGq == gioQua.MaGq);
            ViewBag.SoLuotYeuThich = soLuotYeuThich;

            var gioQuaLienQuan = _context.GioQuas
                .Where(gq => gq.MaDm == gioQua.MaDm &&
                             gq.MaGq != gioQua.MaGq &&
                             gq.TrangThai == 1 &&
                             gq.GiaBan != null && gioQua.GiaBan != null &&
                             Math.Abs(gq.GiaBan - gioQua.GiaBan) <= gioQua.GiaBan * 0.2m)
                .OrderByDescending(gq => gq.GiaBan)
                .Take(4)
                .ToList();
            ViewBag.GioQuaLienQuan = gioQuaLienQuan;

            var binhLuans = gioQua.BinhLuans
                .Where(bl => bl.TrangThai == 1)
                .OrderByDescending(bl => bl.NgayBinhLuan)
                .ToList();

            int totalComments = binhLuans.Count;
            int totalPages = (int)Math.Ceiling((double)totalComments / pageSize);

            var binhLuansPhanTrang = binhLuans
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            ViewBag.BinhLuans = binhLuansPhanTrang;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;

            ViewBag.SoLuongGioQua = TinhSoLuongTonGioQua(slug);

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

        private int? GetLoggedInKhachHangId()
        {
            string userName = HttpContext.Session.GetString("UserName");
            var khachHang = _context.TaiKhoans.Include(t => t.KhachHang)
                .FirstOrDefault(t => t.TenDangNhap == userName);

            return khachHang?.KhachHang?.MaKh;
        }

        //
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BinhLuan(int? MaSp, int? MaGq, int Rating, string NoiDung, int MaDh)
        {
            var maKH = GetLoggedInKhachHangId();
            if (!maKH.HasValue)
            {
                TempData["Message"] = "Bạn cần đăng nhập để đánh giá.";

                if (MaSp.HasValue)
                {
                    var spLogin = await _context.SanPhams.FindAsync(MaSp.Value);
                    return RedirectToAction("ChiTietSanPham", new { slug = spLogin?.Slug ?? "" });
                }
                else if (MaGq.HasValue)
                {
                    var gqLogin = await _context.GioQuas.FindAsync(MaGq.Value);
                    return RedirectToAction("ChiTietGioQua", new { slug = gqLogin?.Slug ?? "" });
                }
                else
                {
                    return RedirectToAction("Index", "TrangChu");
                }
            }

            // Kiểm tra đã mua sản phẩm hoặc giỏ quà trong đơn hàng chưa?
            bool daMua = false;
            if (MaSp.HasValue)
            {
                daMua = _context.ChiTietDonHangs
                    .Include(ct => ct.MaDhNavigation)
                    .Any(ct => ct.MaSp == MaSp.Value &&
                               ct.MaDh == MaDh &&
                               ct.MaDhNavigation.MaKh == maKH &&
                               ct.MaDhNavigation.TrangThai == 4);
            }
            else if (MaGq.HasValue)
            {
                daMua = _context.ChiTietDonHangs
                    .Include(ct => ct.MaDhNavigation)
                    .Any(ct => ct.MaGq == MaGq.Value &&
                               ct.MaDh == MaDh &&
                               ct.MaDhNavigation.MaKh == maKH &&
                               ct.MaDhNavigation.TrangThai == 4);
            }

            if (!daMua)
            {
                TempData["Message"] = "Bạn cần mua sản phẩm trước khi đánh giá.";
                if (MaSp.HasValue)
                {
                    var spNotBuy = await _context.SanPhams.FindAsync(MaSp.Value);
                    return RedirectToAction("ChiTietSanPham", new { slug = spNotBuy?.Slug ?? "" });
                }
                else if (MaGq.HasValue)
                {
                    var gqNotBuy = await _context.GioQuas.FindAsync(MaGq.Value);
                    return RedirectToAction("ChiTietGioQua", new { slug = gqNotBuy?.Slug ?? "" });
                }
                else
                {
                    return RedirectToAction("Index", "TrangChu");
                }
            }

            // Kiểm tra đã đánh giá đơn hàng này cho sản phẩm/giỏ quà này chưa
           

            bool daDanhGiaDonHangNay = false;

            if (MaSp.HasValue)
            {
                daDanhGiaDonHangNay = await _context.BinhLuans
                    .AnyAsync(b => b.MaKh == maKH &&
                                   b.MaDh == MaDh &&
                                   b.MaSp == MaSp.Value);
            }
            else if (MaGq.HasValue)
            {
                daDanhGiaDonHangNay = await _context.BinhLuans
                    .AnyAsync(b => b.MaKh == maKH &&
                                   b.MaDh == MaDh &&
                                   b.MaGq == MaGq.Value);
            }
            if (daDanhGiaDonHangNay)
            {
                TempData["Message"] = "Bạn đã đánh giá giỏ quà này cho đơn hàng " + MaDh + " này rồi.";
                if (MaSp.HasValue)
                {
                    var spRated = await _context.SanPhams.FindAsync(MaSp.Value);
                    return RedirectToAction("ChiTietSanPham", new { slug = spRated?.Slug ?? "" });
                }
                else if (MaGq.HasValue)
                {
                    var gqRated = await _context.GioQuas.FindAsync(MaGq.Value);
                    return RedirectToAction("ChiTietGioQua", new { slug = gqRated?.Slug ?? "" });
                }
                else
                {
                    return RedirectToAction("Index", "TrangChu");
                }
            }

            // Tạo mới bình luận
            var binhLuan = new BinhLuan
            {
                MaKh = maKH.Value,
                MaSp = MaSp,
                MaGq = MaGq,
                SoSao = Rating,
                NoiDung = NoiDung,
                NgayBinhLuan = DateTime.Now,
                TrangThai = 1,
                CreatedAt = DateTime.Now,
                MaDh = MaDh
            };


            _context.BinhLuans.Add(binhLuan);
            await _context.SaveChangesAsync();

            TempData["Message"] = "Đánh giá đã được gửi thành công!";

            if (MaSp.HasValue)
            {
                var sp = await _context.SanPhams.FindAsync(MaSp.Value);
                return RedirectToAction("ChiTietSanPham", new { slug = sp?.Slug ?? "" });
            }
            else if (MaGq.HasValue)
            {
                var gq = await _context.GioQuas.FindAsync(MaGq.Value);
                return RedirectToAction("ChiTietGioQua", new { slug = gq?.Slug ?? "" });
            }
            else
            {
                return RedirectToAction("Index", "TrangChu");
            }
        }


    }
}
