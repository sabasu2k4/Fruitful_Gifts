using Fruitful_Gifts.Database;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Fruitful_Gifts.Controllers
{
    public class GioHangController : Controller
    {
        private readonly FruitfulGiftsContext _context;

        public GioHangController(FruitfulGiftsContext context)
        {
            _context = context;
        }
        public decimal TinhTongTienDaChon(int maKhachHang)
        {
            try
            {
                return _context.ChiTietGioHangs
                    .Where(x => x.MaKh == maKhachHang && x.TrangThai == 1)
                    .ToList() // Force execute query trước
                    .Sum(x => (x.SoLuong ?? 0) * (x.MaSpNavigation?.GiaBan ?? x.MaGqNavigation?.GiaBan ?? 0m));
            }
            catch
            {
                return 0; // Trả về 0 nếu có lỗi
            }
        }
        //
        [HttpPost]
        public IActionResult CapNhatTatCaTrangThai(bool trangThai)
        {
            int? maKh = HttpContext.Session.GetInt32("MaKh");
            if (maKh == null)
                return Json(new { success = false, message = "Chưa đăng nhập" });

            var items = _context.ChiTietGioHangs
                .Where(x => x.MaKh == maKh)
                .ToList();

            foreach (var item in items)
            {
                item.TrangThai = trangThai ? 1 : 0;
            }

            _context.SaveChanges();

            decimal tongTien = TinhTongTien(maKh);
            return Json(new
            {
                success = true,
                tongTienMoi = tongTien.ToString("N0") + "₫"
            });
        }
        //
        [HttpGet]
        public IActionResult KiemTraGioHang()
        {
            int? maKh = HttpContext.Session.GetInt32("MaKh");
            if (maKh == null)
                return Json(new { success = false, message = "Chưa đăng nhập" });

            var coSanPhamChon = _context.ChiTietGioHangs
                .Any(x => x.MaKh == maKh && x.TrangThai == 1);

            var tongTien = TinhTongTienDaChon(maKh.Value);

            return Json(new
            {
                success = true,
                coSanPhamChon = coSanPhamChon,
                tongTien = tongTien
            });
        }
        public IActionResult Index()
        {
            int? maKh = HttpContext.Session.GetInt32("MaKh");

            if (maKh == null)
            {
                return RedirectToAction("DangNhap", "TaiKhoan");
            }

            var danhSach = _context.ChiTietGioHangs
                .Include(x => x.MaSpNavigation)
                .Include(x => x.MaGqNavigation)
                .Where(c => c.MaKh == maKh)
                .ToList();

            decimal tongTien = danhSach.Sum(x =>
                (x.SoLuong ?? 0) * (
                    x.MaGqNavigation?.GiaBan ?? x.MaSpNavigation?.GiaBan ?? 0m
                )
            );

            // Dòng này sẽ ghi ra Output Window trong Visual Studio khi chạy Debug
            System.Diagnostics.Debug.WriteLine($"Tong tien da chon: {tongTien}");

            ViewBag.DanhSach = danhSach;
            ViewBag.TongTien = tongTien;
            ViewBag.CheckALl = danhSach.All(x => x.TrangThai == 1);

            return View(danhSach);
        }


        [HttpPost]
        public IActionResult ThemVaoGio(int? maSp, int? maGq, int soLuong = 1)
        {
            int? maKh = HttpContext.Session.GetInt32("MaKh");

            if (maKh == null)
            {
                return Json(new { success = false, message = "Bạn cần đăng nhập để mua hàng." });
            }

            if (soLuong <= 0)
                soLuong = 1;

            ChiTietGioHang? gioHangItem = null;

            if (maSp != null)
            {
                gioHangItem = _context.ChiTietGioHangs
                    .FirstOrDefault(c => c.MaKh == maKh && c.MaSp == maSp && c.MaGq == null);

                if (gioHangItem != null)
                {
                    gioHangItem.SoLuong = (gioHangItem.SoLuong ?? 0) + soLuong;
                }
                else
                {
                    gioHangItem = new ChiTietGioHang
                    {
                        MaKh = maKh.Value,
                        MaSp = maSp,
                        SoLuong = soLuong,
                        TrangThai = 1,
                        CreatedAt = DateTime.Now
                    };
                    _context.ChiTietGioHangs.Add(gioHangItem);
                }
            }
            else if (maGq != null)
            {
                gioHangItem = _context.ChiTietGioHangs
                    .FirstOrDefault(c => c.MaKh == maKh && c.MaGq == maGq && c.MaSp == null);

                if (gioHangItem != null)
                {
                    gioHangItem.SoLuong = (gioHangItem.SoLuong ?? 0) + soLuong;
                }
                else
                {
                    gioHangItem = new ChiTietGioHang
                    {
                        MaKh = maKh.Value,
                        MaGq = maGq,
                        SoLuong = soLuong,
                        TrangThai = 1,
                        CreatedAt = DateTime.Now
                    };
                    _context.ChiTietGioHangs.Add(gioHangItem);
                }
            }
            else
            {
                return Json(new { success = false, message = "Không xác định được sản phẩm hoặc giỏ quà." });
            }

            _context.SaveChanges();

            var tongSoLuong = _context.ChiTietGioHangs
                .Where(c => c.MaKh == maKh)
                .Sum(c => c.SoLuong) ?? 0;

            return Json(new
            {
                success = true,
                tongSoLuong = tongSoLuong
            });
        }

        [HttpPost]
        public IActionResult CapNhatSoLuongAjax(int id, string loai, string type)
        {
            int? maKh = HttpContext.Session.GetInt32("MaKh");

            ChiTietGioHang gioHang = null;

            if (loai == "gq")
            {
                gioHang = _context.ChiTietGioHangs
                    .Include(x => x.MaGqNavigation)
                    .FirstOrDefault(x => x.MaGq == id && x.MaKh == maKh);
            }
            else if (loai == "sp")
            {
                gioHang = _context.ChiTietGioHangs
                    .Include(x => x.MaSpNavigation)
                    .FirstOrDefault(x => x.MaSp == id && x.MaKh == maKh);
            }

            if (gioHang != null)
            {
                if (type == "tang") gioHang.SoLuong += 1;
                else if (type == "giam") gioHang.SoLuong = Math.Max((gioHang.SoLuong ?? 1) - 1, 1);

                _context.SaveChanges();

                decimal gia = 0;
                if (loai == "gq") gia = gioHang.MaGqNavigation?.GiaBan ?? 0;
                else if (loai == "sp") gia = gioHang.MaSpNavigation?.GiaBan ?? 0;

                var thanhTien = (gioHang.SoLuong ?? 0) * gia;
                var tongTien = TinhTongTien(maKh); // bạn tự định nghĩa
                var tongSoLuong = _context.ChiTietGioHangs
                    .Where(x => x.MaKh == maKh)
                    .Sum(x => x.SoLuong ?? 0);

                return Json(new
                {
                    success = true,
                    soLuongMoi = gioHang.SoLuong,
                    thanhTien = thanhTien.ToString("N0") + "₫",
                    tongTien = tongTien.ToString("N0") + "₫",
                    tongSoLuongMoi = tongSoLuong
                });
            }

            return Json(new { success = false });
        }

        [HttpPost]
        public IActionResult XoaItem(int id, string type)
        {
            int? maKh = HttpContext.Session.GetInt32("MaKh");
            if (maKh == null)
                return Json(new { success = false, message = "Bạn cần đăng nhập" });

            if (id == null)
                return Json(new { success = false, message = "ID không hợp lệ." });

            ChiTietGioHang? item = null;

            if (type == "gq")
            {
                item = _context.ChiTietGioHangs.FirstOrDefault(c => c.MaKh == maKh && c.MaGq == id);
            }
            else if (type == "sp")
            {
                item = _context.ChiTietGioHangs.FirstOrDefault(c => c.MaKh == maKh && c.MaSp == id);
            }
            else
            {
                return Json(new { success = false, message = "Loại mục không hợp lệ." });
            }

            if (item == null)
                return Json(new { success = false, message = "Không tìm thấy mục để xóa." });

            int soLuongBiTru = item.SoLuong ?? 0;

            _context.ChiTietGioHangs.Remove(item);
            _context.SaveChanges();

            var tongTien = TinhTongTien(maKh);

            return Json(new { success = true, soLuongBiTru = soLuongBiTru, tongTienMoi = tongTien });
        }

        [HttpPost]
        public IActionResult XoaTatCaGioHang()
        {
            int? maKh = HttpContext.Session.GetInt32("MaKh");

            var danhSach = _context.ChiTietGioHangs.Where(c => c.MaKh == maKh).ToList();

            if (danhSach.Any())
            {
                _context.ChiTietGioHangs.RemoveRange(danhSach);
                _context.SaveChanges();
                return Json(new { success = true });
            }

            return Json(new { success = false, message = "Không có sản phẩm nào để xóa." });
        }

        private decimal TinhTongTien(int? maKh)
        {
            var gioHang = _context.ChiTietGioHangs
                .Where(x => x.MaKh == maKh && x.TrangThai == 1)
                .Include(x => x.MaGqNavigation)
                .Include(x => x.MaSpNavigation)
                .ToList();

            return gioHang.Sum(x =>
            {
                int soLuong = x.SoLuong ?? 0;
                decimal gia = x.MaGqNavigation?.GiaBan ?? x.MaSpNavigation?.GiaBan ?? 0;
                return soLuong * gia;
            });
        }

        [HttpPost]
        public IActionResult CapNhatTrangThai(int id, string loai, bool trangThai)
        {
            int? maKh = HttpContext.Session.GetInt32("MaKh");

            ChiTietGioHang? item = null;

            if (loai == "sp")
            {
                item = _context.ChiTietGioHangs.FirstOrDefault(c => c.MaKh == maKh && c.MaSp == id);
            }
            else if (loai == "gq")
            {
                item = _context.ChiTietGioHangs.FirstOrDefault(c => c.MaKh == maKh && c.MaGq == id);
            }

            if (item != null)
            {
                item.TrangThai = trangThai ? 1 : 0;
                _context.SaveChanges();

                decimal tongTien = TinhTongTien(maKh);
                return Json(new { success = true, tongTienMoi = tongTien.ToString("N0") + "₫" });
            }

            return Json(new { success = false, message = "Không tìm thấy sản phẩm để cập nhật." });
        }


    }
}
