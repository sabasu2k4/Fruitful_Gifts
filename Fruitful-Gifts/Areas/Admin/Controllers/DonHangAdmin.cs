using Fruitful_Gifts.Database;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using ClosedXML.Excel;
using System.IO;
using Fruitful_Gifts.ViewModels;

namespace Fruitful_Gifts.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class DonHangAdminController : Controller
    {
        private readonly FruitfulGiftsContext _context;

        public DonHangAdminController(FruitfulGiftsContext context)
        {
            _context = context;
        }

        // Action để hiển thị form tạo đơn hàng offline
        [HttpGet]
        public IActionResult TaoDonHangOffline()
        {
            // Lấy danh sách sản phẩm và giỏ quà có sẵn
            var sanPhams = _context.SanPhams
                .Include(sp => sp.KhoHangs)
                .Where(sp => sp.TrangThai == 1 && sp.KhoHangs.Sum(k => k.SoLuongTon) > 0)
                .ToList();

            var gioQuas = _context.GioQuas
                .Include(gq => gq.ChiTietGioQuas)
                .ThenInclude(ct => ct.MaSpNavigation)
                .ThenInclude(sp => sp.KhoHangs)
                .Where(gq => gq.TrangThai == 1)
                .ToList();

            // Tính số lượng tồn cho từng giỏ quà
            var gioQuaViewModels = gioQuas.Select(gq => new GioQuaViewModel
            {
                GioQua = gq,
                SoLuongTon = TinhSoLuongTonGioQua(gq.Slug)
            }).ToList();

            ViewBag.SanPhams = sanPhams;
            ViewBag.GioQuas = gioQuaViewModels;

            return View();
        }

        // Action xử lý tạo đơn hàng offline
        [HttpPost]
        public async Task<IActionResult> TaoDonHangOffline(DonHangOfflineViewModel model)
        {
            if (ModelState.IsValid)
            {
                using (var transaction = await _context.Database.BeginTransactionAsync())
                {
                    try
                    {
                        // Tạo đơn hàng mới
                        var donHang = new DonHang
                        {
                            MaNv = model.MaNv,
                            NgayDatHang = DateTime.Now,
                            TrangThai = 1, // Trạng thái đã đặt hàng
                            TrangThaiThanhToan = model.DaThanhToan ? 1 : 0,
                            PhuongThucBan = "Offline",
                            CreatedAt = DateTime.Now,
                            UpdatedAt = DateTime.Now,
                            SoDienThoai = model.SoDienThoai,
                            GhiChu = model.GhiChu
                        };

                        _context.DonHangs.Add(donHang);
                        await _context.SaveChangesAsync();

                        decimal tongTien = 0;
                        var chiTietDonHangs = new List<ChiTietDonHang>();

                        // Xử lý sản phẩm được chọn
                        if (model.SelectedSanPhams != null)
                        {
                            foreach (var sp in model.SelectedSanPhams)
                            {
                                var sanPham = await _context.SanPhams
                                    .Include(s => s.KhoHangs)
                                    .FirstOrDefaultAsync(s => s.MaSp == sp.MaSp);

                                if (sanPham == null || sanPham.KhoHangs.Sum(k => k.SoLuongTon) < sp.SoLuong)
                                {
                                    transaction.Rollback();
                                    return Json(new { success = false, message = $"Sản phẩm {sanPham?.TenSp} không đủ số lượng trong kho" });
                                }

                                // Trừ số lượng trong kho
                                await TruSoLuongKho(sanPham.MaSp, sp.SoLuong);

                                var chiTiet = new ChiTietDonHang
                                {
                                    MaDh = donHang.MaDh,
                                    MaSp = sp.MaSp,
                                    SoLuong = sp.SoLuong,
                                    GiaBan = sanPham.GiaBan,
                                    TongTienTungSanPham = sp.SoLuong * sanPham.GiaBan
                                };

                                chiTietDonHangs.Add(chiTiet);
                                tongTien += chiTiet.TongTienTungSanPham ?? 0;
                            }
                        }

                        // Xử lý giỏ quà được chọn
                        if (model.SelectedGioQuas != null)
                        {
                            foreach (var gq in model.SelectedGioQuas)
                            {
                                var gioQua = await _context.GioQuas
                                    .Include(g => g.ChiTietGioQuas)
                                    .ThenInclude(ct => ct.MaSpNavigation)
                                    .ThenInclude(sp => sp.KhoHangs)
                                    .FirstOrDefaultAsync(g => g.MaGq == gq.MaGq);

                                if (gioQua == null)
                                {
                                    transaction.Rollback();
                                    return Json(new { success = false, message = "Giỏ quà không tồn tại" });
                                }

                                // Kiểm tra số lượng tồn của giỏ quà
                                var soLuongTonGioQua = TinhSoLuongTonGioQua(gioQua.Slug);
                                if (soLuongTonGioQua < gq.SoLuong)
                                {
                                    transaction.Rollback();
                                    return Json(new { success = false, message = $"Giỏ quà {gioQua.TenGioQua} không đủ số lượng trong kho" });
                                }

                                // Trừ số lượng các sản phẩm trong giỏ quà
                                foreach (var ct in gioQua.ChiTietGioQuas)
                                {
                                    var soLuongCanTru = (decimal)ct.SoLuong * gq.SoLuong;
                                    await TruSoLuongKho(ct.MaSp, (decimal)soLuongCanTru);
                                }

                                var chiTiet = new ChiTietDonHang
                                {
                                    MaDh = donHang.MaDh,
                                    MaGq = gq.MaGq,
                                    SoLuong = gq.SoLuong,
                                    GiaBan = gioQua.GiaBan,
                                    TongTienTungSanPham = gq.SoLuong * gioQua.GiaBan
                                };

                                chiTietDonHangs.Add(chiTiet);
                                tongTien += chiTiet.TongTienTungSanPham ?? 0;
                            }
                        }

                        // Cập nhật tổng tiền đơn hàng
                        donHang.TongTienDonHang = tongTien;
                        donHang.UpdatedAt = DateTime.Now;

                        // Thêm chi tiết đơn hàng
                        await _context.ChiTietDonHangs.AddRangeAsync(chiTietDonHangs);
                        await _context.SaveChangesAsync();

                        await transaction.CommitAsync();

                        return Json(new { success = true, message = "Tạo đơn hàng offline thành công", maDh = donHang.MaDh });
                    }
                    catch (Exception ex)
                    {
                        await transaction.RollbackAsync();
                        return Json(new { success = false, message = $"Lỗi khi tạo đơn hàng: {ex.Message}" });
                    }
                }
            }

            return Json(new { success = false, message = "Dữ liệu không hợp lệ" });
        }

        // Hàm tính số lượng tồn của giỏ quà
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

        // Hàm trừ số lượng trong kho
        private async Task TruSoLuongKho(int maSp, decimal soLuong)
        {
            var khoHangs = await _context.KhoHangs
                .Where(k => k.MaSp == maSp && k.SoLuongTon > 0)
                .OrderBy(k => k.CreatedAt)
                .ToListAsync();

            decimal soLuongCanTru = soLuong;

            foreach (var kho in khoHangs)
            {
                if (soLuongCanTru <= 0) break;

                if (kho.SoLuongTon >= soLuongCanTru)
                {
                    kho.SoLuongTon -= (int)soLuongCanTru;
                    soLuongCanTru = 0;
                }
                else
                {
                    soLuongCanTru -= kho.SoLuongTon;
                    kho.SoLuongTon = 0;
                }

                kho.UpdatedAt = DateTime.Now;
            }

            await _context.SaveChangesAsync();
        }
    }

    // ViewModel cho đơn hàng offline
    public class DonHangOfflineViewModel
    {
        public int MaNv { get; set; }
        public string SoDienThoai { get; set; }
        public bool DaThanhToan { get; set; }
        public string GhiChu { get; set; }
        public List<SelectedSanPham> SelectedSanPhams { get; set; }
        public List<SelectedGioQua> SelectedGioQuas { get; set; }
    }

    public class SelectedSanPham
    {
        public int MaSp { get; set; }
        public decimal SoLuong { get; set; }
    }

    public class SelectedGioQua
    {
        public int MaGq { get; set; }
        public decimal SoLuong { get; set; }
    }

    public class GioQuaViewModel
    {
        public GioQua GioQua { get; set; }
        public int SoLuongTon { get; set; }
    }
}