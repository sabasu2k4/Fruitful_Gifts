using DocumentFormat.OpenXml.Wordprocessing;
using Fruitful_Gifts.Database;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
namespace Fruitful_Gifts.Areas.Admin.Controllers;
using ClosedXML.Excel;
using DocumentFormat.OpenXml.InkML;
using System.IO;

[Area("Admin")]
public class QuanLyKhachHangController : Controller
{
    private readonly FruitfulGiftsContext _context;
    private readonly ILogger<QuanLyKhachHangController> _logger;
    private const int PageSize = 10;
    public QuanLyKhachHangController(FruitfulGiftsContext context,
                                       ILogger<QuanLyKhachHangController> logger)
    {
        _context = context;
        _logger = logger;
    }

    public bool checkDangNhap()
    {
        var taiKhoanId = HttpContext.Session.GetInt32("TaiKhoanId");
        var vaiTro = HttpContext.Session.GetString("VaiTro");

        // ✅ Kiểm tra Admin
        if (taiKhoanId.HasValue && vaiTro == "Admin")
        {
            return true;
        }

        return false;
    }

    public async Task<IActionResult> QLKhachHang(int? page, string searchString, string sortOrder)
    {
        int pageSize = 8;
        int pageNumber = page ?? 1;

        // Thiết lập các tham số sắp xếp
        ViewData["CurrentSort"] = sortOrder;
        ViewData["NameSort"] = string.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
        ViewData["DateSort"] = sortOrder == "Date" ? "date_desc" : "Date";
        ViewData["StatusSort"] = sortOrder == "Status" ? "status_desc" : "Status";
        ViewData["CurrentFilter"] = searchString;

        // Truy vấn bao gồm thông tin tài khoản
        var customers = _context.KhachHangs
            .Include(k => k.TaiKhoan) // Thêm include để load thông tin tài khoản
            .AsQueryable();

        // Tìm kiếm
        if (!string.IsNullOrEmpty(searchString))
        {
            customers = customers.Where(c =>
                c.HoKh.Contains(searchString) ||
                c.TenKh.Contains(searchString) ||
                c.Email.Contains(searchString) ||
                c.Sdt.Contains(searchString));
        }

        // Sắp xếp
        switch (sortOrder)
        {
            case "name_desc":
                customers = customers.OrderByDescending(c => c.TenKh);
                break;
            case "Date":
                customers = customers.OrderBy(c => c.CreatedAt);
                break;
            case "date_desc":
                customers = customers.OrderByDescending(c => c.CreatedAt);
                break;
            case "Status":
                customers = customers.OrderBy(c => c.TaiKhoan.TrangThai); // Sửa thành TrangThai từ TaiKhoan
                break;
            case "status_desc":
                customers = customers.OrderByDescending(c => c.TaiKhoan.TrangThai); // Sửa thành TrangThai từ TaiKhoan
                break;
            default:
                customers = customers.OrderBy(c => c.TenKh);
                break;
        }

        // Phân trang
        int totalCount = await customers.CountAsync();
        var items = await customers
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .AsNoTracking()
            .ToListAsync();

        // Truyền dữ liệu phân trang
        ViewBag.CurrentPage = pageNumber;
        ViewBag.PageSize = pageSize;
        ViewBag.TotalCount = totalCount;
        ViewBag.TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
        // Thêm thống kê
        ViewBag.TotalCustomers = await _context.KhachHangs.CountAsync();
        ViewBag.TotalLockedAccounts = await _context.TaiKhoans.CountAsync(t => t.TrangThai == 0);
        ViewBag.TotalActiveAccounts = await _context.TaiKhoans.CountAsync(t => t.TrangThai == 1);
        return View(items);
    }
    public async Task<IActionResult> DetailsKH(int id, int pageNumber = 1)
    {
        int pageSize = 8;

        // Lấy thông tin khách hàng
        var customer = await _context.KhachHangs
            .FirstOrDefaultAsync(m => m.MaKh == id);

        if (customer == null)
        {
            return NotFound();
        }

        // Truy vấn đơn hàng khách hàng theo ngày giảm dần
        var ordersQuery = _context.DonHangs
            .Where(d => d.MaKh == id)
            .OrderByDescending(d => d.NgayDatHang)
            .Include(d => d.ChiTietDonHangs);

        // Tổng số đơn hàng
        int totalOrders = await ordersQuery.CountAsync();

        // Tính tổng số trang
        int totalPages = (int)Math.Ceiling(totalOrders / (double)pageSize);

        // Lấy dữ liệu đơn hàng phân trang
        var orders = await ordersQuery
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .AsNoTracking()
            .ToListAsync();

        // Truyền dữ liệu qua ViewBag
        ViewBag.Orders = orders;
        ViewBag.CurrentPage = pageNumber;
        ViewBag.TotalPages = totalPages;

        // Thống kê tổng đơn và tổng chi tiêu
        ViewBag.TotalOrders = totalOrders;
        ViewBag.TotalSpent = await _context.DonHangs
            .Where(d => d.MaKh == id && d.TrangThai == 4)
            .SumAsync(d => (decimal?)d.TongTienDonHang) ?? 0;
   


        return View(customer);
    }
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleLock([FromBody] ToggleLockRequest request)
    {
        try
        {
            // 1. Kiểm tra đăng nhập
            if (!checkDangNhap())
                return Json(new { success = false, message = "Bạn không có quyền thực hiện thao tác này." });

            // 2. Tìm tài khoản (cách tối ưu hơn)
            var taiKhoan = await _context.TaiKhoans
                .Include(t => t.KhachHang) // Load thông tin khách hàng
                .Where(t => t.KhachHang != null && t.KhachHang.MaKh == request.id)
                .FirstOrDefaultAsync();

            if (taiKhoan == null)
                return Json(new
                {
                    success = false,
                    message = "Tài khoản không tồn tại hoặc không liên kết với khách hàng."
                });

            // 3. Đảo trạng thái
            int newStatus = taiKhoan.TrangThai == 1 ? 0 : 1;

            // 4. Cập nhật
            taiKhoan.TrangThai = newStatus;
            taiKhoan.UpdatedAt = DateTime.UtcNow; // Sử dụng UTC

            // 5. Lưu thay đổi
            await _context.SaveChangesAsync();

            // 6. Trả kết quả
            return Json(new
            {
                success = true,
                message = $"Đã {(newStatus == 1 ? "mở khóa" : "khóa")} tài khoản thành công!",
                newStatus = newStatus,
                customerName = taiKhoan.KhachHang?.HoKh + " " + taiKhoan.KhachHang?.TenKh
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi cập nhật trạng thái tài khoản. ID: {Id}", request.id);
            return Json(new
            {
                success = false,
                message = $"Lỗi hệ thống: {ex.Message}"
            });
        }
    }
    public class ToggleLockRequest
    {
        public int id { get; set; }
    }

    // Phương thức HttpGet để hiển thị danh sách bình luận
    [HttpGet]
    public IActionResult Index(int? page, string searchTerm, int? trangThai, int? soSao)
    {
        int pageSize = 7;
        int pageNumber = (page ?? 1);

        var binhLuansQuery = _context.BinhLuans
            .Include(b => b.MaKhNavigation)
            .Include(b => b.MaSpNavigation)
            .Include(b => b.MaGqNavigation)
            .OrderByDescending(b => b.NgayBinhLuan)
            .AsQueryable();

        // Tìm kiếm theo tên KH, tên sản phẩm, hoặc giỏ quà
        if (!string.IsNullOrEmpty(searchTerm))
        {
            binhLuansQuery = binhLuansQuery.Where(b =>
                b.MaKhNavigation.TenKh.Contains(searchTerm) ||
                (b.MaSpNavigation != null && b.MaSpNavigation.TenSp.Contains(searchTerm)) ||
                (b.MaGqNavigation != null && b.MaGqNavigation.TenGioQua.Contains(searchTerm))
            );
        }

        // Lọc theo trạng thái hiển thị
        if (trangThai.HasValue)
        {
            binhLuansQuery = binhLuansQuery.Where(b => b.TrangThai == trangThai);
        }
        // Lọc theo số sao
        if (soSao.HasValue)
        {
            binhLuansQuery = binhLuansQuery.Where(b => b.SoSao == soSao);
        }

        // Truy xuất dữ liệu
        var binhLuans = binhLuansQuery.ToList();

        // Thống kê nhanh
        ViewBag.TotalItems = binhLuans.Count();                             // Tổng bình luận đang hiển thị (theo filter)
        ViewBag.TotalHienThi = binhLuans.Count(b => b.TrangThai == 1);     // Hiển thị
        ViewBag.TotalAn = binhLuans.Count(b => b.TrangThai == 0);          // Ẩn
        ViewBag.MoiNhat = binhLuans.OrderByDescending(b => b.NgayBinhLuan)
                                   .FirstOrDefault()?.NgayBinhLuan;

        // Phân trang
        var pagedBinhLuans = binhLuans.Skip((pageNumber - 1) * pageSize)
                                      .Take(pageSize)
                                      .ToList();

        int totalItems = binhLuans.Count();
        int totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

        // Truyền dữ liệu cho view
        ViewBag.TotalPages = totalPages;
        ViewBag.CurrentPage = pageNumber;
        ViewBag.SearchTerm = searchTerm;
        ViewBag.TrangThai = trangThai;
        ViewBag.SoSao = soSao;
        return View(pagedBinhLuans);
    }

    // Cập nhật trạng thái hiển thị của bình luận
    public async Task<IActionResult> ToggleCommentVisibility(int id)
    {
        var binhLuan = await _context.BinhLuans.FirstOrDefaultAsync(bl => bl.IdBinhLuan == id);

        if (binhLuan == null)
        {
            return NotFound();
        }

        binhLuan.TrangThai = (binhLuan.TrangThai == 1) ? 0 : 1;
        binhLuan.UpdatedAt = DateTime.Now;

        _context.Update(binhLuan);
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = (binhLuan.TrangThai == 0)
            ? "Bình luận đã được ẩn."
            : "Bình luận đã được hiển thị.";

        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Details(int id)
    {
        var binhLuan = await _context.BinhLuans
            .Include(b => b.MaKhNavigation)
            .Include(b => b.MaSpNavigation)
            .Include(b => b.MaGqNavigation)
            .FirstOrDefaultAsync(b => b.IdBinhLuan == id);

        if (binhLuan == null)
        {
            return NotFound();
        }

        return View(binhLuan);
    }

    // Hiển thị danh sách liên hệ
    public IActionResult Indexlienhe(int page = 1)
    {
        int pageSize = 10; // số item mỗi trang

        var query = _context.LienHes.Include(lh => lh.MaNvNavigation);
        int totalItems = query.Count();
        ViewBag.TotalItems = query.Count();
        var dslh = query
            .OrderBy(lh => lh.MaLh)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        // Truyền dữ liệu phân trang qua ViewBag
        ViewBag.CurrentPage = page;
        ViewBag.TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

        return View(dslh);  // truyền trực tiếp danh sách Liên hệ
    }


    // Xóa liên hệ 
    [HttpPost]
    public IActionResult XoaLienHe(int maLh)
    {
        var item = _context.LienHes.FirstOrDefault(x => x.MaLh == maLh);
        if (item == null)
        {
            return Json(new { success = false, message = "Không tìm thấy liên hệ." });
        }

        _context.LienHes.Remove(item);
        _context.SaveChanges();

        return Json(new { success = true, message = "Đã xóa liên hệ thành công!" });
    }

    // Cập nhật trạng thái liên hệ (ví dụ: Đã xử lý / Chưa xử lý)
    [HttpPost]
    public IActionResult CapNhatTrangThai(int maLh)
    {
        var item = _context.LienHes.FirstOrDefault(x => x.MaLh == maLh);
        if (item == null)
        {
            return Json(new { success = false, message = "Không tìm thấy liên hệ." });
        }

        // Gán nhân viên xử lý (ví dụ: mã nhân viên hiện tại là 1)
        item.MaNv = 1; // Bạn nên thay bằng mã đăng nhập thực tế nếu có Auth

        // Đảo trạng thái: true => false và ngược lại
        item.TrangThai = !item.TrangThai;
        _context.SaveChanges();

        return Json(new
        {
            success = true,
            newStatus = item.TrangThai,
            message = "Đã cập nhật trạng thái liên hệ."
        });
    }




    public async Task<IActionResult> ExportToExcel()
    {
        var customers = await _context.KhachHangs
            .Include(k => k.TaiKhoan)
            .AsNoTracking()
            .ToListAsync();

        using (var workbook = new XLWorkbook())
        {
            var worksheet = workbook.Worksheets.Add("DanhSachKhachHang");

            // Tạo tiêu đề
            worksheet.Cell(1, 1).Value = "Họ tên";
            worksheet.Cell(1, 2).Value = "Email";
            worksheet.Cell(1, 3).Value = "SĐT";
            worksheet.Cell(1, 4).Value = "Ngày tạo";
            worksheet.Cell(1, 5).Value = "Trạng thái";

            // Định dạng header: in đậm, nền xám nhạt, canh giữa
            var headerRange = worksheet.Range("A1:E1");
            headerRange.Style.Font.Bold = true;
            headerRange.Style.Fill.BackgroundColor = XLColor.LightGray;
            headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            headerRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            headerRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

            int row = 2;
            foreach (var kh in customers)
            {
                worksheet.Cell(row, 1).Value = $"{kh.HoKh} {kh.TenKh}";
                worksheet.Cell(row, 2).Value = kh.Email;
                worksheet.Cell(row, 3).Value = kh.Sdt;
                worksheet.Cell(row, 4).Value = kh.CreatedAt?.ToString("dd/MM/yyyy");
                worksheet.Cell(row, 5).Value = kh.TaiKhoan?.TrangThai == 1 ? "Hoạt động" : "Đã khóa";

                // Tạo border cho hàng dữ liệu
                var dataRowRange = worksheet.Range(row, 1, row, 5);
                dataRowRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                dataRowRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

                // Canh lề trái cho dữ liệu
                dataRowRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;

                row++;
            }

            // Tự động giãn cột
            worksheet.Columns().AdjustToContents();

            // Freeze header
            worksheet.SheetView.FreezeRows(1);

            using (var stream = new MemoryStream())
            {
                workbook.SaveAs(stream);
                var content = stream.ToArray();
                return File(content,
                            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                            "DanhSachKhachHang.xlsx");
            }
        }
    }

}
