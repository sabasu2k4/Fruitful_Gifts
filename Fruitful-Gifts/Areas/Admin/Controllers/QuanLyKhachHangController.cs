using Fruitful_Gifts.Database;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
namespace Fruitful_Gifts.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class QuanLyKhachHangController : Controller
    {
        private readonly FruitfulGiftsContext _context;

        public QuanLyKhachHangController(FruitfulGiftsContext context)
        {
            _context = context;
        }
        public bool checkDangNhap()
        {
            var taiKhoanId = HttpContext.Session.GetInt32("TaiKhoanId");
            var vaiTro = HttpContext.Session.GetString("VaiTro");

            // Kiểm tra nếu có ID và vai trò là KhachHang
            if (taiKhoanId.HasValue && vaiTro == "KhachHang")
            {
                return true;
            }

            return false;
        }

        // Phương thức HttpGet để hiển thị danh sách bình luận
        [HttpGet]
        public IActionResult Index(int? page, string searchTerm, int? trangThai)
        {
            //if (!checkDangNhap())
            //{
            //    return RedirectToAction("AdminDangNhap", "Admin", new { area = "Admin" });
            //}

            int pageSize = 7;
            int pageNumber = (page ?? 1);

            // Lấy danh sách bình luận, bao gồm Khách hàng, Sản phẩm và Giỏ quà
            var binhLuansQuery = _context.BinhLuans
                                         .Include(b => b.MaKhNavigation)
                                         .Include(b => b.MaSpNavigation)
                                         .Include(b => b.MaGqNavigation)
                                         .OrderByDescending(b => b.NgayBinhLuan)
                                         .AsQueryable();

            // Tìm kiếm theo tên khách hàng, sản phẩm hoặc giỏ quà
            if (!string.IsNullOrEmpty(searchTerm))
            {
                binhLuansQuery = binhLuansQuery.Where(b =>
                    b.MaKhNavigation.TenKh.Contains(searchTerm) ||
                    (b.MaSpNavigation != null && b.MaSpNavigation.TenSp.Contains(searchTerm)) ||
                    (b.MaGqNavigation != null && b.MaGqNavigation.TenGioQua.Contains(searchTerm))
                );
            }

            // Lọc theo trạng thái hiển thị (trangThai: 1 = ẩn, 0 = hiện)
            if (trangThai.HasValue)
            {
                binhLuansQuery = binhLuansQuery.Where(b => b.TrangThai == trangThai);
            }

            // Lấy danh sách bình luận sau khi lọc
            var binhLuans = binhLuansQuery.ToList();

            // Phân trang
            var pagedBinhLuans = binhLuans.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();

            int totalItems = binhLuans.Count();
            int totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

            // Truyền thông tin vào View
            ViewBag.TotalPages = totalPages;
            ViewBag.CurrentPage = pageNumber;
            ViewBag.SearchTerm = searchTerm;
            ViewBag.TrangThai = trangThai;

            return View(pagedBinhLuans);
        }

        public async Task<IActionResult> ToggleCommentVisibility(int id)
        {
            var binhLuan = await _context.BinhLuans.FirstOrDefaultAsync(bl => bl.IdBinhLuan == id);

            if (binhLuan == null)
            {
                return NotFound();
            }

            // Đảo trạng thái: 1 => 0 hoặc 0 => 1
            binhLuan.TrangThai = (binhLuan.TrangThai == 1) ? 0 : 1;
            binhLuan.UpdatedAt = DateTime.Now;

            _context.Update(binhLuan);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = (binhLuan.TrangThai == 0)
                ? "Bình luận đã được ẩn."
                : "Bình luận đã được hiển thị.";

            return RedirectToAction(nameof(Index));
        }

        // Hiển thị danh sách liên hệ
        public IActionResult Indexlienhe(int page = 1)
        {
            int pageSize = 10; // số item mỗi trang

            var query = _context.LienHes.Include(lh => lh.MaNvNavigation);
            int totalItems = query.Count();

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


        // Xóa liên hệ (Hard delete)
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




    }
}
