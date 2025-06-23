using Fruitful_Gifts.Database;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DoAnWEBDEMO.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class QLDonHangController : Controller
    {
        private readonly FruitfulGiftsContext _context;

        public QLDonHangController(FruitfulGiftsContext context)
        {
            _context = context;
        }

        public IActionResult Index(int? trangThai, int page = 1)
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


        public IActionResult DonHangDaXoa(int page = 1, int pageSize = 10)
        {
            var donHangsQuery = _context.DonHangs
                .Where(d => d.TrangThai == 6)
                .Include(d => d.MaKhNavigation)
                .Include(d => d.ChiTietDonHangs)
                    .ThenInclude(ct => ct.MaSpNavigation)
                .Include(d => d.TrangThaiNavigation);

            int totalItems = donHangsQuery.Count();

            var danhSachDonHangDaXoa = donHangsQuery
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
        [Route("Admin/DonHang/XoaDonHang")]
        public JsonResult XoaDonHang(int maDh)
        {
            try
            {
                var donHang = _context.DonHangs.FirstOrDefault(d => d.MaDh == maDh);

                if (donHang == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy đơn hàng này." });
                }

                if (donHang.TrangThai != 3) // Chỉ có thể xóa khi trạng thái là "Hủy đơn hàng"
                {
                    return Json(new { success = false, message = "Đơn hàng không có trạng thái 'Hủy đơn hàng'." });
                }

                donHang.TrangThai = 6; // 6: Đã xóa
                donHang.UpdatedAt = DateTime.Now;

                _context.SaveChanges();

                return Json(new { success = true, message = "Đơn hàng đã được xóa thành công." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Đã xảy ra lỗi: " + ex.Message });
            }
        }
    }
}
