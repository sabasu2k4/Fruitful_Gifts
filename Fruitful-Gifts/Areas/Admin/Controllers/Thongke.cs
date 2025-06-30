using ClosedXML.Excel;
using DocumentFormat.OpenXml.InkML;
using Fruitful_Gifts.Database;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using System.Globalization;
using ClosedXML.Excel;
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
        public IActionResult ThongKeDoanhThu(string fromDate, string toDate, int? phuongThuc)
        {
            var donHangs = _context.DonHangs
                .Include(d => d.MaPtNavigation)
                .Where(d => d.TrangThai == 4); // Chỉ tính đơn hoàn thành

            // Lọc ngày
            if (DateTime.TryParseExact(fromDate, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var from))
                donHangs = donHangs.Where(d => d.NgayDatHang >= from);

            if (DateTime.TryParseExact(toDate, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var to))
                donHangs = donHangs.Where(d => d.NgayDatHang <= to.AddDays(1));

            // ✅ Lọc theo phương thức thanh toán
            if (phuongThuc.HasValue)
                donHangs = donHangs.Where(d => d.MaPt == phuongThuc.Value);

            // Dữ liệu View
            ViewBag.TongDoanhThu = donHangs.Sum(d => d.TongTienDonHang ?? 0);
            ViewBag.TongDon = donHangs.Count();

            ViewBag.DoanhThuTheoNgay = donHangs
                .GroupBy(d => d.NgayDatHang.Value.Date)
                .Select(g => new
                {
                    Ngay = g.Key,
                    DoanhThu = g.Sum(d => d.TongTienDonHang ?? 0),
                    SoDon = g.Count()
                })
                .OrderBy(g => g.Ngay)
                .ToList();
            // thống kê theo tháng
            ViewBag.DoanhThuTheoThang = donHangs
              .Where(d => d.NgayDatHang.HasValue) // Loại bỏ những đơn không có ngày
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

            // Đưa danh sách phương thức thanh toán xuống View
            ViewBag.DanhSachPT = _context.PhuongThucThanhToans.ToList();

            return View();
        }

        [HttpGet]
        public IActionResult SanPhamVaGioQuaBanChayKem(string fromDate, string toDate, int? phuongThuc, int top = 5)
        {
            // Lấy danh sách đơn hàng hoàn thành
            var donHangs = _context.DonHangs
                .Include(d => d.ChiTietDonHangs)
                .ThenInclude(ct => ct.MaSpNavigation)
                .Include(d => d.ChiTietDonHangs)
                .ThenInclude(ct => ct.MaGqNavigation)
                .Where(d => d.TrangThai == 4); // Đơn hoàn thành

            // Lọc theo ngày
            if (DateTime.TryParseExact(fromDate, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var from))
                donHangs = donHangs.Where(d => d.NgayDatHang >= from);

            if (DateTime.TryParseExact(toDate, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var to))
                donHangs = donHangs.Where(d => d.NgayDatHang <= to.AddDays(1));

            // Lọc theo phương thức thanh toán nếu có
            if (phuongThuc.HasValue)
                donHangs = donHangs.Where(d => d.MaPt == phuongThuc.Value);

            var chiTietDonHangs = donHangs.SelectMany(d => d.ChiTietDonHangs);

            // Thống kê sản phẩm - Logic cải tiến
            var allSanPham = chiTietDonHangs
                .Where(ct => ct.MaSp.HasValue)
                .GroupBy(ct => ct.MaSp)
                .Select(g => new
                {
                    MaSp = g.Key,
                    TenSp = g.FirstOrDefault().MaSpNavigation.TenSp,
                    TongSoLuong = g.Sum(ct => ct.SoLuong)
                })
                .ToList();

            // Tính toán các ngưỡng quan trọng
            var avgSanPham = allSanPham.Any() ? allSanPham.Average(x => x.TongSoLuong) : 0;
            decimal avgSanPhamValue = avgSanPham ?? 0m;

            var minBanChaySanPham = Math.Max(avgSanPhamValue * 1.5m, 5m); // Ít nhất 5 SP hoặc 150% trung bình
            var maxBanKemSanPham = Math.Min(avgSanPhamValue * 0.5m, 2m);  // Tối đa 2 SP hoặc 50% trung bình




            // Sản phẩm bán chạy (phải đạt ngưỡng tối thiểu)
            var sanPhamBanChay = allSanPham
                .Where(x => x.TongSoLuong >= minBanChaySanPham)
                .OrderByDescending(x => x.TongSoLuong)
                .Take(top)
                .ToList();

            // Sản phẩm bán kém (dưới ngưỡng tối đa)
            var sanPhamBanKem = allSanPham
                .Where(x => x.TongSoLuong <= maxBanKemSanPham)
                .OrderBy(x => x.TongSoLuong)
                .Take(top)
                .ToList();

            // Thống kê giỏ quà - Logic tương tự
            var allGioQua = chiTietDonHangs
                .Where(ct => ct.MaGq.HasValue)
                .GroupBy(ct => ct.MaGq)
                .Select(g => new
                {
                    MaGq = g.Key,
                    TenGq = g.FirstOrDefault().MaGqNavigation.TenGioQua,
                    TongSoLuong = g.Sum(ct => ct.SoLuong)
                })
                .ToList();

            var avgGioQua = allGioQua.Any() ? allGioQua.Average(x => x.TongSoLuong) : 0;
            decimal avgValue = avgGioQua ?? 0m;
            var minBanChayGioQua = Math.Max(avgValue * 1.5m, 3m); // Giỏ quà có thể đặt ngưỡng thấp hơn sản phẩm
            var maxBanKemGioQua = Math.Min(avgValue * 0.5m, 1m); // Tối đa 1 giỏ hoặc 50% trung bình

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

            // Xử lý trường hợp đặc biệt cho sản phẩm mùa vụ
            var now = DateTime.Now;
            if (!(now.Month == 3 && now.Day <= 15)) // Không phải thời điểm 8/3
            {
                sanPhamBanChay = sanPhamBanChay.Where(x => !x.TenSp.Contains("8/3")).ToList();
                gioQuaBanChay = gioQuaBanChay.Where(x => !x.TenGq.Contains("8/3")).ToList();
            }

            if (!(now.Month == 10 && now.Day <= 20)) // Không phải thời điểm 20/10
            {
                sanPhamBanChay = sanPhamBanChay.Where(x => !x.TenSp.Contains("20/10")).ToList();
                gioQuaBanChay = gioQuaBanChay.Where(x => !x.TenGq.Contains("20/10")).ToList();
            }

            ViewBag.SanPhamBanChay = sanPhamBanChay;
            ViewBag.SanPhamBanKem = sanPhamBanKem;
            ViewBag.GioQuaBanChay = gioQuaBanChay;
            ViewBag.GioQuaBanKem = gioQuaBanKem;

            ViewBag.FromDate = fromDate;
            ViewBag.ToDate = toDate;
            ViewBag.PhuongThuc = phuongThuc;
            ViewBag.DanhSachPT = _context.PhuongThucThanhToans.ToList();

            // Thêm các ngưỡng để hiển thị
            ViewBag.AvgSanPham = avgSanPham;
            ViewBag.MinBanChaySanPham = minBanChaySanPham;
            ViewBag.MaxBanKemSanPham = maxBanKemSanPham;
            ViewBag.AvgGioQua = avgGioQua;
            ViewBag.MinBanChayGioQua = minBanChayGioQua;
            ViewBag.MaxBanKemGioQua = maxBanKemGioQua;

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
    }
}
