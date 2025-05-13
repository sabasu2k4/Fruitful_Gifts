using Fruitful_Gifts.Database;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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
                .FirstOrDefault(sp => sp.Slug == slug && sp.TrangThai == true);

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
            var danhGiaTB = (sanPham.BinhLuans != null && sanPham.BinhLuans.Any(b => b.IsHienThi == true))
                ? sanPham.BinhLuans.Where(b => b.IsHienThi == true).Average(b => b.SoSao ?? 0)
                : 0;
            ViewBag.TrungBinhSoSao = danhGiaTB;

            // Số lượt yêu thích
            var soLuotYeuThich = _context.SanPhamYeuThiches
                .Count(spy => spy.MaSp == sanPham.MaSp);
            ViewBag.SoLuotYeuThich = soLuotYeuThich;

            // Sản phẩm liên quan
            var lienQuan = _context.SanPhams
                .Where(p => p.MaDm == sanPham.MaDm && p.MaSp != sanPham.MaSp && p.TrangThai == true)
                .OrderByDescending(p => p.Gia)
                .Take(4) //số lượng hiện thị 
                .ToList();
            ViewBag.SanPhamLienQuan = lienQuan;

            ViewBag.DaMua = daMua;
            ViewBag.TrangThaiDangNhap = userId != null;

            return View(sanPham);
        }


        //realtime sản phẩm 
        [HttpGet]
        public IActionResult GetQuantity(int maSp)
        {
            var product = _context.SanPhams.FirstOrDefault(x => x.MaSp == maSp);
            if (product == null) return NotFound();

            return Json(new { soLuong = product.SoLuong });
        }

        [HttpPost]
        public IActionResult AddToFavorites(int maSp)
        {
            var userId = GetLoggedInKhachHangId();

            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var sanPhamYeuThich = _context.SanPhamYeuThiches
                .FirstOrDefault(spy => spy.MaKh == userId && spy.MaSp == maSp);

            if (sanPhamYeuThich == null)
            {
                var sanPhamMoi = new SanPhamYeuThich
                {
                    MaKh = userId.Value,
                    MaSp = maSp
                };
                _context.SanPhamYeuThiches.Add(sanPhamMoi);
                _context.SaveChanges();
            }

            var soLuotYeuThich = _context.SanPhamYeuThiches
                .Count(spy => spy.MaSp == maSp);

            return Json(new { soLuotYeuThich });
        }

        public IActionResult RemoveFromFavorites(int maSp)
        {
            var userId = GetLoggedInKhachHangId();

            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var sanPhamYeuThich = _context.SanPhamYeuThiches
                .FirstOrDefault(spy => spy.MaKh == userId && spy.MaSp == maSp);

            if (sanPhamYeuThich != null)
            {
                _context.SanPhamYeuThiches.Remove(sanPhamYeuThich);
                _context.SaveChanges();
            }

            return RedirectToAction("ChiTietSanPham", new { slug = "some-slug" });
        }


        private int? GetLoggedInKhachHangId()
        {
            if (HttpContext.Session.TryGetValue("KhachHangId", out var bytes))
            {
                return BitConverter.ToInt32(bytes, 0);
            }
            return null;
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

    }
}
