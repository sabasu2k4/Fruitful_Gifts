using Fruitful_Gifts.Database;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;
using System.Text;
using System.Text.Json;
using Fruitful_Gifts.Models.ViewModels;
using X.PagedList.Extensions;
using Microsoft.EntityFrameworkCore;
namespace Fruitful_Gifts.Controllers
{
    public class TrangChuController : Controller
    {

        private readonly FruitfulGiftsContext _context;

        public TrangChuController(FruitfulGiftsContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var san_pham_hotdeal = _context.GioQuas
            .Include(g => g.MaDmNavigation)
            .OrderBy(g => g.MaDmNavigation.TenDm)
            .ThenBy(g => g.TenGioQua)
            .ToList();

            var qua_dip_le = _context.GioQuas
                .Include(gq => gq.MaDmNavigation)
                .Where(gq => gq.MaDmNavigation != null && gq.MaDmNavigation.DanhMucChaId == 1)
                .ToList();

            var qua_gia_dinh_ca_nha = _context.GioQuas
            .Include(gq => gq.MaDmNavigation)
            .Where(gq => gq.MaDmNavigation != null && gq.MaDmNavigation.DanhMucChaId == 2)
            .ToList();

            var qua_thuong_mai = _context.GioQuas
            .Include(gq => gq.MaDmNavigation)
            .Where(gq => gq.MaDmNavigation != null && gq.MaDmNavigation.DanhMucChaId == 3)
            .ToList();

            var san_pham_khac = _context.SanPhams
            .ToList();

            var baiVietList = _context.BaiViets
                .Where(bv => bv.Slug != "gioi-thieu-ve-chung-toi")
                .OrderByDescending(bv => bv.CreatedAt);

            ViewBag.TinTuc = baiVietList.Take(8).ToList();

            var danhMucCons = _context.DanhMucGioQuas
                .Where(dm => dm.DanhMucChaId != null)
                .ToList();

            var dichVuCongTyList = _context.DichVuCongTies
            .Where(dv => dv.TrangThai == 1)
            .OrderByDescending(dv => dv.CreatedAt)
            .ToList();

            ViewBag.SanPham = san_pham_hotdeal;
            ViewBag.SanPham1 = qua_dip_le;
            ViewBag.SanPham2 = qua_gia_dinh_ca_nha;
            ViewBag.SanPham3 = qua_thuong_mai;
            ViewBag.SanPham4 = san_pham_khac;
            ViewBag.DanhMuc = danhMucCons;
            ViewBag.DichVuCongTy = dichVuCongTyList;

            return View();
        }

