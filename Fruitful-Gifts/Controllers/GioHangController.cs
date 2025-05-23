﻿using Fruitful_Gifts.Database;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace Fruitful_Gifts.Controllers
{
    public class GioHangController : Controller
    {
        private readonly FruitfulGiftsContext _context;

        public GioHangController(FruitfulGiftsContext context)
        {
            _context = context;
        }
        public IActionResult Index()
        {
            int? maKh = HttpContext.Session.GetInt32("MaKh");

            if (maKh == null)
            {
                return RedirectToAction("DangNhap", "TaiKhoan");
            }

            var danhSach = _context.ChiTietGioHangs
                .Include(c => c.MaSpNavigation)
                .Where(c => c.MaKh == maKh)
                .ToList();

            ViewBag.DanhSach = danhSach;

            decimal tongTien = danhSach.Sum(x =>
                (x.SoLuong ?? 0) * (x.MaSpNavigation.Gia ?? 0)
            );
            ViewBag.TongTien = tongTien;

            return View(danhSach);
        }

        [HttpPost]
        public IActionResult ThemVaoGio(int maSp)
        {
            int? maKh = HttpContext.Session.GetInt32("MaKh");

            if (maKh == null)
            {
                return Json(new { success = false, message = "Bạn cần đăng nhập để mua hàng." });
            }

            var gioHangItem = _context.ChiTietGioHangs
                .FirstOrDefault(c => c.MaKh == maKh && c.MaSp == maSp);

            if (gioHangItem != null)
            {
                gioHangItem.SoLuong = (gioHangItem.SoLuong ?? 0) + 1;
            }
            else
            {
                gioHangItem = new ChiTietGioHang
                {
                    MaKh = (int)maKh,
                    MaSp = maSp,
                    SoLuong = 1,
                    CreatedAt = DateTime.Now
                };
                _context.ChiTietGioHangs.Add(gioHangItem);
            }

            _context.SaveChanges();

            var tongSoLuong = _context.ChiTietGioHangs
                .Where(c => c.MaKh == maKh)
                .Sum(c => c.SoLuong);

            return Json(new
            {
                success = true,
                tongSoLuong = tongSoLuong
            });
        }

        [HttpPost]
        public IActionResult CapNhatSoLuongAjax(int id, string type)
        {
            int? maKh = HttpContext.Session.GetInt32("MaKh");
            var sp = _context.ChiTietGioHangs
                .Include(x => x.MaSpNavigation)
                .FirstOrDefault(x => x.MaSp == id && x.MaKh == maKh);

            if (sp != null)
            {
                if (type == "tang") sp.SoLuong += 1;
                else if (type == "giam") sp.SoLuong = Math.Max(sp.SoLuong.Value - 1, 1);

                _context.SaveChanges();

                var tongTien = TinhTongTien(maKh);
                var thanhTien = (sp.SoLuong ?? 0) * (sp.MaSpNavigation.Gia ?? 0);

                var tongSoLuongGioHang = _context.ChiTietGioHangs
                .Where(x => x.MaKh == maKh)
                .Sum(x => x.SoLuong ?? 0);

                return Json(new
                {
                    success = true,
                    soLuongMoi = sp.SoLuong,
                    tongTien = tongTien.ToString("N0") + "₫",
                    thanhTien = thanhTien.ToString("N0") + "₫",
                    tongSoLuongMoi = tongSoLuongGioHang
                });
            }

            return Json(new { success = false });
        }

        [HttpPost]
        public IActionResult XoaSanPham(int maSp)
        {
            int? maKh = HttpContext.Session.GetInt32("MaKh");
            var item = _context.ChiTietGioHangs
                .FirstOrDefault(c => c.MaKh == maKh && c.MaSp == maSp);

            int soLuongBiTru = item.SoLuong ?? 0;

            if (item != null)
            {
                _context.ChiTietGioHangs.Remove(item);
                _context.SaveChanges();
            }
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

        private int TinhTongTien(int? maKh)
        {
            return (int)_context.ChiTietGioHangs
                .Where(x => x.MaKh == maKh)
                .Sum(x => (x.SoLuong ?? 0) * (x.MaSpNavigation.Gia ?? 0));
        }
        [HttpPost]
        public IActionResult ThemVaoGioHang(int maSP, int soLuong)
        {
            // Kiểm tra đăng nhập
            var KH_JSON = HttpContext.Session.GetString("user");
            if (string.IsNullOrEmpty(KH_JSON))
            {
                return StatusCode(401, new { success = false, message = "Bạn cần đăng nhập!" });
            }

            var thongTinkhachHang = JsonSerializer.Deserialize<KhachHang>(KH_JSON);
            if (thongTinkhachHang?.MaKh == null)
            {
                return BadRequest(new { success = false, message = "Không thể xác định khách hàng." });
            }

            var maKH = thongTinkhachHang.MaKh;

            // Lấy thông tin sản phẩm
            var sanPham = _context.SanPhams
              .FirstOrDefault(sp => sp.MaSp == maSP && sp.TrangThai == 1);
            if (sanPham == null)
            {
                return NotFound(new { success = false, message = "Sản phẩm không tồn tại hoặc đã bị ẩn." });
            }

            if (soLuong <= 0)
            {
                return BadRequest(new { success = false, message = "Số lượng không hợp lệ." });
            }

            if (soLuong > sanPham.SoLuong)
            {
                return BadRequest(new { success = false, message = "Sản phẩm không đủ tồn kho." });
            }

            // Kiểm tra sản phẩm đã có trong giỏ hàng chưa
            var gioHangItem = _context.ChiTietGioHangs
                .FirstOrDefault(x => x.MaKh == maKH && x.MaSp == maSP);

            if (gioHangItem != null)
            {
                gioHangItem.SoLuong += soLuong;

                if (gioHangItem.SoLuong > sanPham.SoLuong)
                {
                    return BadRequest(new { success = false, message = "Tổng số lượng vượt tồn kho!" });
                }

                _context.ChiTietGioHangs.Update(gioHangItem);
            }
            else
            {
                var gioHangMoi = new ChiTietGioHang
                {
                    MaKh = maKH,
                    MaSp = maSP,
                    SoLuong = soLuong,
                    CreatedAt = DateTime.Now
                };

                _context.ChiTietGioHangs.Add(gioHangMoi);
            }

            _context.SaveChanges();

            return Ok(new { success = true, message = "Đã thêm vào giỏ hàng!" });
        }
    }
}
