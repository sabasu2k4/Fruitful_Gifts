using Fruitful_Gifts.Database;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.Scripting;
using System.Globalization;
using System.Text;
using System.Text.Json;
using Fruitful_Gifts.Models.ViewModels;
using X.PagedList;
using X.PagedList.Extensions;
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
            var sp = _context.SanPhams
                        .Where(sp => sp.MaDm == 1)
                        .ToList();
            var sp1 = _context.SanPhams
            .Where(sp => sp.MaDm == 2)
            .ToList();

            var sp2 = _context.SanPhams
            .Where(sp => sp.MaDm == 3)
            .ToList();

            var sp3 = _context.SanPhams
            .Where(sp => sp.MaDm == 4)
            .ToList();

            var dm = _context.DanhMucs.ToList();

            ViewBag.SanPham = sp;
            ViewBag.SanPham1 = sp1;
            ViewBag.SanPham2 = sp2;
            ViewBag.SanPham3 = sp3;
            ViewBag.DanhMuc = dm;

            return View();
        }

        public IActionResult TimKiemSanPham(string? TenTimKiem, int? DanhMucId, decimal? GiaMin, decimal? GiaMax, string? SortOrder, int page = 1, int pageSize = 8)
        {
            var userId = GetLoggedInKhachHangId();
            bool trangThaiDangNhap = userId != null;
            ViewBag.TrangThaiDangNhap = trangThaiDangNhap;

            // Lấy dữ liệu sản phẩm kèm khuyến mãi
            var query = from sp in _context.SanPhams
                        join km in _context.KhuyenMais on sp.MaSp equals km.MaSp into kmGroup
                        from km in kmGroup.DefaultIfEmpty()
                        where sp.TrangThai == true && sp.IsHienThi == true
                        select new
                        {
                            SanPham = sp,
                            GiaSauKhiGiam = (km != null &&
                                            km.NgayBatDau <= DateOnly.FromDateTime(DateTime.Now.Date) &&
                                            km.NgayKetThuc >= DateOnly.FromDateTime(DateTime.Now.Date))
                                            ? sp.Gia - km.MucGiamGia
                                            : sp.Gia
                        };

            // Lọc theo tên sản phẩm
            if (!string.IsNullOrEmpty(TenTimKiem))
            {
                var TenDaDuocMaHoa = MaHoaTenTimKiem(TenTimKiem.ToLower());
                query = query
                    .AsEnumerable()
                    .Where(sp => MaHoaTenTimKiem(sp.SanPham.TenSp?.ToLower() ?? "").Contains(TenDaDuocMaHoa))
                    .Select(sp => new { sp.SanPham, sp.GiaSauKhiGiam })
                    .AsQueryable();
            }

            // Lọc theo danh mục
            if (DanhMucId.HasValue && DanhMucId > 0)
            {
                query = query.Where(sp => sp.SanPham.MaDm == DanhMucId);
            }

            // Lọc theo khoảng giá
            if (GiaMin.HasValue)
            {
                query = query.Where(sp => sp.GiaSauKhiGiam >= GiaMin);
            }
            if (GiaMax.HasValue)
            {
                query = query.Where(sp => sp.GiaSauKhiGiam <= GiaMax);
            }

            // Sắp xếp
            if (SortOrder == "asc")
            {
                query = query.OrderBy(sp => sp.GiaSauKhiGiam);
            }
            else if (SortOrder == "desc")
            {
                query = query.OrderByDescending(sp => sp.GiaSauKhiGiam);
            }
            else
            {
                query = query.OrderBy(sp => sp.SanPham.TenSp);
            }

            // Tạo danh sách sản phẩm có trường GiáSauKhiGiam
            var sanPhamViewModels = query
            .Select(sp => new SanPhamViewModel
            {
                SanPham = sp.SanPham,
                GiaSauKhiGiam = sp.GiaSauKhiGiam ?? 0
            })
            .ToList() // ép về List để dùng ToPagedList
            .ToPagedList(page, pageSize);

            ViewBag.DanhMuc = _context.DanhMucs.ToList();
            ViewBag.CurrentSearchTerm = TenTimKiem;
            ViewBag.CurrentCategory = DanhMucId;
            ViewBag.CurrentPriceMin = GiaMin;
            ViewBag.CurrentPriceMax = GiaMax;
            ViewBag.CurrentSortOrder = SortOrder;

            ViewBag.SanPhamYeuThich = _context.SanPhamYeuThiches.ToList();

            return View(sanPhamViewModels);
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
            var khachHangJson = HttpContext.Session.GetString("user");

            if (string.IsNullOrEmpty(khachHangJson))
            {
                return null;
            }

            var thongTinkhachHang = JsonSerializer.Deserialize<KhachHang>(khachHangJson); //*

            if (thongTinkhachHang != null)
            {
                var customer = _context.KhachHangs.FirstOrDefault(kh => kh.MaKh == thongTinkhachHang.MaKh);

                if (customer != null)
                {
                    return customer.MaKh;
                }
            }


            return null;
        }



    }
}