        public IActionResult TimKiem(string? TuKhoa, int? DanhMucId, decimal? GiaMin, decimal? GiaMax, string? SortOrder, int page = 1, int pageSize = 6)
        {
            var userId = GetLoggedInKhachHangId();
            bool daDangNhap = userId != null;

            var resultList = new List<SanPhamViewModel>();

            var today = DateOnly.FromDateTime(DateTime.Now.Date);

            // ======== Tìm kiếm Sản phẩm ========
            var querySP = from sp in _context.SanPhams
                          join km in _context.KhuyenMais on sp.MaSp equals km.MaSp into kmGroup
                          from km in kmGroup.DefaultIfEmpty()
                          where sp.TrangThai == 1
                          select new
                          {
                              SanPham = sp,
                              GiaSauGiam = (km != null && km.NgayBatDau <= today && km.NgayKetThuc >= today)
                                  ? sp.GiaBan - km.MucGiamGia
                                  : sp.GiaBan
                          };

            if (!string.IsNullOrEmpty(TuKhoa))
            {
                var keyword = MaHoaTenTimKiem(TuKhoa.ToLower());
                querySP = querySP
                    .AsEnumerable()
                    .Where(sp => MaHoaTenTimKiem(sp.SanPham.TenSp?.ToLower() ?? "").Contains(keyword))
                    .AsQueryable();
            }

            if (DanhMucId.HasValue)
                querySP = querySP.Where(sp => sp.SanPham.MaLoai == DanhMucId);

            if (GiaMin.HasValue)
                querySP = querySP.Where(sp => sp.GiaSauGiam >= GiaMin);
            if (GiaMax.HasValue)
                querySP = querySP.Where(sp => sp.GiaSauGiam <= GiaMax);

            var sanPhamList = querySP.ToList();
            foreach (var item in sanPhamList)
            {
                resultList.Add(new SanPhamViewModel
                {
                    SanPham = item.SanPham,
                    GiaSauKhiGiam = item.GiaSauGiam ?? 0,
                    TrangThaiDangNhap = daDangNhap,
                    Loai = "sanpham"
                });
            }

            // ======== Tìm kiếm Giỏ Quà ========
            var queryGQ = from gq in _context.GioQuas
                          join km in _context.KhuyenMais on gq.MaGq equals km.MaGq into kmGroup
                          from km in kmGroup.DefaultIfEmpty()
                          where gq.TrangThai == 1
                          select new
                          {
                              GioQua = gq,
                              GiaSauGiam = (km != null && km.NgayBatDau <= today && km.NgayKetThuc >= today)
                                  ? gq.GiaBan - km.MucGiamGia
                                  : gq.GiaBan
                          };

            if (!string.IsNullOrEmpty(TuKhoa))
            {
                var keyword = MaHoaTenTimKiem(TuKhoa.ToLower());
                queryGQ = queryGQ
                    .AsEnumerable()
                    .Where(gq => MaHoaTenTimKiem(gq.GioQua.TenGioQua?.ToLower() ?? "").Contains(keyword))
                    .AsQueryable();
            }

            if (DanhMucId.HasValue)
                queryGQ = queryGQ.Where(gq => gq.GioQua.MaDm == DanhMucId);

            if (GiaMin.HasValue)
                queryGQ = queryGQ.Where(gq => gq.GiaSauGiam >= GiaMin);
            if (GiaMax.HasValue)
                queryGQ = queryGQ.Where(gq => gq.GiaSauGiam <= GiaMax);

            var gioQuaList = queryGQ.ToList();
            foreach (var item in gioQuaList)
            {
                resultList.Add(new SanPhamViewModel
                {
                    GioQua = item.GioQua,
                    GiaSauKhiGiam = item.GiaSauGiam ?? 0,
                    TrangThaiDangNhap = daDangNhap,
                    Loai = "gioqua"
                });
            }

            // Sắp xếp nếu có
            if (SortOrder == "asc")
                resultList = resultList.OrderBy(x => x.GiaSauKhiGiam).ToList();
            else if (SortOrder == "desc")
                resultList = resultList.OrderByDescending(x => x.GiaSauKhiGiam).ToList();

            var pagedList = resultList.ToPagedList(page, pageSize);
            // Lấy danh mục sản phẩm
            var danhMucList = _context.DanhMucSanPhams.ToList();
            // ViewBag để giữ lại trạng thái tìm kiếm

            // Gán danh mục cho ViewBag
            ViewBag.DanhMuc = danhMucList;
            ViewBag.CurrentSearchTerm = TuKhoa;
            ViewBag.CurrentCategory = DanhMucId;
            ViewBag.CurrentPriceMin = GiaMin;
            ViewBag.CurrentPriceMax = GiaMax;
            ViewBag.CurrentSortOrder = SortOrder;

            return View(pagedList); // nếu bạn đang có file tên như vậy

        }

        public static string MaHoaTenTimKiem(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return text;

            var normalizedString = text.Normalize(NormalizationForm.FormD);
            var stringBuilder = new System.Text.StringBuilder();

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

        public int? GetLoggedInKhachHangId()
        {
            // Lấy TaiKhoanId đã lưu khi đăng nhập
            int? taiKhoanId = HttpContext.Session.GetInt32("TaiKhoanId");

            if (taiKhoanId == null)
            {
                return null;
            }

            // Tìm KhachHang tương ứng với TaiKhoanId
            var khachHang = _context.KhachHangs.FirstOrDefault(kh => kh.TaiKhoanId == taiKhoanId);

            if (khachHang != null)
            {
                return khachHang.MaKh;
            }

            return null;
        }
    }
}
