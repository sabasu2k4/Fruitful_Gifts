using Microsoft.AspNetCore.Mvc;
using Fruitful_Gifts.Database;
using Microsoft.EntityFrameworkCore;
using X.PagedList.Extensions;

namespace Fruitful_Gifts.Areas.Admin.Controllers;

[Area("Admin")]
public class QuanLyNhanVienController : Controller
{
    private readonly FruitfulGiftsContext _context;

    public QuanLyNhanVienController(FruitfulGiftsContext context)
    {
        _context = context;
    }

    public IActionResult DanhSachNhanVien(string tuKhoa, int? page)
    {
        int pageSize = 10;
        int pageNumber = page ?? 1;

        var query = _context.NhanViens
            .Include(nv => nv.TaiKhoan)
            .Include(nv => nv.Luongs)
            .Include(nv => nv.ChamCongs)
            .AsQueryable();

        if (!string.IsNullOrEmpty(tuKhoa))
        {
            tuKhoa = tuKhoa.Trim().ToLower();
            query = query.Where(nv =>
                nv.TenNv.ToLower().Contains(tuKhoa) ||
                nv.Sdt.Contains(tuKhoa) ||
                nv.Email.ToLower().Contains(tuKhoa)
            );
        }

        ViewBag.TuKhoa = tuKhoa;
        ViewBag.TaiKhoans = _context.TaiKhoans.ToList();

        var pagedList = query.OrderBy(nv => nv.MaNv).ToPagedList(pageNumber, pageSize);
        return View(pagedList);
    }

    public IActionResult ThemNhanVien()
    {
        ViewBag.TaiKhoans = _context.TaiKhoans.ToList();
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult ThemNhanVien(NhanVien nv, string TenDangNhap, string MatKhau)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.TaiKhoans = _context.TaiKhoans.ToList();
            return View(nv);
        }

        if (_context.TaiKhoans.Any(t => t.TenDangNhap == TenDangNhap))
        {
            ModelState.AddModelError("", "Tên đăng nhập đã tồn tại");
            ViewBag.TaiKhoans = _context.TaiKhoans.ToList();
            return View(nv);
        }

