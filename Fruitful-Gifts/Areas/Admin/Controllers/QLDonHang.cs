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
        public IActionResult ChiTietThanhToan(string status, string startDate, string endDate, decimal? minAmount, decimal? maxAmount)
        {
            var query = _context.ThanhToans
                .Include(t => t.MaDhNavigation)
                .Include(t => t.MaPtNavigation)
                .AsQueryable();

            // Áp dụng các bộ lọc
            if (!string.IsNullOrEmpty(status))
            {
                query = query.Where(t => t.PaymentStatus == status);
            }

            if (!string.IsNullOrEmpty(startDate) && DateTime.TryParse(startDate, out var start))
            {
                query = query.Where(t => t.PaymentTime >= start);
            }

            if (!string.IsNullOrEmpty(endDate) && DateTime.TryParse(endDate, out var end))
            {
                query = query.Where(t => t.PaymentTime <= end.AddDays(1)); // Thêm 1 ngày để bao gồm cả ngày cuối
            }

            if (minAmount.HasValue)
            {
                query = query.Where(t => t.Amount >= minAmount.Value);
            }

            if (maxAmount.HasValue)
            {
                query = query.Where(t => t.Amount <= maxAmount.Value);
            }

            var thanhToans = query
                .OrderBy(t => t.PaymentStatus == "PendingRefund" ? 0 : 1) // PendingRefund lên đầu
                .ThenByDescending(t => t.PaymentTime)
                .ToList();

            return View(thanhToans);
        }
        [HttpGet]
        public async Task<IActionResult> PaymentDetail(int id)
        {
            var thanhToan = await _context.ThanhToans
                .Include(t => t.MaDhNavigation)
                .Include(t => t.MaPtNavigation)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (thanhToan == null)
            {
                return NotFound();
            }

            return PartialView("_PaymentDetailPartial", thanhToan);
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

            // Bộ lọc theo tìm kiếm
            if (!string.IsNullOrEmpty(searchString))
            {
                donHangsQuery = donHangsQuery.Where(d =>
                    d.MaDh.ToString().Contains(searchString) ||
                    d.MaKhNavigation.TenKh.Contains(searchString) ||
                    d.MaKhNavigation.Sdt.Contains(searchString));
            }

            // Bộ lọc theo ngày
            if (DateTime.TryParseExact(fromDate, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var from))
            {
                donHangsQuery = donHangsQuery.Where(d => d.NgayDatHang >= from);
            }

            if (DateTime.TryParseExact(toDate, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var to))
            {
                donHangsQuery = donHangsQuery.Where(d => d.NgayDatHang <= to.AddDays(1));
            }

            // Bộ lọc theo trạng thái đơn hàng
            if (trangThai.HasValue)
            {
                donHangsQuery = donHangsQuery.Where(d => d.TrangThai == trangThai);
            }

            // Tính tổng số bản ghi thỏa điều kiện
            int totalItems = donHangsQuery.Count();

            // Tính tổng số trang
            int totalPages = (int)Math.Ceiling((double)totalItems / pageSize);

            // Giới hạn page trong khoảng hợp lệ
            if (page < 1)
            {
                page = 1;
            }
            else if (page > totalPages && totalPages > 0)
            {
                page = totalPages;
            }

            // Lấy dữ liệu phân trang
            var donHangs = donHangsQuery
                .OrderBy(d => d.TrangThai)
                .ThenByDescending(d => d.NgayDatHang)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            // Lấy ngày cũ nhất
            var oldestOrderDate = _context.DonHangs
                .OrderBy(d => d.NgayDatHang)
                .Select(d => d.NgayDatHang)
                .FirstOrDefault();

            // Lấy ngày mới nhất
            var newestOrderDate = _context.DonHangs
                .OrderByDescending(d => d.NgayDatHang)
                .Select(d => d.NgayDatHang)
                .FirstOrDefault();

            // Truyền dữ liệu cho View
            ViewBag.TongDonHang = donHangsQuery.Count();
            ViewBag.TongGiaTriDonHang = donHangsQuery.Sum(d => d.TongTienDonHang ?? 0);
            ViewBag.OldestOrderDate = oldestOrderDate;
            ViewBag.NewestOrderDate = newestOrderDate;
            ViewBag.TotalItems = totalItems;

            ViewData["CurrentPage"] = page;
            ViewData["TotalPages"] = totalPages;
            ViewData["TrangThai"] = trangThai;

            ViewBag.DonHangChoXacNhan = _context.DonHangs.Count(d => d.TrangThai == 1);
            ViewData["SearchString"] = searchString ?? "";
            ViewData["FromDate"] = fromDate ?? "";
            ViewData["ToDate"] = toDate ?? "";

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
            // Lấy ngày cũ nhất
            var oldestOrderDate = _context.DonHangs
                .OrderBy(d => d.NgayDatHang)
                .Select(d => d.NgayDatHang)
                .FirstOrDefault();

            // Lấy ngày mới nhất
            var newestOrderDate = _context.DonHangs
                .OrderByDescending(d => d.NgayDatHang)
                .Select(d => d.NgayDatHang)
                .FirstOrDefault();

            // Đưa vào ViewBag
            ViewBag.OldestOrderDate = oldestOrderDate;
            ViewBag.NewestOrderDate = newestOrderDate;

            ViewBag.Page = page;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalPages = (int)Math.Ceiling((double)totalItems / pageSize);

            return View(danhSachDonHangDaXoa);
        }

        [HttpPost]
        [Route("Admin/DonHang/CapNhatDonHang")]
        public IActionResult CapNhatDonHang(int maDh, int trangThai)
        {
            using var transaction = _context.Database.BeginTransaction();
            try
            {
                var donHang = _context.DonHangs
                    .Include(d => d.ChiTietDonHangs)
                    .ThenInclude(ct => ct.MaSpNavigation)
                    .ThenInclude(sp => sp.KhoHangs)
                    .Include(d => d.ChiTietDonHangs)
                    .ThenInclude(ct => ct.MaGqNavigation)
                    .FirstOrDefault(d => d.MaDh == maDh);

                if (donHang == null)
                {
                    return Json(new { success = false, message = "Đơn hàng không tồn tại." });
                }

                int currentStatus = donHang.TrangThai ?? 1;

                if (trangThai < 1 || trangThai > 8)
                {
                    return Json(new { success = false, message = "Trạng thái không hợp lệ." });
                }

                if (!IsValidStatusTransition(currentStatus, trangThai))
                {
                    var currentStatusName = GetStatusName(currentStatus);
                    var newStatusName = GetStatusName(trangThai);
                    return Json(new
                    {
                        success = false,
                        message = $"Không thể chuyển từ '{currentStatusName}' sang '{newStatusName}'"
                    });
                }

                // Xử lý hoàn lại số lượng khi hủy đơn
                if (currentStatus == 1 && (trangThai == 6 || trangThai == 8))
                {
                    foreach (var chiTiet in donHang.ChiTietDonHangs)
                    {
                        
                        // Đối với sản phẩm đơn lẻ
                        if (chiTiet.MaSp != null)
                        {
                            var khoHang = _context.KhoHangs.FirstOrDefault(k => k.MaSp == chiTiet.MaSp);
                            if (khoHang != null)
                            {
                                khoHang.SoLuongTon += (int)Math.Round(chiTiet.SoLuong ?? 0);
                            }
                        }
                       
                        // Đối với giỏ quà
                        else if (chiTiet.MaGq != null)
                        {
                            var chiTietGioQuaList = _context.ChiTietGioQuas
                                .Where(ct => ct.MaGq == chiTiet.MaGq)
                                .Include(ct => ct.MaSpNavigation)
                                .ThenInclude(sp => sp.KhoHangs)
                                .ToList();

                            foreach (var ctGioQua in chiTietGioQuaList)
                            {
                                var khoHang = ctGioQua.MaSpNavigation.KhoHangs.FirstOrDefault();
                                if (khoHang != null)
                                {
                                    // Chuyển đổi tất cả về double trước khi tính toán
                                    double soLuongGioQua = ctGioQua.SoLuong;
                                    double soLuongDonHang = chiTiet.SoLuong ?? 0.0;

                                    khoHang.SoLuongTon += (int)Math.Round(soLuongGioQua * soLuongDonHang);
                                }
                            }
                        }
                    }
                }

                donHang.TrangThai = trangThai;
                donHang.UpdatedAt = DateTime.Now;
                if (trangThai == 4 && donHang.MaPt == 3)
                {
                    donHang.TrangThaiThanhToan = 1; // Đã thanh toán
                }
                _context.SaveChanges();
                transaction.Commit();

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                return Json(new { success = false, message = "Lỗi hệ thống: " + ex.Message });
            }
        }
        private bool IsValidStatusTransition(int currentStatus, int newStatus)
        {
            var allowedTransitions = new Dictionary<int, List<int>>
            {
                {1, new List<int>{2, 6, 8}}, // Chờ xác nhận
                {2, new List<int>{3, 6}},     // Đã xác nhận
                {3, new List<int>{4, 7}},     // Đang giao
                {4, new List<int>{5}},        // Đã giao
                {5, new List<int>{}},         // Hoàn hàng
                {6, new List<int>{}},         // Đã hủy
                {7, new List<int>{3}},        // Giao thất bại
                {8, new List<int>{}}          // Từ chối
            };

            return allowedTransitions.ContainsKey(currentStatus) &&
                   allowedTransitions[currentStatus].Contains(newStatus);
        }

        private string GetStatusName(int status)
        {
            return status switch
            {
                1 => "Chờ xác nhận",
                2 => "Đã xác nhận",
                3 => "Đang giao hàng",
                4 => "Đã giao hàng",
                5 => "Hoàn hàng",
                6 => "Đã hủy",
                7 => "Giao hàng thất bại",
                8 => "Từ chối",
                _ => "Không xác định"
            };
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


        [HttpGet("api/items")]
        public IActionResult GetItems(int type)
        {
            if (type == 0) // Sản phẩm
            {
                var products = _context.SanPhams
                    .Where(p => p.TrangThai == 1) // Chỉ lấy sp active
                    .Select(p => new {
                        id = p.MaSp,
                        name = p.TenSp,
                        price = p.GiaBan
                    }).ToList();

                return Ok(products);
            }
            else // Giỏ quà
            {
                var giftBaskets = _context.GioQuas
                    .Where(g => g.TrangThai == 1) // Chỉ lấy giỏ quà active
                    .Select(g => new {
                        id = g.MaGq,
                        name = g.TenGioQua,
                        price = g.GiaBan
                    }).ToList();

                return Ok(giftBaskets);
            }
        }

    

    }

}

    
