using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Fruitful_Gifts.Database;
using X.PagedList.Extensions;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Text;
using X.PagedList;

namespace Fruitful_Gifts.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class QuanLySanPhamController : Controller
    {
        private readonly FruitfulGiftsContext _context;

        public QuanLySanPhamController(FruitfulGiftsContext context)
        {
            _context = context;
        }

        //----------------------- GIỎ QUÀ ----------------------------//
        public IActionResult DanhSachGioQua(int? page, int? idDanhMuc)
        {
            int pageSize = 10;
            int pageNumber = page ?? 1;

            var query = _context.GioQuas
                .Include(gq => gq.MaDmNavigation)
                .AsQueryable();

            if (idDanhMuc.HasValue)
            {
                query = query.Where(gq => gq.MaDm == idDanhMuc.Value);
                ViewBag.SelectedCategory = idDanhMuc.Value;
            }

            var pagedGioQua = query
                .OrderBy(gq => gq.MaGq)
                .ToPagedList(pageNumber, pageSize);

            ViewBag.DanhMucList = _context.DanhMucGioQuas.ToList();

            return View(pagedGioQua);
        }

        //----------------------- SẢN PHẨM ----------------------------//
        public IActionResult DanhSachSanPham(int? page, int? idDanhMuc)
        {
            int pageSize = 10;
            int pageNumber = page ?? 1;

            var query = _context.SanPhams
                .Include(p => p.MaLoaiNavigation)
                .Include(p => p.MaNccNavigation)
                .Include(p => p.KhoHangs)
                .AsQueryable();

            if (idDanhMuc.HasValue && idDanhMuc > 0)
            {
                query = query.Where(p => p.MaLoai == idDanhMuc);
            }

            var pagedProducts = query
                .OrderBy(p => p.MaSp)
                .ToPagedList(pageNumber, pageSize);

            ViewBag.SelectedCategory = idDanhMuc;
            ViewBag.DanhMucList = _context.DanhMucSanPhams.ToList(); // Để render combobox

            return View(pagedProducts);
        }

        [HttpGet]
        public IActionResult ThemSanPham()
        {
            LoadDropdownData();
            ModelState.Clear();
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ThemSanPham(SanPham model, IFormFile? HinhAnhUpload)
        {
            model.Slug = GenerateSlug(model.TenSp);

            bool sanPhamTonTai = _context.SanPhams
                .Any(sp => sp.Slug == model.Slug || sp.TenSp == model.TenSp);

            if (sanPhamTonTai)
            {
                ModelState.AddModelError("", "Sản phẩm đã tồn tại.");
                LoadDropdownData();
                return View(model);
            }

            if (ModelState.IsValid)
            {
                if (HinhAnhUpload != null)
                {
                    string fileName = Path.GetFileName(HinhAnhUpload.FileName);
                    string path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/sanpham", fileName);

                    using (var stream = new FileStream(path, FileMode.Create))
                    {
                        await HinhAnhUpload.CopyToAsync(stream);
                    }

                    model.HinhAnh = fileName;
                }

                // Sinh slug từ TenSp nếu chưa có
                if (!string.IsNullOrEmpty(model.TenSp))
                {
                    model.Slug = GenerateSlug(model.TenSp);
                }

                model.CreatedAt = DateTime.Now;
                _context.SanPhams.Add(model);
                await _context.SaveChangesAsync();

                return RedirectToAction("DanhSachSanPham", "QuanLySanPham");
            }

            LoadDropdownData();
            return View(model);
        }

        private void LoadDropdownData()
        {
            ViewBag.DanhMucList = _context.DanhMucSanPhams.ToList();
            ViewBag.NccList = _context.NhaCungCaps.ToList();
            ViewBag.XuatXuList = _context.XuatXus.ToList();
            ViewBag.ChatLieuList = _context.ChatLieus.ToList();
            ViewBag.DonViTinhList = _context.DonViTinhs.ToList();
        }

        // Bỏ dấu tiếng Việt
        public static string RemoveDiacritics(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return string.Empty;

            var normalizedString = text.Normalize(NormalizationForm.FormD);
            var stringBuilder = new StringBuilder();

            foreach (var c in normalizedString)
            {
                var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
                if (unicodeCategory != UnicodeCategory.NonSpacingMark)
                {
                    stringBuilder.Append(c);
                }
            }

            return stringBuilder.ToString().Normalize(NormalizationForm.FormC);
        }

        public static string GenerateSlug(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return string.Empty;

            text = RemoveDiacritics(text).ToLowerInvariant();            // Bỏ dấu + thường hóa
            text = Regex.Replace(text, @"[^a-z0-9\s-]", "");             // Loại ký tự không hợp lệ
            text = Regex.Replace(text, @"\s+", "-").Trim('-');           // Thay khoảng trắng = - và xóa - dư
            text = Regex.Replace(text, @"-+", "-");                      // Gộp nhiều dấu - thành 1

            return text;
        }

    }
}
