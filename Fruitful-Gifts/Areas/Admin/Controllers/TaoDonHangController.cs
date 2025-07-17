using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Fruitful_Gifts.Database;
using System.Linq;
using System.Collections.Generic;
using System;
using Fruitful_Gifts.Helpers;
using Microsoft.AspNetCore.Authorization;

namespace Fruitful_Gifts.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class TaoDonHangController : Controller
    {
        private readonly FruitfulGiftsContext _context;

        public TaoDonHangController(FruitfulGiftsContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            // Lấy danh sách sản phẩm và giỏ quà để hiển thị trong dropdown
            ViewBag.SanPhams = _context.SanPhams.Where(sp => sp.TrangThai == 1).ToList();
            ViewBag.GioQuas = _context.GioQuas.Where(gq => gq.TrangThai == 1).ToList();
            ViewBag.PhuongThucThanhToans = _context.PhuongThucThanhToans.ToList();

            return View();
        }

        [HttpPost]
        public IActionResult CreateOrder([FromForm] DonHang donHang, [FromForm] List<ChiTietDonHang> chiTietDonHangs)
        {
            using (var transaction = _context.Database.BeginTransaction())
            {
                try
                {
                    var maNv = HttpContext.Session.GetInt32("MaNv");
                    var userQuyen = HttpContext.Session.GetString("Quyen");
                    var userVaiTro = HttpContext.Session.GetString("VaiTro");

                    Console.WriteLine($"Debug - MaNv: {maNv}, Quyen: {userQuyen}, VaiTro: {userVaiTro}");

                    // Danh sách các quyền được phép tạo đơn hàng
                    var allowedQuyens = new List<string> { "admin", "quanly", "thungan", "banhang" };

                    // Kiểm tra quyền: Chỉ các quyền được phép mới được tạo đơn
                    if (!allowedQuyens.Contains(userQuyen))
                    {
                        return Json(new { success = false, message = "Bạn không có quyền thực hiện thao tác này" });
                    }

                    // Kiểm tra tồn kho trước khi tạo đơn
                    foreach (var item in chiTietDonHangs)
                    {
                        if (item.MaSp != null)
                        {
                            // Kiểm tra tồn kho sản phẩm đơn lẻ
                            var tonKho = _context.KhoHangs
                                .Where(k => k.MaSp == item.MaSp)
                                .Select(k => k.SoLuongTon)
                                .FirstOrDefault();

                            if (tonKho < item.SoLuong)
                            {
                                transaction.Rollback();
                                var tenSanPham = _context.SanPhams
                                    .Where(p => p.MaSp == item.MaSp)
                                    .Select(p => p.TenSp)
                                    .FirstOrDefault();

                                return Json(new
                                {
                                    success = false,
                                    message = $"{tenSanPham} không đủ số lượng tồn kho (còn {tonKho})"
                                });
                            }
                        }
                        else if (item.MaGq != null)
                        {
                            // Kiểm tra tồn kho cho giỏ quà
                            var chiTietGioQua = _context.ChiTietGioQuas
                                .Where(ct => ct.MaGq == item.MaGq)
                                .Select(ct => new {
                                    ct.MaSp,
                                    ct.SoLuong
                                })
                                .ToList();

                            foreach (var ct in chiTietGioQua)
                            {
                                var tonKhoSp = _context.KhoHangs
                                    .Where(k => k.MaSp == ct.MaSp)
                                    .Select(k => k.SoLuongTon)
                                    .FirstOrDefault();

                                // Số lượng cần cho tất cả giỏ quà đặt hàng
                                var soLuongCan = ct.SoLuong * item.SoLuong;

                                if (tonKhoSp < soLuongCan)
                                {
                                    transaction.Rollback();
                                    var tenGioQua = _context.GioQuas
                                        .Where(g => g.MaGq == item.MaGq)
                                        .Select(g => g.TenGioQua)
                                        .FirstOrDefault();
                                    var tenSanPham = _context.SanPhams
                                        .Where(p => p.MaSp == ct.MaSp)
                                        .Select(p => p.TenSp)
                                        .FirstOrDefault();

                                    return Json(new
                                    {
                                        success = false,
                                        message = $"{tenGioQua}: Không đủ {tenSanPham} (cần {soLuongCan}, còn {tonKhoSp})"
                                    });
                                }
                            }
                        }
                        else
                        {
                            transaction.Rollback();
                            return Json(new { success = false, message = "Mỗi sản phẩm phải có mã sản phẩm hoặc mã giỏ quà" });
                        }
                    }

                    // Thiết lập thông tin đơn hàng
                    donHang.MaNv = maNv;
                    donHang.NgayDatHang = DateTime.Now;
                    donHang.CreatedAt = DateTime.Now;
                    donHang.UpdatedAt = DateTime.Now;
                    donHang.TrangThai = 4; // Trạng thái "Đã giao hàng"
                    donHang.TrangThaiThanhToan = 1; // Trạng thái "Đã thanh toán"
                    donHang.PhuongThucBan = "offline";

                    // Tính tổng tiền
                    decimal tongTien = 0;
                    foreach (var item in chiTietDonHangs)
                    {
                        if (item.MaSp != null)
                        {
                            var sanPham = _context.SanPhams
                            .Include(sp => sp.MaLoaiNavigation) // Thêm include để lấy thông tin loại
                            .FirstOrDefault(sp => sp.MaSp == item.MaSp);

                            if (sanPham == null)
                            {
                                transaction.Rollback();
                                return Json(new { success = false, message = $"Sản phẩm với mã {item.MaSp} không tồn tại" });
                            }

                            // Kiểm tra nếu KHÔNG phải là trái cây (mã loại 2) thì số lượng phải là số nguyên
                            if (sanPham.MaLoai != 2 && item.SoLuong % 1 != 0)
                            {
                                transaction.Rollback();
                                return Json(new
                                {
                                    success = false,
                                    message = $"{sanPham.TenSp} phải có số lượng là số nguyên"
                                });
                            }

                            item.GiaBan = sanPham.GiaBan;
                        }
                        else if (item.MaGq != null)
                        {
                            var gioQua = _context.GioQuas.Find(item.MaGq);
                            if (gioQua == null)
                            {
                                transaction.Rollback();
                                return Json(new { success = false, message = $"Giỏ quà với mã {item.MaGq} không tồn tại" });
                            }
                            item.GiaBan = gioQua.GiaBan;
                        }
                        else
                        {
                            transaction.Rollback();
                            return Json(new { success = false, message = "Mỗi sản phẩm phải có mã sản phẩm hoặc mã giỏ quà" });
                        }

                        item.TongTienTungSanPham = item.GiaBan * (decimal?)item.SoLuong;
                        tongTien += item.TongTienTungSanPham ?? 0;
                    }

                    donHang.TongTienDonHang = tongTien + (donHang.PhiVanChuyenBanHang ?? 0);

                    // Thêm đơn hàng
                    _context.DonHangs.Add(donHang);
                    _context.SaveChanges();

                    // Thêm chi tiết đơn hàng
                    foreach (var item in chiTietDonHangs)
                    {
                        item.MaDh = donHang.MaDh;
                        _context.ChiTietDonHangs.Add(item);
                    }
                    _context.SaveChanges();

                    // Trừ số lượng sản phẩm trong kho
                    foreach (var item in chiTietDonHangs)
                    {
                        if (item.MaSp != null)
                        {
                            // Trừ sản phẩm đơn lẻ
                            var khoHang = _context.KhoHangs.FirstOrDefault(k => k.MaSp == item.MaSp);
                            if (khoHang != null)
                            {
                                khoHang.SoLuongTon -= item.SoLuong ?? 0.0;
                                if (khoHang.SoLuongTon < 0) // Đảm bảo không âm
                                    khoHang.SoLuongTon = 0;
                            }
                        }
                        else if (item.MaGq != null)
                        {
                            // Trừ các sản phẩm trong giỏ quà
                            var chiTietGioQua = _context.ChiTietGioQuas
                                .Where(ct => ct.MaGq == item.MaGq)
                                .ToList();

                            foreach (var ct in chiTietGioQua)
                            {
                                var khoHang = _context.KhoHangs.FirstOrDefault(k => k.MaSp == ct.MaSp);
                                if (khoHang != null)
                                {
                                    khoHang.SoLuongTon -= ((ct.SoLuong * item.SoLuong) ?? 0.0);
                                    if (khoHang.SoLuongTon < 0) // Đảm bảo không âm
                                        khoHang.SoLuongTon = 0;
                                }
                            }
                        }
                    }

                    // Lưu thay đổi vào kho
                    _context.SaveChanges();

                    transaction.Commit();
                    return Json(new { success = true, message = "Tạo đơn hàng thành công", orderId = donHang.MaDh });
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    return Json(new { success = false, message = "Lỗi khi tạo đơn hàng: " + ex.Message });
                }
            }
        }

        [HttpGet]
        public IActionResult GetProducts(int page = 1, int pageSize = 9, string search = "")
        {
            var query = _context.SanPhams
                .Where(sp => sp.TrangThai == 1)
                .Select(sp => new {
                    sp.MaSp,
                    sp.TenSp,
                    sp.GiaBan,
                    sp.HinhAnh,
                    sp.MaLoai,
                    SoLuongTon = _context.KhoHangs
                        .Where(k => k.MaSp == sp.MaSp)
                        .Select(k => k.SoLuongTon)
                        .FirstOrDefault()
                });

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(sp => sp.TenSp.Contains(search));
            }

            // Phần còn lại giữ nguyên
            var totalProducts = query.Count();
            var products = query
                .OrderBy(sp => sp.MaSp)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            return Json(new
            {
                data = products,
                currentPage = page,
                totalPages = (int)Math.Ceiling((double)totalProducts / pageSize),
                totalItems = totalProducts
            });
        }

        [HttpGet]
        public IActionResult GetGifts(int page = 1, int pageSize = 9, string search = "")
        {
            var query = _context.GioQuas
                .Where(gq => gq.TrangThai == 1)
                .Select(gq => new {
                    gq.MaGq,
                    gq.TenGioQua,
                    gq.GiaBan,
                    gq.HinhAnh,
                    SoLuongTon = _context.ChiTietGioQuas
                    .Where(ct => ct.MaGq == gq.MaGq)
                    .Join(_context.KhoHangs,
                        ct => ct.MaSp,
                        kho => kho.MaSp,
                        (ct, kho) => new {
                            Required = ct.SoLuong,
                            Available = kho.SoLuongTon
                        })
                    .GroupBy(x => 1)
                    .Select(g => g.Min(x => (int)(x.Available / x.Required)))
                    .FirstOrDefault()
                });

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(gq => gq.TenGioQua.Contains(search));
            }

            // Phần còn lại giữ nguyên
            var totalGifts = query.Count();
            var gifts = query
                .OrderBy(gq => gq.MaGq)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            return Json(new
            {
                data = gifts,
                currentPage = page,
                totalPages = (int)Math.Ceiling((double)totalGifts / pageSize),
                totalItems = totalGifts
            });
        }
    }
}