        using (var transaction = _context.Database.BeginTransaction())
        {
            try
            {
                string quyen = nv.ChucVu switch
                {
                    "Quản lý" => "quanly",
                    "Thu ngân" => "thungan",
                    "Nhân viên kho" => "kho",
                    _ => "banhang" // Mặc định là nhân viên bán hàng
                };
                var taiKhoan = new TaiKhoan
                {
                    TenDangNhap = TenDangNhap,
                    MatKhau = BCrypt.Net.BCrypt.HashPassword(MatKhau),
                    VaiTro = "NhanVien",
                    Quyen = quyen, // Thêm quyền ở đây
                    TrangThai = 1,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                };

                _context.TaiKhoans.Add(taiKhoan);
                _context.SaveChanges();

                // Gán thông tin nhân viên
                nv.TaiKhoanId = taiKhoan.TaiKhoanId;
                nv.CreatedAt = DateTime.Now;
                nv.UpdatedAt = DateTime.Now;
                nv.TrangThai = 1;

                // Nếu có ngày sinh từ form (DateOnly), giữ nguyên
                if (nv.NgaySinh.HasValue)
                    nv.NgaySinh = nv.NgaySinh.Value;

                _context.NhanViens.Add(nv);
                _context.SaveChanges();

                // Tạo lương mặc định
                var luong = new Luong
                {
                    NhanVienId = nv.MaNv,
                    TuNgay = DateOnly.FromDateTime(new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1)),
                    DenNgay = DateOnly.FromDateTime(DateTime.Now),
                    LuongCoBan = 0,
                    SoNgayCong = 0,
                    LuongPhuCap = 0,
                    Thuong = 0,
                    Phat = 0,
                    DaThanhToan = false,
                    CreatedAt = DateTime.Now
                };

                _context.Luongs.Add(luong);
                _context.SaveChanges();

                transaction.Commit();

                TempData["SuccessMessage"] = "Thêm nhân viên thành công";
                return RedirectToAction("DanhSachNhanVien");
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                ModelState.AddModelError("", "Có lỗi xảy ra khi thêm nhân viên: " + ex.Message);
                ViewBag.TaiKhoans = _context.TaiKhoans.ToList();
                return View(nv);
            }
        }
    }


    [HttpPost]
    public IActionResult CapNhatNhanVien(int MaNv, string TenNv, string Sdt, string Email, string? DiaChi, string? ChucVu, DateOnly? NgayVaoLam,
    string? MatKhau, int? TaiKhoanId, decimal? LuongCoBan, int? SoNgayCong, decimal? LuongPhuCap, decimal? Thuong, decimal? Phat)
    {
        var nv = _context.NhanViens.Find(MaNv);
        if (nv == null)
            return Json(new { success = false, message = "Không tìm thấy nhân viên." });

        string quyen = nv.ChucVu switch
        {
            "Quản lý" => "quanly",
            "Thu ngân" => "thungan",
            "Nhân viên kho" => "kho",
            _ => "banhang" // Mặc định là nhân viên bán hàng
        };

        // Cập nhật nhân viên
        nv.TenNv = TenNv;
        nv.Sdt = Sdt;
        nv.Email = Email;
        nv.DiaChi = DiaChi;
        nv.ChucVu = ChucVu;
        nv.NgayVaoLam = NgayVaoLam;
        nv.TaiKhoanId = TaiKhoanId;
        nv.UpdatedAt = DateTime.Now;

        // Cập nhật mật khẩu nếu có nhập
        if (!string.IsNullOrEmpty(MatKhau) && TaiKhoanId.HasValue)
        {
            var tk = _context.TaiKhoans.Find(TaiKhoanId.Value);
            if (tk != null)
            {
                tk.MatKhau = BCrypt.Net.BCrypt.HashPassword(MatKhau);
                tk.UpdatedAt = DateTime.Now;
            }
        }

        // Cập nhật lương gần nhất
        var luong = _context.Luongs
            .Where(l => l.NhanVienId == MaNv)
            .OrderByDescending(l => l.TuNgay)
            .FirstOrDefault();

        if (luong != null)
        {
            luong.LuongCoBan = LuongCoBan;
            luong.SoNgayCong = SoNgayCong;
            luong.LuongPhuCap = LuongPhuCap;
            luong.Thuong = Thuong;
            luong.Phat = Phat;
            luong.UpdatedAt = DateTime.Now;

            // Tính lại tổng lương
            int ngayCongChuan = 26;
            decimal coBan = LuongCoBan ?? 0;
            int cong = SoNgayCong ?? 0;
            decimal phuCap = LuongPhuCap ?? 0;
            decimal thuong = Thuong ?? 0;
            decimal phat = Phat ?? 0;

            luong.TongLuong = (coBan / ngayCongChuan) * cong + phuCap + thuong - phat;
        }

        _context.SaveChanges();
        return Json(new { success = true });
    }

    [HttpPost]
    public IActionResult CheckinNhanVien(int maNv)
    {
        var nhanVien = _context.NhanViens.Find(maNv);
        if (nhanVien == null)
            return Json(new { success = false, message = "Không tìm thấy nhân viên." });

        var homNay = DateOnly.FromDateTime(DateTime.Today);

        // Kiểm tra đã chấm công hôm nay chưa
        bool daCham = _context.ChamCongs.Any(c => c.NhanVienId == maNv && c.Ngay == homNay);
        if (daCham)
        {
            return Json(new { success = false, message = "Đã chấm công hôm nay." });
        }

        // Tạo bản ghi chấm công
        var chamCong = new ChamCong
        {
            NhanVienId = maNv,
            Ngay = homNay,
            GioCheckIn = TimeOnly.FromDateTime(DateTime.Now)
        };
        _context.ChamCongs.Add(chamCong);

        // Cập nhật lương tháng
        var thangNay = new DateOnly(DateTime.Today.Year, DateTime.Today.Month, 1);
        var luongThang = _context.Luongs.FirstOrDefault(l => l.NhanVienId == maNv && l.TuNgay == thangNay);
        if (luongThang == null)
        {
            luongThang = new Luong
            {
                NhanVienId = maNv,
                TuNgay = thangNay,
                DenNgay = DateOnly.FromDateTime(DateTime.Today),
                LuongCoBan = 0,
                SoNgayCong = 0,
                LuongPhuCap = 0,
                Thuong = 0,
                Phat = 0,
                DaThanhToan = false,
                CreatedAt = DateTime.Now
            };
            _context.Luongs.Add(luongThang);
        }

        luongThang.SoNgayCong += 1;
        luongThang.UpdatedAt = DateTime.Now;

        // Tính lại tổng lương
        decimal coBan = luongThang.LuongCoBan ?? 0;
        int ngayCongChuan = 26;
        luongThang.TongLuong = (coBan / ngayCongChuan) * luongThang.SoNgayCong
            + (luongThang.LuongPhuCap ?? 0)
            + (luongThang.Thuong ?? 0)
            - (luongThang.Phat ?? 0);

        _context.SaveChanges();

        return Json(new
        {
            success = true,
            checkInTime = DateTime.Now.ToString("HH:mm")
        });
    }

    public IActionResult BangLuong(int? page, int? thang, int? nam)
    {
        int pageSize = 10;
        int pageNumber = page ?? 1;

        // Mặc định là tháng và năm hiện tại nếu không có giá trị
        var now = DateTime.Now;
        int selectedMonth = thang ?? now.Month;
        int selectedYear = nam ?? now.Year;

        // Lấy dữ liệu lương theo tháng/năm
        var query = _context.Luongs
            .Include(l => l.NhanVien)
            .Where(l => l.TuNgay.HasValue &&
                       l.TuNgay.Value.Month == selectedMonth &&
                       l.TuNgay.Value.Year == selectedYear)
            .OrderByDescending(l => l.TuNgay)
            .AsQueryable();

        // Truyền dữ liệu sang view
        ViewBag.Thang = selectedMonth;
        ViewBag.Nam = selectedYear;
        ViewBag.DanhSachThang = Enumerable.Range(1, 12).ToList();
        ViewBag.DanhSachNam = Enumerable.Range(now.Year - 5, 6).ToList(); // 5 năm gần đây + năm hiện tại

        var pagedList = query.ToPagedList(pageNumber, pageSize);
        return View(pagedList);
    }

    [HttpPost]
    public IActionResult ThanhToanLuong(int maLuong)
    {
        try
        {
            var luong = _context.Luongs.Find(maLuong);
            if (luong == null)
                return Json(new { success = false, message = "Không tìm thấy bản ghi lương" });

            luong.DaThanhToan = true;
            luong.NgayThanhToan = DateTime.Now;
            luong.UpdatedAt = DateTime.Now;

            _context.SaveChanges();
            return Json(new { success = true });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = ex.Message });
        }
    }

    public IActionResult ChiTietLuong(int maLuong)
    {
        var luong = _context.Luongs
            .Include(l => l.NhanVien)
            .FirstOrDefault(l => l.MaLuong == maLuong);

        if (luong == null)
            return Content("Không tìm thấy thông tin lương");

        // Lấy danh sách chấm công của nhân viên trong tháng
        var chamCongs = _context.ChamCongs
            .Where(c => c.NhanVienId == luong.NhanVienId &&
                       c.Ngay >= luong.TuNgay &&
                       c.Ngay <= luong.DenNgay)
            .OrderBy(c => c.Ngay)
            .ToList();

        ViewBag.ChamCongs = chamCongs;
        return PartialView("_ChiTietLuong", luong);
    }

}
