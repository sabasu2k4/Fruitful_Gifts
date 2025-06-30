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
    public class QLDonHangController : Controller
    {
        private readonly FruitfulGiftsContext _context;

        public QLDonHangController(FruitfulGiftsContext context)
        {
            _context = context;
        }

        public IActionResult Index(string searchString, string fromDate, string toDate, int? trangThai, int page = 1)
        {
            int pageSize = 8;

            var donHangsQuery = _context.DonHangs
                .Include(d => d.MaKhNavigation)
                .Include(d => d.ChiTietDonHangs).ThenInclude(ct => ct.MaSpNavigation)
                .Include(d => d.ChiTietDonHangs).ThenInclude(ct => ct.MaGqNavigation)
                .Include(d => d.TrangThaiNavigation)
                .Include(d => d.MaPtNavigation)
                //.Where(d => d.TrangThai != 6) 
                .AsQueryable();
            // ✅ Bộ lọc theo tìm kiếm
            if (!string.IsNullOrEmpty(searchString))
            {
                donHangsQuery = donHangsQuery.Where(d =>
                    d.MaDh.ToString().Contains(searchString) ||
                    d.MaKhNavigation.TenKh.Contains(searchString) ||
                    d.MaKhNavigation.Sdt.Contains(searchString));
            }
            ViewBag.TongDonHang = donHangsQuery.Count();
            ViewBag.TongGiaTriDonHang = donHangsQuery.Sum(d => d.TongTienDonHang ?? 0);

            // ✅ Bộ lọc theo ngày
            if (DateTime.TryParseExact(fromDate, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var from))
            {
                donHangsQuery = donHangsQuery.Where(d => d.NgayDatHang >= from);
            }

            if (DateTime.TryParseExact(toDate, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var to))
            {
                // cộng thêm 1 ngày để bao gồm ngày đó
                donHangsQuery = donHangsQuery.Where(d => d.NgayDatHang <= to.AddDays(1));
            }
            if (trangThai.HasValue)
            {
                donHangsQuery = donHangsQuery.Where(d => d.TrangThai == trangThai);
            }

            int totalItems = donHangsQuery.Count();

            var donHangs = donHangsQuery
                .OrderBy(d => d.TrangThai)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            ViewBag.TotalItems = totalItems;
            ViewData["CurrentPage"] = page;
            ViewData["TotalPages"] = (int)Math.Ceiling((double)totalItems / pageSize);
            ViewData["TrangThai"] = trangThai;

            return View(donHangs);
        }


        public IActionResult DonHangDaXoa(string searchString, string fromDate, string toDate, int page = 1, int pageSize = 10)
        {
            var query = _context.DonHangs
                .Where(d => d.TrangThai == 6) // chỉ đơn đã xóa
                .Include(d => d.MaKhNavigation)
                .Include(d => d.ChiTietDonHangs).ThenInclude(ct => ct.MaSpNavigation)
                .Include(d => d.ChiTietDonHangs).ThenInclude(ct => ct.MaGqNavigation)
                .Include(d => d.TrangThaiNavigation)
                .AsQueryable();

            // ✅ Bộ lọc theo tìm kiếm
            if (!string.IsNullOrEmpty(searchString))
            {
                query = query.Where(d =>
                    d.MaDh.ToString().Contains(searchString) ||
                    d.MaKhNavigation.TenKh.Contains(searchString) ||
                    d.MaKhNavigation.Sdt.Contains(searchString));
            }

            // ✅ Bộ lọc theo ngày
            if (DateTime.TryParseExact(fromDate, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var from))
            {
                query = query.Where(d => d.NgayDatHang >= from);
            }

            if (DateTime.TryParseExact(toDate, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var to))
            {
                // cộng thêm 1 ngày để bao gồm ngày đó
                query = query.Where(d => d.NgayDatHang <= to.AddDays(1));
            }

            // ✅ Tổng số đơn & tổng giá trị (toàn bộ hệ thống sau lọc)
            ViewBag.TongDonHangDaXoa = query.Count();
            ViewBag.TongGiaTriDonHangDaXoa = query.Sum(d => d.TongTienDonHang ?? 0);

            // ✅ Phân trang
            int totalItems = query.Count();

            var danhSachDonHangDaXoa = query
                .OrderByDescending(d => d.NgayDatHang)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            ViewBag.Page = page;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalPages = (int)Math.Ceiling((double)totalItems / pageSize);

            return View(danhSachDonHangDaXoa);
        }

        [HttpPost]
        [Route("Admin/DonHang/CapNhatDonHang")]
        public IActionResult CapNhatDonHang(int maDh, int trangThai)
        {
            try
            {
                var donHang = _context.DonHangs.FirstOrDefault(d => d.MaDh == maDh);
                if (donHang == null)
                {
                    return Json(new { success = false, message = "Đơn hàng không tồn tại." });
                }

                donHang.TrangThai = trangThai;
                donHang.UpdatedAt = DateTime.Now;

                _context.SaveChanges();

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        [Route("Admin/QLDonHang/RestoreDonHang")]
        public JsonResult RestoreDonHang(int maDH)
        {
            try
            {
                var donHang = _context.DonHangs.FirstOrDefault(d => d.MaDh == maDH);

                if (donHang == null)
                {
                    return Json(new { success = false, message = "Đơn hàng không tồn tại." });
                }

                if (donHang.TrangThai != 6) // Chỉ khôi phục đơn hàng đã bị xóa (trạng thái 6)
                {
                    return Json(new { success = false, message = "Chỉ có thể khôi phục đơn hàng đã xóa." });
                }

                // Cập nhật trạng thái đơn hàng về trạng thái "Chờ xác nhận" (hoặc trạng thái hợp lý theo nghiệp vụ)
                donHang.TrangThai = 1;
                donHang.UpdatedAt = DateTime.Now;

                _context.SaveChanges();

                return Json(new { success = true, message = "Khôi phục đơn hàng thành công." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Đã xảy ra lỗi: " + ex.Message });
            }
        }



        public async Task<IActionResult> ExportToExcel(string searchString, string fromDate, string toDate, int? trangThai)
        {
            var donHangsQuery = _context.DonHangs
                .Include(d => d.MaKhNavigation)
                .Include(d => d.ChiTietDonHangs).ThenInclude(ct => ct.MaSpNavigation)
                .Include(d => d.ChiTietDonHangs).ThenInclude(ct => ct.MaGqNavigation)
                .Include(d => d.TrangThaiNavigation)
                .Include(d => d.MaPtNavigation)
                .AsQueryable();

            // Áp dụng filter tương tự như trong Index
            if (!string.IsNullOrEmpty(searchString))
            {
                donHangsQuery = donHangsQuery.Where(d =>
                    d.MaDh.ToString().Contains(searchString) ||
                    d.MaKhNavigation.TenKh.Contains(searchString) ||
                    d.MaKhNavigation.Sdt.Contains(searchString));
            }

            if (DateTime.TryParseExact(fromDate, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var from))
            {
                donHangsQuery = donHangsQuery.Where(d => d.NgayDatHang >= from);
            }

            if (DateTime.TryParseExact(toDate, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var to))
            {
                donHangsQuery = donHangsQuery.Where(d => d.NgayDatHang <= to.AddDays(1));
            }

            if (trangThai.HasValue)
            {
                donHangsQuery = donHangsQuery.Where(d => d.TrangThai == trangThai);
            }

            var donHangs = await donHangsQuery
                .OrderBy(d => d.TrangThai)
                .ToListAsync();

            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("DanhSachDonHang");

                // Tiêu đề cột
                worksheet.Cell(1, 1).Value = "Mã Đơn Hàng";
                worksheet.Cell(1, 2).Value = "Khách Hàng";
                worksheet.Cell(1, 3).Value = "SĐT";
                worksheet.Cell(1, 4).Value = "Ngày Đặt";
                worksheet.Cell(1, 5).Value = "Tổng Tiền";
                worksheet.Cell(1, 6).Value = "Phương Thức Thanh Toán";
                worksheet.Cell(1, 7).Value = "Trạng Thái";

                // Định dạng header
                var headerRange = worksheet.Range("A1:G1");
                headerRange.Style.Font.Bold = true;
                headerRange.Style.Fill.BackgroundColor = XLColor.LightGray;
                headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                headerRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                headerRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

                int row = 2;
                foreach (var dh in donHangs)
                {
                    worksheet.Cell(row, 1).Value = dh.MaDh;
                    worksheet.Cell(row, 2).Value = dh.MaKhNavigation?.HoKh + " " + dh.MaKhNavigation?.TenKh;
                    worksheet.Cell(row, 3).Value = dh.MaKhNavigation?.Sdt;
                    worksheet.Cell(row, 4).Value = dh.NgayDatHang?.ToString("dd/MM/yyyy");
                    worksheet.Cell(row, 5).Value = dh.TongTienDonHang ?? 0;
                    worksheet.Cell(row, 6).Value = dh.MaPtNavigation?.TenPt ?? "";
                    worksheet.Cell(row, 7).Value = dh.TrangThaiNavigation?.TenTrangThai ?? "";

                    var dataRowRange = worksheet.Range(row, 1, row, 7);
                    dataRowRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    dataRowRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                    dataRowRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;

                    row++;
                }

                worksheet.Columns().AdjustToContents();
                worksheet.SheetView.FreezeRows(1);

                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    var content = stream.ToArray();
                    return File(content,
                        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                        "DanhSachDonHang.xlsx");
                }
            }
        }


        // Action để hiển thị trang tạo đơn hàng offline
        public IActionResult TaoDonHangOffline()
        {
            var model = new DonHangOfflineViewModel
            {
                // Thêm hàm tính số lượng tồn
                TinhSoLuongTonGioQua = TinhSoLuongTonGioQua
            };

            // Lấy danh sách sản phẩm và giỏ quà có sẵn
            var sanPhams = _context.SanPhams
             .Include(sp => sp.KhoHangs)
             .Where(sp => sp.TrangThai == 1)
             .Select(sp => new {
                 sp.MaSp,
                 sp.TenSp,
                 sp.GiaBan,
                 TongTonKho = sp.KhoHangs.Sum(k => k.SoLuongTon)
             })
             .ToList();

          
            var gioQuas = _context.GioQuas
                .Include(gq => gq.ChiTietGioQuas)
                .Where(gq => gq.TrangThai == 1) // Chỉ lấy giỏ quà đang hoạt động
                .ToList();

            ViewBag.SanPhams = sanPhams;
            ViewBag.GioQuas = gioQuas;

            return View();
        }

        // Action để xử lý tạo đơn hàng offline
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
                            TrangThaiThanhToan = 1, // Đã thanh toán
                            PhuongThucBan = "Offline",
                            DiaChiNhanHang = "Mua tại quầy",
                            CreatedAt = DateTime.Now,
                            UpdatedAt = DateTime.Now,
                            TongTienDonHang = 0 // Khởi tạo ban đầu là 0
                        };

                        _context.DonHangs.Add(donHang);
                        await _context.SaveChangesAsync();

                        decimal tongTien = 0;

                        // Xử lý các sản phẩm được chọn
                        if (model.SelectedSanPhams != null && model.SelectedSanPhams.Any())
                        {
                            foreach (var item in model.SelectedSanPhams)
                            {
                                var sanPham = await _context.SanPhams
                                    .Include(sp => sp.KhoHangs)
                                    .FirstOrDefaultAsync(sp => sp.MaSp == item.MaSp);

                                if (sanPham == null || sanPham.KhoHangs.Sum(k => k.SoLuongTon) < item.SoLuong)
                                {
                                    transaction.Rollback();
                                    return Json(new { success = false, message = $"Sản phẩm {sanPham?.TenSp} không đủ số lượng trong kho" });
                                }

                                // Trừ số lượng trong kho
                                await TruSoLuongTonKho(sanPham.MaSp, item.SoLuong);

                                // Thêm chi tiết đơn hàng
                                var chiTiet = new ChiTietDonHang
                                {
                                    MaDh = donHang.MaDh,
                                    MaSp = item.MaSp,
                                    SoLuong = item.SoLuong,
                                    GiaBan = sanPham.GiaBan,
                                    TongTienTungSanPham = sanPham.GiaBan * item.SoLuong
                                };

                                _context.ChiTietDonHangs.Add(chiTiet);
                                tongTien += chiTiet.TongTienTungSanPham ?? 0;
                            }
                        }

                        // Xử lý các giỏ quà được chọn
                        if (model.SelectedGioQuas != null && model.SelectedGioQuas.Any())
                        {
                            foreach (var item in model.SelectedGioQuas)
                            {
                                var gioQua = await _context.GioQuas
                                    .Include(gq => gq.ChiTietGioQuas)
                                    .ThenInclude(ct => ct.MaSpNavigation)
                                    .ThenInclude(sp => sp.KhoHangs)
                                    .FirstOrDefaultAsync(gq => gq.MaGq == item.MaGq);

                                if (gioQua == null)
                                {
                                    transaction.Rollback();
                                    return Json(new { success = false, message = "Giỏ quà không tồn tại" });
                                }

                                // Kiểm tra số lượng tồn kho cho từng sản phẩm trong giỏ quà
                                foreach (var chiTietGQ in gioQua.ChiTietGioQuas)
                                {
                                    var soLuongCan = (decimal)chiTietGQ.SoLuong * item.SoLuong;
                                    var tonKho = chiTietGQ.MaSpNavigation.KhoHangs.Sum(k => k.SoLuongTon);

                                    if (tonKho < soLuongCan)
                                    {
                                        transaction.Rollback();
                                        return Json(new { success = false, message = $"Không đủ số lượng sản phẩm {chiTietGQ.MaSpNavigation.TenSp} trong kho để tạo giỏ quà {gioQua.TenGioQua}" });
                                    }
                                }

                                // Trừ số lượng từng sản phẩm trong giỏ quà
                                foreach (var chiTietGQ in gioQua.ChiTietGioQuas)
                                {
                                    await TruSoLuongTonKho(chiTietGQ.MaSp, (decimal)chiTietGQ.SoLuong * item.SoLuong);
                                }

                                // Thêm chi tiết đơn hàng
                                var chiTiet = new ChiTietDonHang
                                {
                                    MaDh = donHang.MaDh,
                                    MaGq = item.MaGq,
                                    SoLuong = item.SoLuong,
                                    GiaBan = gioQua.GiaBan,
                                    TongTienTungSanPham = gioQua.GiaBan * item.SoLuong
                                };

                                _context.ChiTietDonHangs.Add(chiTiet);
                                tongTien += chiTiet.TongTienTungSanPham ?? 0;
                            }
                        }

                        // Cập nhật tổng tiền đơn hàng
                        donHang.TongTienDonHang = tongTien;
                        donHang.UpdatedAt = DateTime.Now;

                        _context.DonHangs.Update(donHang);
                        await _context.SaveChangesAsync();

                        await transaction.CommitAsync();

                        return Json(new { success = true, message = "Tạo đơn hàng offline thành công", maDh = donHang.MaDh });
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        return Json(new { success = false, message = $"Lỗi khi tạo đơn hàng: {ex.Message}" });
                    }
                }
            }

            return Json(new { success = false, message = "Dữ liệu không hợp lệ" });
        }

        // Hàm trừ số lượng tồn kho
        private async Task TruSoLuongTonKho(int maSp, decimal soLuong)
        {
            var khoHangs = await _context.KhoHangs
                .Where(k => k.MaSp == maSp && k.SoLuongTon > 0)
                .OrderBy(k => k.CreatedAt)
                .ToListAsync();

            decimal soLuongConLai = soLuong;

            foreach (var kho in khoHangs)
            {
                if (soLuongConLai <= 0) break;

                if (kho.SoLuongTon >= (int)soLuongConLai) // Ép kiểu ở đây
                {
                    kho.SoLuongTon -= (int)soLuongConLai; // Ép kiểu ở đây
                    soLuongConLai = 0;
                }
                else
                {
                    soLuongConLai -= kho.SoLuongTon;
                    kho.SoLuongTon = 0;
                }

                kho.UpdatedAt = DateTime.Now;
                _context.KhoHangs.Update(kho);
            }

            await _context.SaveChangesAsync();
        }
        // Hàm tính số lượng tồn kho cho giỏ quà (sử dụng logic bạn đã cung cấp)
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
    }

    // ViewModel cho đơn hàng offline
}

    
