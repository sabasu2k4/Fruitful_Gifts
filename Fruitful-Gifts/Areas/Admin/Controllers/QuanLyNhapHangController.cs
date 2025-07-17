using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using Fruitful_Gifts.Database;
using Fruitful_Gifts.Models;
using System.Globalization;

namespace Fruitful_Gifts.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class QuanLyNhapHangController : Controller
    {
        private readonly FruitfulGiftsContext _context;

        public QuanLyNhapHangController(FruitfulGiftsContext context)
        {
            _context = context;
        }

        public IActionResult NhapHang(string searchString, string fromDate, string toDate, int? trangThai, int page = 1)
        {

            int pageSize = 8;
            var nhapHangsQuery = _context.NhapHangs
                .Include(n => n.MaNccNavigation)
                .Include(n => n.ChiTietNhapHangs)
                .ThenInclude(ct => ct.MaSpNavigation)
                .AsQueryable();
            // Bộ lọc
            if (!string.IsNullOrEmpty(searchString))
            {
                nhapHangsQuery = nhapHangsQuery.Where(n =>
                    n.MaNhap.ToString().Contains(searchString) ||
                    n.MaNccNavigation.TenNcc.Contains(searchString) ||
                    n.MaNccNavigation.Sdt.Contains(searchString));
            }
            if (DateTime.TryParseExact(fromDate, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var from))
            {
                nhapHangsQuery = nhapHangsQuery.Where(n => n.NgayNhap >= from);
            }
            if (DateTime.TryParseExact(toDate, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var to))
            {
                nhapHangsQuery = nhapHangsQuery.Where(n => n.NgayNhap <= to.AddDays(1));
            }
            if (trangThai.HasValue)
            {
                nhapHangsQuery = nhapHangsQuery.Where(n => n.TrangThai == trangThai);
            }
            int totalItems = nhapHangsQuery.Count();
            int totalPages = (int)Math.Ceiling((double)totalItems / pageSize);
            if (page < 1) page = 1;
            else if (page > totalPages && totalPages > 0) page = totalPages;
            var nhapHangs = nhapHangsQuery
                .OrderByDescending(n => n.NgayNhap)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();
            // Thống kê
            ViewBag.TongPhieuNhap = nhapHangsQuery.Count();
            ViewBag.TongGiaTriNhap = nhapHangsQuery
                .Where(n => n.TrangThai == 1)
                .Sum(n => n.TongTien ?? 0);
            ViewBag.OldestDate = _context.NhapHangs.Min(n => n.NgayNhap);
            ViewBag.NewestDate = _context.NhapHangs.Max(n => n.NgayNhap);
            ViewData["CurrentPage"] = page;
            ViewData["TotalPages"] = totalPages;
            ViewData["TrangThai"] = trangThai;
            return View(nhapHangs);
        }

        // GET: Tạo phiếu nhập mới
        public async Task<IActionResult> TaoPhieuNhap()
        {
            ViewBag.NhaCungCap = await _context.NhaCungCaps.ToListAsync();
            ViewBag.SanPham = await _context.SanPhams.ToListAsync();
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> TaoPhieuNhap(NhapHang nhapHang, List<int> MaSp, List<double> SoLuong, List<decimal> GiaNhap)
        {
            if (!ModelState.IsValid || MaSp.Count == 0)
            {
                ViewBag.NhaCungCap = _context.NhaCungCaps.ToList();
                ViewBag.SanPham = _context.SanPhams.ToList();
                return View(nhapHang);
            }

            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    nhapHang.NgayNhap = DateTime.Now;
                    nhapHang.CreatedAt = DateTime.Now;
                    nhapHang.UpdatedAt = DateTime.Now;

                    decimal tongTienHang = 0;
                    for (int i = 0; i < MaSp.Count; i++)
                    {
                        tongTienHang += (decimal)SoLuong[i] * GiaNhap[i];
                    }

                    // 2. Cộng thêm phí vận chuyển (nếu có)
                    decimal phiVanChuyen = nhapHang.PhiVanChuyenNhapHang ?? 0;

                    // 3. Tính tổng tiền cuối cùng
                    nhapHang.TongTien = tongTienHang + phiVanChuyen;

                    _context.NhapHangs.Add(nhapHang);
                    await _context.SaveChangesAsync();

                    for (int i = 0; i < MaSp.Count; i++)
                    {
                        var ct = new ChiTietNhapHang
                        {
                            MaNhap = nhapHang.MaNhap,
                            MaSp = MaSp[i],
                            SoLuong = SoLuong[i],
                            GiaNhap = GiaNhap[i]
                        };
                        _context.ChiTietNhapHangs.Add(ct);

                        // Cập nhật kho
                        var kho = await _context.KhoHangs.FirstOrDefaultAsync(k => k.MaSp == MaSp[i]);
                        if (kho != null)
                        {
                            kho.SoLuongTon += SoLuong[i];
                            kho.UpdatedAt = DateTime.Now;
                        }
                        else
                        {
                            kho = new KhoHang
                            {
                                MaSp = MaSp[i],
                                SoLuongTon = SoLuong[i],
                                DonViTinh = "Cái",
                                CreatedAt = DateTime.Now,
                                UpdatedAt = DateTime.Now
                            };
                            _context.KhoHangs.Add(kho);
                        }
                    }

                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    return RedirectToAction(nameof(NhapHang));
                }
                catch (Exception)
                {
                    await transaction.RollbackAsync();
                    ModelState.AddModelError("", "Có lỗi xảy ra khi lưu phiếu nhập.");
                }
            }

            ViewBag.NhaCungCap = _context.NhaCungCaps.ToList();
            ViewBag.SanPham = _context.SanPhams.ToList();
            return View(nhapHang);
        }

        // Xem chi tiết phiếu nhập
        public async Task<IActionResult> Detail(int id)
        {
            var nhapHang = await _context.NhapHangs
                .Include(n => n.MaNccNavigation)
                .Include(n => n.ChiTietNhapHangs)
                .ThenInclude(ct => ct.MaSpNavigation)
                .FirstOrDefaultAsync(n => n.MaNhap == id);

            if (nhapHang == null)
            {
                return NotFound();
            }

            return View(nhapHang);
        }

        // In phiếu nhập
        public IActionResult PrintPartial(int id)
        {
            var nhapHang = _context.NhapHangs
                .Include(n => n.MaNccNavigation)
                .Include(n => n.ChiTietNhapHangs)
                .ThenInclude(ct => ct.MaSpNavigation)
                .FirstOrDefault(n => n.MaNhap == id);
            if (nhapHang == null)
            {
                return NotFound();
            }
            return PartialView("_PrintPartial", nhapHang);
        }

        [HttpPost]
        public async Task<IActionResult> HuyPhieuNhap(int id)
        {
            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    var phieuNhap = await _context.NhapHangs
                        .Include(n => n.ChiTietNhapHangs)
                        .FirstOrDefaultAsync(n => n.MaNhap == id);

                    if (phieuNhap == null)
                    {
                        return Json(new { success = false, message = "Không tìm thấy phiếu nhập" });
                    }

                    if (phieuNhap.TrangThai == 0)
                    {
                        return Json(new { success = false, message = "Phiếu nhập đã bị hủy trước đó" });
                    }

                    // Cập nhật trạng thái phiếu nhập
                    phieuNhap.TrangThai = 0;
                    phieuNhap.UpdatedAt = DateTime.Now;

                    // Hoàn trả số lượng về kho
                    foreach (var chiTiet in phieuNhap.ChiTietNhapHangs)
                    {
                        var kho = await _context.KhoHangs.FirstOrDefaultAsync(k => k.MaSp == chiTiet.MaSp);
                        if (kho != null)
                        {
                            kho.SoLuongTon -= chiTiet.SoLuong ?? 0.0;
                            kho.UpdatedAt = DateTime.Now;
                        }
                    }

                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    return Json(new { success = true, message = "Hủy phiếu nhập thành công" });
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    return Json(new { success = false, message = "Có lỗi xảy ra: " + ex.Message });
                }
            }
        }

    }
}