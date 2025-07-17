using Fruitful_Gifts.Database;
using Microsoft.AspNetCore.Mvc;
using X.PagedList.Extensions;

namespace Fruitful_Gifts.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class QuanLyNhaCungCapController : Controller
    {
        private readonly FruitfulGiftsContext _context;

        public QuanLyNhaCungCapController(FruitfulGiftsContext context)
        {
            _context = context;
        }

        public IActionResult DanhSachNhaCungCap(int? page, string keyword)
        {
            int pageSize = 10;
            int pageNumber = page ?? 1;

            var query = _context.NhaCungCaps.AsQueryable();

            if (!string.IsNullOrEmpty(keyword))
            {
                query = query.Where(ncc =>
                    ncc.TenNcc.Contains(keyword) ||
                    ncc.Sdt.Contains(keyword) ||
                    ncc.Email.Contains(keyword)
                );

                ViewBag.TuKhoa = keyword;
            }

            var danhSach = query.OrderBy(ncc => ncc.TenNcc).ToPagedList(pageNumber, pageSize);
            return View(danhSach);
        }


        [HttpGet]
        public IActionResult ThemNhaCungCap()
        {
            return View(); 
        }

        [HttpPost]
        public IActionResult ThemNhaCungCap(NhaCungCap ncc)
        {
            var ten = ncc.TenNcc?.Trim().ToLower();
            var sdt = ncc.Sdt?.Trim();
            var email = ncc.Email?.Trim().ToLower();

            var existed = _context.NhaCungCaps.Any(x =>
                x.TenNcc!.Trim().ToLower() == ten &&
                x.Email!.Trim().ToLower() == email
            );

            if (existed)
            {
                ModelState.AddModelError(string.Empty, "Nhà cung cấp này đã tồn tại trong hệ thống.");
                return View(ncc); 
            }

            ncc.CreatedAt = DateTime.Now;
            _context.NhaCungCaps.Add(ncc);
            _context.SaveChanges();

            return RedirectToAction("DanhSachNhaCungCap");
        }


        [HttpPost]
        public IActionResult CapNhatNhaCungCap(NhaCungCap ncc)
        {
            var existing = _context.NhaCungCaps.Find(ncc.MaNcc);
            if (existing == null)
                return Json(new { success = false, message = "Không tìm thấy NCC" });

            existing.TenNcc = ncc.TenNcc;
            existing.Sdt = ncc.Sdt;
            existing.Email = ncc.Email;
            existing.TrangThai = ncc.TrangThai;
            existing.UpdatedAt = DateTime.Now;

            _context.SaveChanges();

            return Json(new { success = true });
        }

        [HttpPost]
        public IActionResult XoaNhaCungCap(int mancc)
        {
            var ncc = _context.NhaCungCaps.Find(mancc);
            if (ncc == null)
                return Json(new { success = false, message = "Không tìm thấy nhà cung cấp." });

            _context.NhaCungCaps.Remove(ncc);
            _context.SaveChanges();

            return Json(new { success = true });
        }

    }
}
