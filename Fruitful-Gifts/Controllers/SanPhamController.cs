using Fruitful_Gifts.Database;
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
        public IActionResult ChiTietSanPham(string slug)
        {
            var userId = GetLoggedInKhachHangId();

            var sanPham = _context.SanPhams
                .Include(sp => sp.MaDmNavigation)
                .Include(sp => sp.MaNccNavigation)
                .Include(sp => sp.BinhLuans)
                    .ThenInclude(bl => bl.MaKhNavigation)
                .FirstOrDefault(sp => sp.Slug == slug && sp.TrangThai == 1);

            if (sanPham == null)
            {
                return NotFound();
            }

            // Kiểm tra người dùng đã đăng nhập hay chưa
            ViewBag.TrangThaiDangNhap = userId != null;

            // Kiểm tra sản phẩm có trong danh sách yêu thích của người dùng hay không
            var isFavorite = _context.SanPhamYeuThiches
                .Any(spy => spy.MaKh == userId && spy.MaSp == sanPham.MaSp);
            ViewBag.IsFavorite = isFavorite;

            // Kiểm tra người dùng đã từng mua hàng
            bool daMua = _context.ChiTietDonHangs
                .Include(ct => ct.MaDhNavigation)
                .Any(ct => ct.MaSp == sanPham.MaSp &&
                           ct.MaDhNavigation.MaKh == userId &&
                           ct.MaDhNavigation.TrangThai == 4);

            // Tính số sao trung bình
            var danhGiaTB = (sanPham.BinhLuans != null && sanPham.BinhLuans.Any(b => b.TrangThai == 1))
                ? sanPham.BinhLuans.Where(b => b.TrangThai == 1).Average(b => b.SoSao ?? 0)
                : 0;
            ViewBag.TrungBinhSoSao = danhGiaTB;

            // Số lượt yêu thích
            var soLuotYeuThich = _context.SanPhamYeuThiches
                .Count(spy => spy.MaSp == sanPham.MaSp);
            ViewBag.SoLuotYeuThich = soLuotYeuThich;

            // Sản phẩm liên quan
            var lienQuan = _context.SanPhams
                .Where(p => p.MaDm == sanPham.MaDm && p.MaSp != sanPham.MaSp && p.TrangThai == 1)
                .OrderByDescending(p => p.Gia)
                .Take(4) //số lượng hiện thị 
                .ToList();
            ViewBag.SanPhamLienQuan = lienQuan;

            ViewBag.DaMua = daMua;
            ViewBag.TrangThaiDangNhap = userId != null;

            return View(sanPham);
        }

        [HttpGet]
        public IActionResult GetQuantity(int maSp)
        {
            var product = _context.SanPhams.FirstOrDefault(x => x.MaSp == maSp);
            if (product == null) return NotFound();

            return Json(new { soLuong = product.SoLuong });
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
                    .ThenInclude(sp => sp.MaDmNavigation)
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
        public IActionResult ThemSanPhamYeuThich(int productId)
        {
            var userId = GetLoggedInKhachHangId(); // Lấy ID của khách hàng đã đăng nhập

            if (userId == null)
            {
                return Json(new { success = false, message = "Bạn cần đăng nhập để thêm vào danh sách yêu thích." });
            }

            var existingItem = _context.SanPhamYeuThiches
                .FirstOrDefault(x => x.MaKh == userId && x.MaSp == productId);

            if (existingItem != null)
            {
                _context.SanPhamYeuThiches.Remove(existingItem);
                _context.SaveChanges();

                return Json(new { success = true, message = "Sản phẩm đã được xóa khỏi danh sách yêu thích.", newLikeCount = GetLikeCount(productId), isAdded = false });
            }

            var newWishlistItem = new SanPhamYeuThich
            {
                MaKh = userId.Value,
                MaSp = productId
            };

            _context.SanPhamYeuThiches.Add(newWishlistItem);
            _context.SaveChanges();

            // Trả về số lượt yêu thích mới và trạng thái sản phẩm đã được thêm
            return Json(new { success = true, message = "Sản phẩm đã được thêm vào danh sách yêu thích!", newLikeCount = GetLikeCount(productId), isAdded = true });
        }

        private int GetLikeCount(int productId)
        {
            return _context.SanPhamYeuThiches.Count(x => x.MaSp == productId);
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
        //
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BinhLuan(int MaSp, int Rating, string NoiDung)
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
                           ct.MaDhNavigation.MaKh == maKH &&
                           ct.MaDhNavigation.TrangThai == 4);

            if (!daMua)
            {
                TempData["Message"] = "Bạn cần mua sản phẩm trước khi đánh giá.";
                var spNotBuy = await _context.SanPhams.FindAsync(MaSp);
                return RedirectToAction("ChiTietSanPham", new { slug = spNotBuy?.Slug ?? "" });
            }

            var binhLuanCu = await _context.BinhLuans
                .FirstOrDefaultAsync(b => b.MaSp == MaSp && b.MaKh == maKH);

            if (binhLuanCu != null)
            {
                TempData["Message"] = "Bạn đã đánh giá sản phẩm này rồi.";
                var spRated = await _context.SanPhams.FindAsync(MaSp);
                return RedirectToAction("ChiTietSanPham", new { slug = spRated?.Slug ?? "" });
            }

            var binhLuan = new BinhLuan
            {
                MaKh = maKH.Value,
                MaSp = MaSp,
                SoSao = Rating,
                NoiDung = NoiDung,
                Ngay = DateTime.Now,
                TrangThai = 1
            };

            _context.BinhLuans.Add(binhLuan);
            await _context.SaveChangesAsync();

            TempData["Message"] = "Đánh giá đã được gửi thành công!";

            var sp = await _context.SanPhams.FindAsync(MaSp);
            return RedirectToAction("ChiTietSanPham", new { slug = sp?.Slug ?? "" });
        }


        //
        public async Task<IActionResult> SanPhamChamDiem()
        {
            // Lấy ID khách hàng từ session
            var maKH = GetLoggedInKhachHangId();

            // Kiểm tra nếu maKH là null, có nghĩa là người dùng chưa đăng nhập
            if (!maKH.HasValue)
            {
                ViewBag.Message = "Vui lòng đăng nhập để xem sản phẩm chấm điểm.";
                return View(); // Trả về view hiện tại mà không chuyển hướng
            }

            // Lấy danh sách sản phẩm đã được khách hàng chấm điểm
            var sanPhamChamDiem = await _context.BinhLuans
                .Include(d => d.MaKhNavigation)  // Liên kết với bảng KhachHang
                .Include(d => d.MaSpNavigation)    // Liên kết với bảng SanPham
                .Where(d => d.MaKh == maKH.Value)  // Lọc theo MaKH
                .ToListAsync();

            // Trả về View và gửi thông báo nếu không có dữ liệu
            if (sanPhamChamDiem == null || !sanPhamChamDiem.Any())
            {
                ViewBag.Message = "Không có sản phẩm chấm điểm.";
            }

            return View(sanPhamChamDiem); // Trả về view và danh sách sản phẩm đã chấm điểm
        }
        //sửa chấm điểm 
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
