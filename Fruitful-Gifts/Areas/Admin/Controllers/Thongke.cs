using ClosedXML.Excel;
using DocumentFormat.OpenXml.InkML;
using Fruitful_Gifts.Database;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

using System.IO;
namespace Fruitful_Gifts.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class Thongke : Controller
    {
        private readonly FruitfulGiftsContext _context;

        public Thongke(FruitfulGiftsContext context)
        {
            _context = context;
        }
        public IActionResult ThongKeDoanhThu(string fromDate, string toDate, int? phuongThuc, int? page)
        {
            var donHangs = _context.DonHangs
                .Include(d => d.MaPtNavigation)
                .Where(d => d.TrangThai == 4);

            if (DateTime.TryParseExact(fromDate, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var from))
                donHangs = donHangs.Where(d => d.NgayDatHang >= from);

            if (DateTime.TryParseExact(toDate, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var to))
                donHangs = donHangs.Where(d => d.NgayDatHang <= to.AddDays(1));

            if (phuongThuc.HasValue)
                donHangs = donHangs.Where(d => d.MaPt == phuongThuc.Value);

            ViewBag.TongDoanhThu = donHangs.Sum(d => d.TongTienDonHang ?? 0);
            ViewBag.TongDon = donHangs.Count();

            var doanhThuTheoNgayQuery = donHangs
                .GroupBy(d => d.NgayDatHang.Value.Date)
                .Select(g => new
                {
                    Ngay = g.Key,
                    DoanhThu = g.Sum(d => d.TongTienDonHang ?? 0),
                    SoDon = g.Count()
                })
                .OrderBy(g => g.Ngay);

            int pageSize = 10;
            int pageNumber = page ?? 1;
            int totalItems = doanhThuTheoNgayQuery.Count();
            int totalPages = (int)Math.Ceiling((double)totalItems / pageSize);

            // Lấy dữ liệu cho trang hiện tại
            var doanhThuTheoNgayPaged = doanhThuTheoNgayQuery
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            ViewBag.DoanhThuTheoNgay = doanhThuTheoNgayPaged;
            ViewBag.PageNumber = pageNumber;
            ViewBag.TotalPages = totalPages;

            // Thống kê theo tháng giữ nguyên
            ViewBag.DoanhThuTheoThang = donHangs
                .Where(d => d.NgayDatHang.HasValue)
                .GroupBy(d => new
                {
                    Nam = d.NgayDatHang.Value.Year,
                    Thang = d.NgayDatHang.Value.Month
                })
                .Select(g => new
                {
                    Nam = g.Key.Nam,
                    Thang = g.Key.Thang,
                    ThangNam = $"Tháng {g.Key.Thang}/{g.Key.Nam}",
                    DoanhThu = g.Sum(d => d.TongTienDonHang ?? 0),
                    SoDon = g.Count()
                })
                .OrderBy(g => g.Nam)
                .ThenBy(g => g.Thang)
                .ToList();

            ViewBag.FromDate = fromDate;
            ViewBag.ToDate = toDate;
            ViewBag.PhuongThuc = phuongThuc;
            ViewBag.DanhSachPT = _context.PhuongThucThanhToans.ToList();

            return View();
        }


        [HttpGet]
        public IActionResult SanPhamVaGioQuaBanChayKem(string fromDate, string toDate, int? phuongThuc, int top = 5,
        double? minBanChaySanPham = null, double? maxBanKemSanPham = null,
        double? minBanChayGioQua = null, double? maxBanKemGioQua = null)
        {
            // 1. Lấy đơn hàng hoàn thành
            var donHangs = _context.DonHangs
                .Include(d => d.ChiTietDonHangs)
                    .ThenInclude(ct => ct.MaSpNavigation)
                .Include(d => d.ChiTietDonHangs)
                    .ThenInclude(ct => ct.MaGqNavigation)
                .Where(d => d.TrangThai == 4);

            // 2. Lọc theo ngày
            if (DateTime.TryParseExact(fromDate, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var from))
                donHangs = donHangs.Where(d => d.NgayDatHang >= from);

            if (DateTime.TryParseExact(toDate, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var to))
                donHangs = donHangs.Where(d => d.NgayDatHang <= to.AddDays(1));

            // 3. Lọc phương thức thanh toán nếu có
            if (phuongThuc.HasValue)
                donHangs = donHangs.Where(d => d.MaPt == phuongThuc.Value);

            // 4. Lấy chi tiết đơn hàng
            var chiTietDonHangs = donHangs.SelectMany(d => d.ChiTietDonHangs).ToList();

            //------------------------------
            // === SẢN PHẨM ===
            //------------------------------

            var allSanPham = chiTietDonHangs
             .Where(ct => ct.MaSp.HasValue)
             .GroupBy(ct => ct.MaSp)
             .Select(g => new
             {
                 MaSp = g.Key,
                 TenSp = g.First().MaSpNavigation?.TenSp ?? "(Không rõ tên)",
                 TongSoLuong = g.Sum(ct => ct.SoLuong ?? 0)
             })
             .ToList();

            // Giá trị mặc định nếu không có giá trị truyền vào
            minBanChaySanPham ??= 10.0;
            maxBanKemSanPham ??= 5.0;

            var sanPhamBanChay = allSanPham
                .Where(x => x.TongSoLuong >= minBanChaySanPham)
                .OrderByDescending(x => x.TongSoLuong)
                .Take(top)
                .ToList();

            var sanPhamBanKem = allSanPham
                .Where(x => x.TongSoLuong <= maxBanKemSanPham)
                .OrderBy(x => x.TongSoLuong)
                .Take(top)
                .ToList();

            //------------------------------
            // === GIỎ QUÀ ===
            //------------------------------

            var allGioQua = chiTietDonHangs
                .Where(ct => ct.MaGq.HasValue)
                .GroupBy(ct => ct.MaGq)
                .Select(g => new
                {
                    MaGq = g.Key,
                    TenGq = g.First().MaGqNavigation?.TenGioQua ?? "(Không rõ tên)",
                    TongSoLuong = g.Sum(ct => ct.SoLuong ?? 0)
                })
                .ToList();

            // Giá trị mặc định nếu không có giá trị truyền vào
            minBanChayGioQua ??= 5.0;
            maxBanKemGioQua ??= 3.0;

            var gioQuaBanChay = allGioQua
                .Where(x => x.TongSoLuong >= minBanChayGioQua)
                .OrderByDescending(x => x.TongSoLuong)
                .Take(top)
                .ToList();

            var gioQuaBanKem = allGioQua
                .Where(x => x.TongSoLuong <= maxBanKemGioQua)
                .OrderBy(x => x.TongSoLuong)
                .Take(top)
                .ToList();

            //------------------------------
            // === TRẢ RA VIEW ===
            //------------------------------

            ViewBag.SanPhamBanChay = sanPhamBanChay;
            ViewBag.SanPhamBanKem = sanPhamBanKem;
            ViewBag.GioQuaBanChay = gioQuaBanChay;
            ViewBag.GioQuaBanKem = gioQuaBanKem;

            ViewBag.FromDate = fromDate;
            ViewBag.ToDate = toDate;
            ViewBag.PhuongThuc = phuongThuc;
            ViewBag.DanhSachPT = _context.PhuongThucThanhToans.ToList();

            // Truyền các giá trị ngưỡng vào ViewBag
            ViewBag.MinBanChaySanPham = minBanChaySanPham ?? 10.0;
            ViewBag.MaxBanKemSanPham = maxBanKemSanPham ?? 5.0;   
            ViewBag.MinBanChayGioQua = minBanChayGioQua ?? 5.0;   
            ViewBag.MaxBanKemGioQua = maxBanKemGioQua ?? 3.0;
            return View();
        }
        public IActionResult XuatExcel(string fromDate, string toDate, int? phuongThuc)
        {
            var donHangs = _context.DonHangs
                .Include(d => d.MaPtNavigation)
                .Include(d => d.MaKhNavigation)
                .Where(d => d.TrangThai == 4);

            if (DateTime.TryParseExact(fromDate, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var from))
                donHangs = donHangs.Where(d => d.NgayDatHang >= from);

            if (DateTime.TryParseExact(toDate, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var to))
                donHangs = donHangs.Where(d => d.NgayDatHang <= to.AddDays(1));

            if (phuongThuc.HasValue)
                donHangs = donHangs.Where(d => d.MaPt == phuongThuc.Value);

            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("DoanhThu");

            // Header
            worksheet.Cell(1, 1).Value = "Ngày đặt";
            worksheet.Cell(1, 2).Value = "Khách hàng";
            worksheet.Cell(1, 3).Value = "Phương thức";
            worksheet.Cell(1, 4).Value = "Tổng tiền";

            // Định dạng header
            var headerRange = worksheet.Range(1, 1, 1, 4);
            headerRange.Style.Font.Bold = true;
            headerRange.Style.Fill.BackgroundColor = XLColor.LightGray;
            headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            headerRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            headerRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

            int row = 2;
            foreach (var d in donHangs)
            {
                worksheet.Cell(row, 1).Value = d.NgayDatHang?.ToString("dd/MM/yyyy");
                worksheet.Cell(row, 2).Value = d.MaKhNavigation?.TenKh;
                worksheet.Cell(row, 3).Value = d.MaPtNavigation?.TenPt;
                worksheet.Cell(row, 4).Value = d.TongTienDonHang ?? 0;

                // Căn trái tên khách hàng và phương thức thanh toán
                worksheet.Cell(row, 2).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                worksheet.Cell(row, 3).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;

                // Căn phải tổng tiền và định dạng số
                worksheet.Cell(row, 4).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                worksheet.Cell(row, 4).Style.NumberFormat.Format = "#,##0";

                // Có thể thêm border cho từng dòng dữ liệu nếu muốn
                var dataRowRange = worksheet.Range(row, 1, row, 4);
                dataRowRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                dataRowRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

                row++;
            }

            // Tự động điều chỉnh độ rộng cột
            worksheet.Columns().AdjustToContents();

            var stream = new MemoryStream();
            workbook.SaveAs(stream);
            stream.Position = 0;

            return File(stream,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                "ThongKeDoanhThu.xlsx");
        }




        [HttpGet]
        public IActionResult XuatExcelThongKe(string fromDate, string toDate, int? phuongThuc, int top = 5)
        {
            // --- Lấy dữ liệu ---
            var donHangs = _context.DonHangs
                .Include(d => d.ChiTietDonHangs)
                    .ThenInclude(ct => ct.MaSpNavigation)
                .Include(d => d.ChiTietDonHangs)
                    .ThenInclude(ct => ct.MaGqNavigation)
                .Where(d => d.TrangThai == 4);

            if (DateTime.TryParseExact(fromDate, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var from))
                donHangs = donHangs.Where(d => d.NgayDatHang >= from);

            if (DateTime.TryParseExact(toDate, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var to))
                donHangs = donHangs.Where(d => d.NgayDatHang <= to.AddDays(1));

            if (phuongThuc.HasValue)
                donHangs = donHangs.Where(d => d.MaPt == phuongThuc.Value);

            var chiTietDonHangs = donHangs.SelectMany(d => d.ChiTietDonHangs).ToList();

            var allSanPham = chiTietDonHangs
                .Where(ct => ct.MaSp.HasValue)
                .GroupBy(ct => ct.MaSp)
                .Select(g => new
                {
                    MaSp = g.Key,
                    TenSp = g.First().MaSpNavigation?.TenSp ?? "(Không rõ tên)",
                    TongSoLuong = g.Sum(ct => ct.SoLuong ?? 0)
                })
                .ToList();

            var minBanChaySanPham = 10.0;
            var maxBanKemSanPham = 5.0;

            var sanPhamBanChay = allSanPham
                .Where(x => x.TongSoLuong >= minBanChaySanPham)
                .OrderByDescending(x => x.TongSoLuong)
                .Take(top)
                .ToList();

            var sanPhamBanKem = allSanPham
                .Where(x => x.TongSoLuong <= maxBanKemSanPham)
                .OrderBy(x => x.TongSoLuong)
                .Take(top)
                .ToList();

            var allGioQua = chiTietDonHangs
                .Where(ct => ct.MaGq.HasValue)
                .GroupBy(ct => ct.MaGq)
                .Select(g => new
                {
                    MaGq = g.Key,
                    TenGq = g.First().MaGqNavigation?.TenGioQua ?? "(Không rõ tên)",
                    TongSoLuong = g.Sum(ct => ct.SoLuong ?? 0)
                })
                .ToList();

            var minBanChayGioQua = 5.0;
            var maxBanKemGioQua = 3.0;

            var gioQuaBanChay = allGioQua
                .Where(x => x.TongSoLuong >= minBanChayGioQua)
                .OrderByDescending(x => x.TongSoLuong)
                .Take(top)
                .ToList();

            var gioQuaBanKem = allGioQua
                .Where(x => x.TongSoLuong <= maxBanKemGioQua)
                .OrderBy(x => x.TongSoLuong)
                .Take(top)
                .ToList();

            // --- Tạo file Excel ---
            using var workbook = new XLWorkbook();

            // 1. SP Bán chạy
            var ws1 = workbook.Worksheets.Add("SP Bán chạy");
            ws1.Cell(1, 1).Value = "Mã SP";
            ws1.Cell(1, 2).Value = "Tên SP";
            ws1.Cell(1, 3).Value = "Tổng số lượng";
            ws1.Range(1, 1, 1, 3).Style.Font.Bold = true;
            ws1.Range(1, 1, 1, 3).Style.Fill.BackgroundColor = XLColor.LightGreen;

            int row = 2;
            foreach (var sp in sanPhamBanChay)
            {
                ws1.Cell(row, 1).Value = sp.MaSp;
                ws1.Cell(row, 2).Value = sp.TenSp;
                ws1.Cell(row, 3).Value = sp.TongSoLuong;
                row++;
            }
            ws1.Columns().AdjustToContents();
            ws1.Range(1, 1, row - 1, 3).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            ws1.Range(1, 1, row - 1, 3).Style.Border.InsideBorder = XLBorderStyleValues.Thin;

            // 2. SP Bán kém
            var ws2 = workbook.Worksheets.Add("SP Bán kém");
            ws2.Cell(1, 1).Value = "Mã SP";
            ws2.Cell(1, 2).Value = "Tên SP";
            ws2.Cell(1, 3).Value = "Tổng số lượng";
            ws2.Range(1, 1, 1, 3).Style.Font.Bold = true;
            ws2.Range(1, 1, 1, 3).Style.Fill.BackgroundColor = XLColor.LightYellow;

            row = 2;
            foreach (var sp in sanPhamBanKem)
            {
                ws2.Cell(row, 1).Value = sp.MaSp;
                ws2.Cell(row, 2).Value = sp.TenSp;
                ws2.Cell(row, 3).Value = sp.TongSoLuong;
                row++;
            }
            ws2.Columns().AdjustToContents();
            ws2.Range(1, 1, row - 1, 3).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            ws2.Range(1, 1, row - 1, 3).Style.Border.InsideBorder = XLBorderStyleValues.Thin;

            // 3. Giỏ quà Bán chạy
            var ws3 = workbook.Worksheets.Add("Giỏ quà Bán chạy");
            ws3.Cell(1, 1).Value = "Mã GQ";
            ws3.Cell(1, 2).Value = "Tên Giỏ quà";
            ws3.Cell(1, 3).Value = "Tổng số lượng";
            ws3.Range(1, 1, 1, 3).Style.Font.Bold = true;
            ws3.Range(1, 1, 1, 3).Style.Fill.BackgroundColor = XLColor.LightBlue;

            row = 2;
            foreach (var gq in gioQuaBanChay)
            {
                ws3.Cell(row, 1).Value = gq.MaGq;
                ws3.Cell(row, 2).Value = gq.TenGq;
                ws3.Cell(row, 3).Value = gq.TongSoLuong;
                row++;
            }
            ws3.Columns().AdjustToContents();
            ws3.Range(1, 1, row - 1, 3).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            ws3.Range(1, 1, row - 1, 3).Style.Border.InsideBorder = XLBorderStyleValues.Thin;

            // 4. Giỏ quà Bán kém
            var ws4 = workbook.Worksheets.Add("Giỏ quà Bán kém");
            ws4.Cell(1, 1).Value = "Mã GQ";
            ws4.Cell(1, 2).Value = "Tên Giỏ quà";
            ws4.Cell(1, 3).Value = "Tổng số lượng";
            ws4.Range(1, 1, 1, 3).Style.Font.Bold = true;
            ws4.Range(1, 1, 1, 3).Style.Fill.BackgroundColor = XLColor.LightGray;

            row = 2;
            foreach (var gq in gioQuaBanKem)
            {
                ws4.Cell(row, 1).Value = gq.MaGq;
                ws4.Cell(row, 2).Value = gq.TenGq;
                ws4.Cell(row, 3).Value = gq.TongSoLuong;
                row++;
            }
            ws4.Columns().AdjustToContents();
            ws4.Range(1, 1, row - 1, 3).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            ws4.Range(1, 1, row - 1, 3).Style.Border.InsideBorder = XLBorderStyleValues.Thin;

            // --- Xuất file ---
            var stream = new MemoryStream();
            workbook.SaveAs(stream);
            stream.Position = 0;

            string fileName = $"ThongKe_SanPham_GioQua_{DateTime.Now:yyyyMMddHHmmss}.xlsx";

            return File(
                stream,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                fileName
            );
        }

    }
}
