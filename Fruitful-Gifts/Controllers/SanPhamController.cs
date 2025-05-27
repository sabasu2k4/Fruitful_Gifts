using Fruitful_Gifts.Database;
using Fruitful_Gifts.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace Fruitful_Gifts.Controllers
{
    public class SanPhamController : Controller
    {
        private readonly FruitfulGiftsContext _context;

        public SanPhamController(FruitfulGiftsContext context)
        {
            _context = context;
        }
        [HttpGet("san-pham/{slug}")]
        public IActionResult ChiTietSanPham(string slug, int page = 1)
        {
            int pageSize = 3;//phân trang bình luận

            var userId = GetLoggedInKhachHangId();

            var sanPham = _context.SanPhams
                .Include(sp => sp.MaLoaiNavigation)
                .Include(sp => sp.MaNccNavigation)
                .Include(sp => sp.BinhLuans)
                    .ThenInclude(bl => bl.MaKhNavigation)
                .FirstOrDefault(sp => sp.Slug == slug && sp.TrangThai == 1);

            if (sanPham == null)
                return NotFound();

            int soLuongTonKho = _context.KhoHangs
                .Where(k => k.MaSp == sanPham.MaSp)
                .Select(k => k.SoLuongTon)
                .FirstOrDefault();

            bool isFavorite = false;
            bool daMua = false;
            bool daDanhGia = false;
            List<int> donHangChuaDanhGia = new();

            if (userId != null)
            {
                isFavorite = _context.SanPhamYeuThiches
                    .Any(spy => spy.MaKh == userId && spy.MaSp == sanPham.MaSp);

                daMua = _context.ChiTietDonHangs
                    .Include(ct => ct.MaDhNavigation)
                    .Any(ct => ct.MaSp == sanPham.MaSp &&
                               ct.MaDhNavigation.MaKh == userId &&
                               ct.MaDhNavigation.TrangThai == 4);

                daDanhGia = _context.BinhLuans
                    .Any(bl => bl.MaSp == sanPham.MaSp && bl.MaKh == userId);

                donHangChuaDanhGia = _context.ChiTietDonHangs
                    .Include(ct => ct.MaDhNavigation)
                    .Where(ct => ct.MaSp == sanPham.MaSp &&
                                 ct.MaDhNavigation.MaKh == userId &&
                                 ct.MaDhNavigation.TrangThai == 4 &&
                                 !_context.BinhLuans.Any(bl => bl.MaSp == sanPham.MaSp &&
                                                               bl.MaKh == userId &&
                                                               bl.MaGq == ct.MaDh))
                    .Select(ct => ct.MaDh)
                    .Distinct()
                    .ToList();
            }

            ViewBag.DaMua = daMua;
            ViewBag.DaDanhGia = daDanhGia;
            ViewBag.DonHangChuaDanhGia = donHangChuaDanhGia;

            // Tính sao trung bình
            double trungBinhSoSao = sanPham.BinhLuans?
                .Where(b => b.TrangThai == 1 && b.SoSao != null)
                .Select(b => b.SoSao.Value)
                .DefaultIfEmpty(0)
                .Average() ?? 0;

            // Sản phẩm yêu thích
            int soLuotYeuThich = _context.SanPhamYeuThiches
                .Count(spy => spy.MaSp == sanPham.MaSp);

            // Sản phẩm liên quan
            var sanPhamLienQuan = _context.SanPhams
           .Where(p => p.MaLoai == sanPham.MaLoai &&
                       p.MaSp != sanPham.MaSp &&
                       p.TrangThai == 1 &&
                       p.GiaBan != null && sanPham.GiaBan != null &&
                       Math.Abs(p.GiaBan.Value - sanPham.GiaBan.Value) <= sanPham.GiaBan.Value * 0.2m)
           .OrderByDescending(p => p.GiaBan)
           .Take(4)
           .ToList();


            // BÌNH LUẬN PHÂN TRANG
            var binhLuans = sanPham.BinhLuans
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

            // Gửi về View
            var viewModel = new SanPhamViewModel
            {
                SanPham = sanPham,
                SoLuongTonKho = soLuongTonKho,
                TrangThaiDangNhap = userId != null,
                IsFavorite = isFavorite,
                DaMua = daMua,
                TrungBinhSoSao = trungBinhSoSao,
                SoLuotYeuThich = soLuotYeuThich,
                SanPhamLienQuan = sanPhamLienQuan
            };

            return View(viewModel);
        }


        [HttpGet]
        public IActionResult GetQuantity(int maSp)
        {
            var kho = _context.KhoHangs.FirstOrDefault(k => k.MaSp == maSp);
            if (kho == null) return NotFound();

            return Json(new { soLuong = kho.SoLuongTon });
        }

        //
        [HttpGet("san-pham/yeu-thich")]
        public IActionResult DanhSachSanPhamYeuThich(int page = 1)
        {
            var userId = GetLoggedInKhachHangId();
            if (userId == null)
            {
                return RedirectToAction("DangNhap", "TaiKhoan");
            }

            int pageSize = 6;
            var query = _context.SanPhamYeuThiches
                .Where(spy => spy.MaKh == userId)
                .Include(spy => spy.MaSpNavigation)
                    .ThenInclude(sp => sp.MaLoaiNavigation)
                .Include(spy => spy.MaSpNavigation.MaNccNavigation)
                .Select(spy => spy.MaSpNavigation)
                .Where(sp => sp.TrangThai == 1);

            int totalItems = query.Count();
            var sanPhams = query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            ViewBag.TotalPages = (int)Math.Ceiling((double)totalItems / pageSize);
            ViewBag.CurrentPage = page;

            return View(sanPhams);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveFromFavorites()
        {
            var userId = GetLoggedInKhachHangId();
            if (userId == null)
            {
                return Json(new { success = false, message = "Bạn cần đăng nhập để thực hiện thao tác này." });
            }

            // Đọc JSON body thủ công
            using (var reader = new StreamReader(Request.Body))
            {
                var body = await reader.ReadToEndAsync();
                var data = System.Text.Json.JsonDocument.Parse(body);
                if (!data.RootElement.TryGetProperty("maSp", out var maSpProp))
                {
                    return Json(new { success = false, message = "Thiếu mã sản phẩm." });
                }

                var maSp = maSpProp.GetInt32();

                var yeuThich = _context.SanPhamYeuThiches.FirstOrDefault(sp => sp.MaKh == userId && sp.MaSp == maSp);
                if (yeuThich != null)
                {
                    _context.SanPhamYeuThiches.Remove(yeuThich);
                    _context.SaveChanges();
                    return Json(new { success = true, message = "Đã xóa sản phẩm khỏi danh sách yêu thích." });
                }

                return Json(new { success = false, message = "Không tìm thấy sản phẩm yêu thích." });
            }
        }

        [HttpPost]
        public async Task<IActionResult> ThemSanPhamYeuThich(int productId)
        {
            var userId = GetLoggedInKhachHangId(); // Lấy ID khách hàng

            if (userId == null)
            {
                return Json(new { success = false, message = "Bạn cần đăng nhập để thêm vào danh sách yêu thích." });
            }

            var existingItem = _context.SanPhamYeuThiches
                .FirstOrDefault(x => x.MaKh == userId && x.MaSp == productId);

            if (existingItem != null)
            {
                _context.SanPhamYeuThiches.Remove(existingItem);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Sản phẩm đã được xóa khỏi danh sách yêu thích.", newLikeCount = GetLikeCount(productId), isAdded = false });
            }

            var newWishlistItem = new SanPhamYeuThich
            {
                MaKh = userId.Value,
                MaSp = productId
            };

            _context.SanPhamYeuThiches.Add(newWishlistItem);
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Sản phẩm đã được thêm vào danh sách yêu thích!", newLikeCount = GetLikeCount(productId), isAdded = true });
        }

        private int GetLikeCount(int productId)
        {
            return _context.SanPhamYeuThiches.Count(x => x.MaSp == productId);
        }

        public int? GetLoggedInKhachHangId()
        {
            // Kiểm tra session có tồn tại và đúng vai trò KhachHang không
            var vaiTro = HttpContext.Session.GetString("VaiTro");
            if (vaiTro != "KhachHang")
            {
                return null;
            }

            // Lấy TaiKhoanId từ session
            int? taiKhoanId = HttpContext.Session.GetInt32("TaiKhoanId");
            if (taiKhoanId == null)
            {
                return null;
            }

            // Tìm khách hàng tương ứng, sử dụng Include nếu cần thông tin liên quan
            var khachHang = _context.KhachHangs
                .FirstOrDefault(kh => kh.TaiKhoanId == taiKhoanId);

            return khachHang?.MaKh;
        }

        //
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BinhLuan(int MaSp, int Rating, string NoiDung, int MaDh)
        {
            var maKH = GetLoggedInKhachHangId();
            if (!maKH.HasValue)
            {
                TempData["Message"] = "Bạn cần đăng nhập để đánh giá.";
                var spLogin = await _context.SanPhams.FindAsync(MaSp);
                return RedirectToAction("ChiTietSanPham", new { slug = spLogin?.Slug ?? "" });
            }

            bool daMua = _context.ChiTietDonHangs
                .Include(ct => ct.MaDhNavigation)
                .Any(ct => ct.MaSp == MaSp &&
                           ct.MaDh == MaDh &&
                           ct.MaDhNavigation.MaKh == maKH &&
                           ct.MaDhNavigation.TrangThai == 4);

            if (!daMua)
            {
                TempData["Message"] = "Bạn cần mua sản phẩm trước khi đánh giá.";
                var spNotBuy = await _context.SanPhams.FindAsync(MaSp);
                return RedirectToAction("ChiTietSanPham", new { slug = spNotBuy?.Slug ?? "" });
            }

            var daDanhGiaDonHangNay = await _context.BinhLuans
                .AnyAsync(b => b.MaSp == MaSp && b.MaKh == maKH && b.MaGq == MaDh);

            if (daDanhGiaDonHangNay)
            {
                TempData["Message"] = "Bạn đã đánh giá sản phẩm này cho đơn hàng này rồi.";
                var spRated = await _context.SanPhams.FindAsync(MaSp);
                return RedirectToAction("ChiTietSanPham", new { slug = spRated?.Slug ?? "" });
            }

            var binhLuan = new BinhLuan
            {
                MaKh = maKH.Value,
                MaSp = MaSp,
                MaGq = MaDh,
                SoSao = Rating,
                NoiDung = NoiDung,
                NgayBinhLuan = DateTime.Now,
                TrangThai = 1,
                CreatedAt = DateTime.Now
            };

            _context.BinhLuans.Add(binhLuan);
            await _context.SaveChangesAsync();

            TempData["Message"] = "Đánh giá đã được gửi thành công!";

            var sp = await _context.SanPhams.FindAsync(MaSp);
            return RedirectToAction("ChiTietSanPham", new { slug = sp?.Slug ?? "" });
        }


        //
        public async Task<IActionResult> SanPhamChamDiem(int page = 1, int pageSize = 6)
        {
            var maKH = GetLoggedInKhachHangId();

            if (!maKH.HasValue)
            {
                TempData["ErrorMessage"] = "Vui lòng đăng nhập để xem sản phẩm chấm điểm.";
                return RedirectToAction("DangNhap", "TaiKhoan");
            }

            var query = _context.BinhLuans
                .Include(d => d.MaKhNavigation)
                .Include(d => d.MaSpNavigation)
                .Where(d => d.MaKh == maKH.Value)
                .OrderByDescending(d => d.NgayBinhLuan);

            int totalItems = await query.CountAsync();

            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling((double)totalItems / pageSize);

            if (!items.Any())
            {
                TempData["InfoMessage"] = "Không có sản phẩm chấm điểm.";
            }

            return View(items);
        }


        public async Task<IActionResult> SuaChamDiem(int sanPhamId)
        {
            var maKH = GetLoggedInKhachHangId();

            // Kiểm tra nếu khách hàng chưa đăng nhập
            if (!maKH.HasValue)
            {
                TempData["Message"] = "Vui lòng đăng nhập để sửa chấm điểm.";
                return RedirectToAction("Index", "TrangChu");
            }

            // Lấy chi tiết bình luận/chấm điểm của sản phẩm từ khách hàng đã đăng nhập
            var chiTietBinhLuan = await _context.BinhLuans
                .FirstOrDefaultAsync(c => c.MaKh == maKH.Value && c.MaSp == sanPhamId);
            // Trả về view và truyền chi tiết bình luận/chấm điểm của sản phẩm
            return View(chiTietBinhLuan);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SuaChamDiem(int sanPhamId, int rating, string comment)
        {
            var maKH = GetLoggedInKhachHangId();

            // Kiểm tra nếu khách hàng chưa đăng nhập
            if (!maKH.HasValue)
            {
                TempData["Message"] = "Vui lòng đăng nhập để sửa chấm điểm.";
                return RedirectToAction("Index", "TrangChu");
            }

            // Lấy chi tiết bình luận/chấm điểm của sản phẩm từ khách hàng đã đăng nhập
            var chiTietBinhLuan = await _context.BinhLuans
                .FirstOrDefaultAsync(c => c.MaKh == maKH.Value && c.MaSp == sanPhamId);
            // Cập nhật chấm điểm và bình luận
            chiTietBinhLuan.SoSao = rating; // Cập nhật điểm đánh giá
            chiTietBinhLuan.NoiDung = comment; // Cập nhật bình luận

            // Cập nhật vào cơ sở dữ liệu
            _context.BinhLuans.Update(chiTietBinhLuan);
            await _context.SaveChangesAsync();

            // Cập nhật thông báo thành công
            TempData["Message"] = "Cập nhật chấm điểm thành công!";

            // Trả về cùng trang hiện tại để hiển thị thông báo
            return View(chiTietBinhLuan);
        }


    }
}
