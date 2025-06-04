using Fruitful_Gifts.Database;
using Fruitful_Gifts.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
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
            int pageSize = 3;

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
                                ct.MaDhNavigation.TrangThai == 4)
                   .Where(ct => !_context.BinhLuans.Any(bl =>
                       bl.MaSp == sanPham.MaSp &&
                       bl.MaKh == userId &&
                       bl.MaDh == ct.MaDh))
                   .Select(ct => ct.MaDh)
                   .Distinct()
                   .ToList();

            }

            ViewBag.DaMua = daMua;
            ViewBag.DaDanhGia = daDanhGia;
            ViewBag.DonHangChuaDanhGia = donHangChuaDanhGia;

            double trungBinhSoSao = sanPham.BinhLuans?
                .Where(b => b.TrangThai == 1 && b.SoSao != null)
                .Select(b => b.SoSao.Value)
                .DefaultIfEmpty(0)
                .Average() ?? 0;

            int soLuotYeuThich = _context.SanPhamYeuThiches
                .Count(spy => spy.MaSp == sanPham.MaSp);

            var sanPhamLienQuan = _context.SanPhams
                .Where(p => p.MaLoai == sanPham.MaLoai &&
                            p.MaSp != sanPham.MaSp &&
                            p.TrangThai == 1 &&
                            p.GiaBan != null && sanPham.GiaBan != null &&
                            Math.Abs(p.GiaBan.Value - sanPham.GiaBan.Value) <= sanPham.GiaBan.Value * 0.2m)
                .OrderByDescending(p => p.GiaBan)
                .Take(4)
                .ToList();

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

        [HttpGet("san-pham/yeu-thich")]
        public IActionResult DanhSachSanPhamYeuThich(int page = 1)
        {
            var userId = GetLoggedInKhachHangId();
            if (userId == null)
                return RedirectToAction("DangNhap", "TaiKhoan");

            ViewBag.TrangThaiDangNhap = true; // Gán biến trạng thái đăng nhập

            int pageSize = 6;

            var sanPhamYeuThich = _context.SanPhamYeuThiches
                .Where(x => x.MaKh == userId && x.MaSp != null)
                .Include(x => x.MaSpNavigation)
                    .ThenInclude(sp => sp.MaLoaiNavigation)
                .Include(x => x.MaSpNavigation.MaNccNavigation)
                .ToList()
                .Select(x => x.MaSpNavigation)
                .Where(sp => sp.TrangThai == 1);

            var gioQuaYeuThich = _context.SanPhamYeuThiches
                .Where(x => x.MaKh == userId && x.MaGq != null)
                .Include(x => x.MaGqNavigation)
                .ToList()
                .Select(x => x.MaGqNavigation)
                .Where(gq => gq.TrangThai == 1);

            var danhSachYeuThich = sanPhamYeuThich.Cast<object>()
                .Concat(gioQuaYeuThich.Cast<object>())
                .ToList();

            int totalItems = danhSachYeuThich.Count();
            var items = danhSachYeuThich
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            ViewBag.TotalPages = (int)Math.Ceiling((double)totalItems / pageSize);
            ViewBag.CurrentPage = page;

            return View(items);
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

            using (var reader = new StreamReader(Request.Body))
            {
                var body = await reader.ReadToEndAsync();
                var data = System.Text.Json.JsonDocument.Parse(body);
                if (!data.RootElement.TryGetProperty("id", out var idProp) ||
                    !data.RootElement.TryGetProperty("loai", out var loaiProp))
                {
                    return Json(new { success = false, message = "Thiếu dữ liệu." });
                }

                var id = idProp.GetInt32();
                var loai = loaiProp.GetString();

                SanPhamYeuThich? yeuThich = null;

                if (loai == "sanpham")
                {
                    yeuThich = _context.SanPhamYeuThiches
                        .FirstOrDefault(sp => sp.MaKh == userId && sp.MaSp == id);
                }
                else if (loai == "gioqua")
                {
                    yeuThich = _context.SanPhamYeuThiches
                        .FirstOrDefault(sp => sp.MaKh == userId && sp.MaGq == id);
                }

                if (yeuThich != null)
                {
                    _context.SanPhamYeuThiches.Remove(yeuThich);
                    await _context.SaveChangesAsync();
                    return Json(new { success = true, message = "Đã xóa khỏi danh sách yêu thích." });
                }

                return Json(new { success = false, message = "Không tìm thấy mục yêu thích." });
            }
        }

        [HttpPost]
        public IActionResult ThemSanPhamYeuThich(int productId, string loai)
        {
            var userId = GetLoggedInKhachHangId();
            if (userId == null)
                return Json(new { success = false, message = "Vui lòng đăng nhập" });

            var existingItem = loai == "gioqua"
                ? _context.SanPhamYeuThiches.FirstOrDefault(x => x.MaKh == userId && x.MaGq == productId)
                : _context.SanPhamYeuThiches.FirstOrDefault(x => x.MaKh == userId && x.MaSp == productId);

            if (existingItem != null)
            {
                // Xóa nếu đã tồn tại
                _context.SanPhamYeuThiches.Remove(existingItem);
                _context.SaveChanges();

                var newCount = loai == "gioqua"
                    ? _context.SanPhamYeuThiches.Count(x => x.MaGq == productId)
                    : _context.SanPhamYeuThiches.Count(x => x.MaSp == productId);

                return Json(new
                {
                    success = true,
                    isAdded = false,
                    newLikeCount = newCount,
                    message = "Đã xóa khỏi danh sách yêu thích"
                });
            }
            else
            {
                // Thêm mới
                var newItem = new SanPhamYeuThich
                {
                    MaKh = userId.Value,

                };

                if (loai == "gioqua")
                    newItem.MaGq = productId;
                else
                    newItem.MaSp = productId;

                _context.SanPhamYeuThiches.Add(newItem);
                _context.SaveChanges();

                var newCount = loai == "gioqua"
                    ? _context.SanPhamYeuThiches.Count(x => x.MaGq == productId)
                    : _context.SanPhamYeuThiches.Count(x => x.MaSp == productId);

                return Json(new
                {
                    success = true,
                    isAdded = true,
                    newLikeCount = newCount,
                    message = "Đã thêm vào danh sách yêu thích"
                });
            }
        }
        private int GetLikeCount(int productId, string loai)
        {
            if (loai == "sanpham")
            {
                return _context.SanPhamYeuThiches.Count(x => x.MaSp == productId);
            }
            else if (loai == "gioqua")
            {
                return _context.SanPhamYeuThiches.Count(x => x.MaGq == productId);
            }

            return 0;
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
            var daDanhGiaDonHangNay = await _context.BinhLuans
                .AnyAsync(b => b.MaKh == maKH &&
                               b.MaGq == MaGq &&
                               b.MaSp == MaSp &&
                               b.MaGq == (MaGq.HasValue ? MaGq : null) &&
                               b.MaSp == (MaSp.HasValue ? MaSp : null));

            if (daDanhGiaDonHangNay)
            {
                TempData["Message"] = "Bạn đã đánh giá sản phẩm/giỏ quà này cho đơn hàng này rồi.";
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
                .Include(d => d.MaGqNavigation)
                .Where(d => d.MaKh == maKH.Value)
                .OrderByDescending(d => d.NgayBinhLuan);

            int totalItems = await query.CountAsync();

            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling((double)totalItems / pageSize);

            //if (!items.Any())
            //{
            //    TempData["InfoMessage"] = "Không có sản phẩm hoặc giỏ quà chấm điểm.";
            //}

            return View(items);
        }


        public async Task<IActionResult> SuaChamDiem(int? sanPhamId, int? gioQuaId)
        {
            var maKH = GetLoggedInKhachHangId();

            if (!maKH.HasValue)
            {
                TempData["Message"] = "Vui lòng đăng nhập để sửa chấm điểm.";
                return RedirectToAction("Index", "TrangChu");
            }

            BinhLuan? chiTietBinhLuan = null;
            if (sanPhamId.HasValue)
            {
                chiTietBinhLuan = await _context.BinhLuans
                    .FirstOrDefaultAsync(c => c.MaKh == maKH.Value && c.MaSp == sanPhamId);
            }
            else if (gioQuaId.HasValue)
            {
                chiTietBinhLuan = await _context.BinhLuans
                    .FirstOrDefaultAsync(c => c.MaKh == maKH.Value && c.MaGq == gioQuaId);
            }

            if (chiTietBinhLuan == null)
            {
                TempData["Message"] = "Không tìm thấy đánh giá.";
                return RedirectToAction("SanPhamChamDiem");
            }

            return View(chiTietBinhLuan);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SuaChamDiem(int? sanPhamId, int? gioQuaId, int rating, string comment)
        {
            var maKH = GetLoggedInKhachHangId();

            if (!maKH.HasValue)
            {
                TempData["Message"] = "Vui lòng đăng nhập để sửa chấm điểm.";
                return RedirectToAction("Index", "TrangChu");
            }

            BinhLuan? chiTietBinhLuan = null;
            if (sanPhamId.HasValue)
            {
                chiTietBinhLuan = await _context.BinhLuans
                    .FirstOrDefaultAsync(c => c.MaKh == maKH.Value && c.MaSp == sanPhamId);
            }
            else if (gioQuaId.HasValue)
            {
                chiTietBinhLuan = await _context.BinhLuans
                    .FirstOrDefaultAsync(c => c.MaKh == maKH.Value && c.MaGq == gioQuaId);
            }

            if (chiTietBinhLuan == null)
            {
                TempData["Message"] = "Không tìm thấy đánh giá.";
                return RedirectToAction("SanPhamChamDiem");
            }

            chiTietBinhLuan.SoSao = rating;
            chiTietBinhLuan.NoiDung = comment;
            chiTietBinhLuan.UpdatedAt = DateTime.Now;

            _context.BinhLuans.Update(chiTietBinhLuan);
            await _context.SaveChangesAsync();

            TempData["Message"] = "Cập nhật chấm điểm thành công!";

            return RedirectToAction("SanPhamChamDiem");
        }




    }
}
