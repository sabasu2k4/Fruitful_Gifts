using Fruitful_Gifts.Database;
using Fruitful_Gifts.Helpers;
using Microsoft.AspNetCore.Mvc;
using X.PagedList.Extensions;

namespace Fruitful_Gifts.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class QuanLyBaiVietController : Controller
    {
        private readonly FruitfulGiftsContext _context;

        public QuanLyBaiVietController(FruitfulGiftsContext context)
        {
            _context = context;
        }
        public IActionResult DanhSachBaiViet()
        {
            var list = _context.BaiViets
                        .OrderByDescending(b => b.NgayDang)
                        .ToList();
            return View(list);
        }
        [HttpGet]
        public IActionResult ThemBaiViet()
        {
            return View();
        }

        [HttpPost]
        public IActionResult ThemBaiViet(BaiViet model, IFormFile? HinhAnhUpload)
        {
            model.Slug = SlugHelper.GenerateSlug(model.TieuDe);
            if (ModelState.IsValid)
            {
                if (HinhAnhUpload != null)
                {
                    var fileName = Path.GetFileName(HinhAnhUpload.FileName);
                    var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/baiviet", fileName);
                    using (var stream = new FileStream(path, FileMode.Create))
                    {
                        HinhAnhUpload.CopyTo(stream);
                    }
                    model.HinhAnh = fileName;
                }

                model.NgayDang = DateOnly.FromDateTime(DateTime.Now);
                model.CreatedAt = DateTime.Now;
                _context.BaiViets.Add(model);
                _context.SaveChanges();
                return RedirectToAction("DanhSachBaiViet");
            }

            return View(model);
        }

        [HttpDelete]
        public IActionResult XoaBaiViet(int id)
        {
            var baiViet = _context.BaiViets.Find(id);
            if (baiViet == null)
                return Json(new { success = false });

            // Xoá ảnh nếu có
            if (!string.IsNullOrEmpty(baiViet.HinhAnh))
            {
                var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/baiviet", baiViet.HinhAnh);
                if (System.IO.File.Exists(path))
                {
                    System.IO.File.Delete(path);
                }
            }

            _context.BaiViets.Remove(baiViet);
            _context.SaveChanges();

            return Json(new { success = true });
        }


        [HttpPost]
        public IActionResult Edit(BaiViet model, IFormFile? HinhAnhUpload)
        {
            var baiViet = _context.BaiViets.Find(model.MaBv);
            if (baiViet == null) return NotFound();

            baiViet.TieuDe = model.TieuDe;
            baiViet.NoiDung = model.NoiDung;
            baiViet.TrangThai = model.TrangThai;
            baiViet.UpdatedAt = DateTime.Now;

            if (HinhAnhUpload != null)
            {
                // Xóa ảnh cũ nếu có
                if (!string.IsNullOrEmpty(baiViet.HinhAnh))
                {
                    var oldImagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/baiviet", baiViet.HinhAnh);
                    if (System.IO.File.Exists(oldImagePath))
                    {
                        System.IO.File.Delete(oldImagePath);
                    }
                }

                // Lưu ảnh mới
                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(HinhAnhUpload.FileName);
                var newImagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/baiviet", fileName);
                using (var stream = new FileStream(newImagePath, FileMode.Create))
                {
                    HinhAnhUpload.CopyTo(stream);
                }

                baiViet.HinhAnh = fileName;
            }

            _context.SaveChanges();
            return Ok();
        }

    }
}
