using Fruitful_Gifts.Database;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Fruitful_Gifts.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class QuanLyDanhMucController : Controller
    {
        private readonly FruitfulGiftsContext _context;

        public QuanLyDanhMucController(FruitfulGiftsContext context)
        {
            _context = context;
        }
        // ------------------------- DANH MỤC GIỎ QUÀ -------------------------------- //
        public IActionResult DanhMucGioQua(int? danhMucChaId)
        {
            // Lấy tất cả danh mục cha
            ViewBag.DanhMucChaList = _context.DanhMucGioQuas
                .Where(dm => dm.DanhMucChaId == null)
                .ToList();

            // Nếu có chọn danh mục cha → lọc danh mục con
            var query = _context.DanhMucGioQuas
                .Include(dm => dm.DanhMucCha)
                .Where(dm => dm.DanhMucChaId != null);

            if (danhMucChaId.HasValue)
            {
                query = query.Where(dm => dm.DanhMucChaId == danhMucChaId.Value);
            }

            var danhMucConList = query.ToList();

            return View(danhMucConList);
        }

        [HttpGet]
        public IActionResult ThemDanhMucGioQua()
        {
            ViewBag.DanhMucChaList = _context.DanhMucGioQuas
                .Where(dm => dm.DanhMucChaId == null)
                .ToList();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ThemDanhMucGioQua(DanhMucGioQua model)
        {
            if (ModelState.IsValid)
            {
                // Xử lý upload ảnh
                if (model.ImageUpload != null && model.ImageUpload.Length > 0)
                {
                    var fileName = Path.GetFileName(model.ImageUpload.FileName);
                    var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/danhmuc", fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        model.ImageUpload.CopyTo(stream);
                    }

                    model.HinhAnh = "/images/danhmuc/" + fileName;
                }

                model.CreatedAt = DateTime.Now;
                _context.DanhMucGioQuas.Add(model);
                _context.SaveChanges();
                return RedirectToAction("DanhMucGioQua");
            }

            return View(model);
        }

        [HttpPost]
        public IActionResult XoaDanhMucGioQua(int maDm)
        {
            var danhMuc = _context.DanhMucGioQuas
                .Include(dm => dm.GioQuas)
                .Include(dm => dm.InverseDanhMucCha)
                .FirstOrDefault(dm => dm.MaDm == maDm);

            if (danhMuc == null)
            {
                return Json(new { success = false, message = "Không tìm thấy danh mục." });
            }

            if (danhMuc.GioQuas.Any())
            {
                return Json(new { success = false, message = "Không thể xóa vì danh mục đang chứa giỏ quà." });
            }

            if (danhMuc.InverseDanhMucCha.Any())
            {
                return Json(new { success = false, message = "Không thể xóa vì danh mục đang có danh mục con." });
            }

            _context.DanhMucGioQuas.Remove(danhMuc);
            _context.SaveChanges();

            return Json(new { success = true, message = "Xóa danh mục thành công." });
        }

        // --------------------------------- o0o ---------------------------------- //

        // ------------------------- DANH MỤC SẢN PHẨM -------------------------------- //
        public IActionResult DanhMucSanPham()
        {
            var danhMucList = _context.DanhMucSanPhams.ToList();
            return View(danhMucList);
        }

        [HttpGet]
        public IActionResult ThemDanhMucSanPham()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ThemDanhMucSanPham(DanhMucSanPham model)
        {
            if (ModelState.IsValid)
            {
                _context.DanhMucSanPhams.Add(model);
                _context.SaveChanges();
                return RedirectToAction("DanhMucSanPham");
            }
            return View(model);
        }

        [HttpPost]
        public IActionResult XoaDanhMucSanPham(int maLoai)
        {
            var danhMuc = _context.DanhMucSanPhams.Find(maLoai);
            if (danhMuc == null)
            {
                return Json(new { success = false, message = "Không tìm thấy danh mục." });
            }

            // Kiểm tra xem có sản phẩm nào thuộc danh mục này không
            bool coSanPham = _context.SanPhams.Any(sp => sp.MaLoai == maLoai);
            if (coSanPham)
            {
                return Json(new { success = false, message = "Danh mục này đang chứa sản phẩm, không thể xóa." });
            }

            _context.DanhMucSanPhams.Remove(danhMuc);
            _context.SaveChanges();

            return Json(new { success = true, message = "Đã xóa thành công." });
        }



        // --------------------------------- o0o ---------------------------------- //
    }
}
