using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Fruitful_Gifts.Database;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Fruitful_Gifts.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class QuanLyDonHangController : Controller
    {
        private readonly FruitfulGiftsContext _context;

        public QuanLyDonHangController(FruitfulGiftsContext context)
        {
            _context = context;
        }

        
        public async Task<IActionResult> Index(string searchString, int? trangThai, string sortOrder)
        {
            ViewData["CurrentFilter"] = searchString;
            ViewData["CurrentStatus"] = trangThai;
            ViewData["DateSortParm"] = string.IsNullOrEmpty(sortOrder) ? "date_desc" : "";
            ViewData["TotalSortParm"] = sortOrder == "total" ? "total_desc" : "total";

            // Thêm danh sách trạng thái vào ViewBag
            ViewBag.TrangThaiList = new List<SelectListItem>
            {
                new SelectListItem { Value = "1", Text = "⏳ Chờ xác nhận" },
                new SelectListItem { Value = "2", Text = "✅ Đã xác nhận" },
                new SelectListItem { Value = "3", Text = "🚚 Đang giao hàng" },
                new SelectListItem { Value = "4", Text = "✔️ Đã giao hàng" },
                new SelectListItem { Value = "5", Text = "🔄 Hoàn hàng" },
                new SelectListItem { Value = "6", Text = "❌ Đã hủy" },
                new SelectListItem { Value = "7", Text = "⚠️ Giao hàng thất bại" },
                new SelectListItem { Value = "8", Text = "🚫 Từ chối" }
            };

            var donHangs = _context.DonHangs
                .Include(d => d.MaKhNavigation)
                .Include(d => d.MaNvNavigation)
                .Include(d => d.TrangThaiNavigation)
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                donHangs = donHangs.Where(d =>
                    d.MaDh.ToString().Contains(searchString) ||
                    (d.MaKhNavigation != null && d.MaKhNavigation.TenKh.Contains(searchString)) ||
                    (d.SoDienThoai != null && d.SoDienThoai.Contains(searchString)));
            }

            if (trangThai.HasValue)
            {
                donHangs = donHangs.Where(d => d.TrangThai == trangThai);
            }

            switch (sortOrder)
            {
                case "date_desc":
                    donHangs = donHangs.OrderByDescending(d => d.NgayDatHang);
                    break;
                case "total":
                    donHangs = donHangs.OrderBy(d => d.TongTienDonHang);
                    break;
                case "total_desc":
                    donHangs = donHangs.OrderByDescending(d => d.TongTienDonHang);
                    break;
                default:
                    donHangs = donHangs.OrderBy(d => d.NgayDatHang);
                    break;
            }

            return View(await donHangs.ToListAsync());
        }

        [HttpGet]
        public async Task<IActionResult> ChiTiet(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var donHang = await _context.DonHangs
                .Include(d => d.MaKhNavigation)
                .Include(d => d.MaNvNavigation)
                .Include(d => d.MaPtNavigation)
                .Include(d => d.TrangThaiNavigation)
                .Include(d => d.ChiTietDonHangs)
                    .ThenInclude(ct => ct.MaSpNavigation)
                .FirstOrDefaultAsync(m => m.MaDh == id);

            if (donHang == null)
            {
                return NotFound();
            }

            return View(donHang);
        }

        [HttpGet]
        public async Task<IActionResult> CapNhatTrangThai(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var donHang = await _context.DonHangs.FindAsync(id);
            if (donHang == null)
            {
                return NotFound();
            }

            ViewBag.TrangThaiList = new List<SelectListItem>
            {
                new SelectListItem { Value = "1", Text = "⏳ Chờ xác nhận" },
                new SelectListItem { Value = "2", Text = "✅ Đã xác nhận" },
                new SelectListItem { Value = "3", Text = "🚚 Đang giao hàng" },
                new SelectListItem { Value = "4", Text = "✔️ Đã giao hàng" },
                new SelectListItem { Value = "5", Text = "🔄 Hoàn hàng" },
                new SelectListItem { Value = "6", Text = "❌ Đã hủy" },
                new SelectListItem { Value = "7", Text = "⚠️ Giao hàng thất bại" },
                new SelectListItem { Value = "8", Text = "🚫 Từ chối" }
            };

            return View(donHang);
        }


        [HttpPost("Admin/QuanLyDonHang/CapNhatTrangThai/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CapNhatTrangThai(int id, [Bind("MaDh,TrangThai,GhiChu")] DonHang donHang)
        {
            if (id != donHang.MaDh)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var existingDonHang = await _context.DonHangs.FindAsync(id);
                    if (existingDonHang == null)
                    {
                        return NotFound();
                    }

                    existingDonHang.TrangThai = donHang.TrangThai;
                    existingDonHang.GhiChu = donHang.GhiChu;
                    existingDonHang.UpdatedAt = DateTime.Now;

                    _context.Update(existingDonHang);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = "Cập nhật trạng thái đơn hàng thành công!";
                    return RedirectToAction("ChiTiet", new { id = donHang.MaDh });
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!DonHangExists(donHang.MaDh))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }

            // Nếu ModelState không valid, cần tải lại dữ liệu dropdown
            ViewBag.TrangThaiList = new List<SelectListItem>
    {
        new SelectListItem { Value = "1", Text = "⏳ Chờ xác nhận" },
        new SelectListItem { Value = "2", Text = "✅ Đã xác nhận" },
        new SelectListItem { Value = "3", Text = "🚚 Đang giao hàng" },
        new SelectListItem { Value = "4", Text = "✔️ Đã giao hàng" },
        new SelectListItem { Value = "5", Text = "🔄 Hoàn hàng" },
        new SelectListItem { Value = "6", Text = "❌ Đã hủy" },
        new SelectListItem { Value = "7", Text = "⚠️ Giao hàng thất bại" },
        new SelectListItem { Value = "8", Text = "🚫 Từ chối" }
    };

            return View(donHang);
        }
      
        public async Task<IActionResult> HuyDonHang(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var donHang = await _context.DonHangs
                .Include(d => d.TrangThaiNavigation)
                .FirstOrDefaultAsync(m => m.MaDh == id);

            if (donHang == null)
            {
                return NotFound();
            }

            // Only allow canceling for certain statuses
            if (donHang.TrangThai != 1 && donHang.TrangThai != 2)
            {
                TempData["ErrorMessage"] = "Chỉ có thể hủy đơn hàng ở trạng thái 'Chờ xác nhận' hoặc 'Đã xác nhận'";
                return RedirectToAction(nameof(ChiTiet), new { id });
            }

            return View(donHang);
        }

        // POST: Admin/QuanLyDonHang/HuyDonHang/5
        [HttpPost, ActionName("HuyDonHang")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> HuyDonHangConfirmed(int id)
        {
            var donHang = await _context.DonHangs.FindAsync(id);
            if (donHang == null)
            {
                return NotFound();
            }

            donHang.TrangThai = 6; // Đã hủy
            donHang.UpdatedAt = DateTime.Now;
            _context.Update(donHang);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Hủy đơn hàng thành công!";
            return RedirectToAction(nameof(ChiTiet), new { id });
        }

        private bool DonHangExists(int id)
        {
            return _context.DonHangs.Any(e => e.MaDh == id);
        }
    }
}