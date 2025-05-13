using Fruitful_Gifts.Database;
using Fruitful_Gifts.Models;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace Fruitful_Gifts.Controllers
{
    public class GioHangController : Controller
    {
        private readonly FruitfulGiftsContext _db;

        public GioHangController(FruitfulGiftsContext db)
        {
            _db = db;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public IActionResult ThemVaoGioHang(int maSP, int soLuong)
        {
            // Kiểm tra đăng nhập
            var KH_JSON = HttpContext.Session.GetString("user");
            if (string.IsNullOrEmpty(KH_JSON))
            {
                return StatusCode(401, new { success = false, message = "Bạn cần đăng nhập!" });
            }

            var thongTinkhachHang = JsonSerializer.Deserialize<KhachHang>(KH_JSON);
            if (thongTinkhachHang?.MaKh == null)
            {
                return BadRequest(new { success = false, message = "Không thể xác định khách hàng." });
            }

            var maKH = thongTinkhachHang.MaKh;

            // Lấy thông tin sản phẩm
            var sanPham = _db.SanPhams
                .FirstOrDefault(sp => sp.MaSp == maSP && sp.IsHienThi == true && sp.TrangThai == true);

            if (sanPham == null)
            {
                return NotFound(new { success = false, message = "Sản phẩm không tồn tại hoặc đã bị ẩn." });
            }

            if (soLuong <= 0)
            {
                return BadRequest(new { success = false, message = "Số lượng không hợp lệ." });
            }

            if (soLuong > sanPham.SoLuong)
            {
                return BadRequest(new { success = false, message = "Sản phẩm không đủ tồn kho." });
            }

            // Kiểm tra sản phẩm đã có trong giỏ hàng chưa
            var gioHangItem = _db.ChiTietGioHangs
                .FirstOrDefault(x => x.MaKh == maKH && x.MaSp == maSP);

            if (gioHangItem != null)
            {
                gioHangItem.SoLuong += soLuong;

                if (gioHangItem.SoLuong > sanPham.SoLuong)
                {
                    return BadRequest(new { success = false, message = "Tổng số lượng vượt tồn kho!" });
                }

                _db.ChiTietGioHangs.Update(gioHangItem);
            }
            else
            {
                var gioHangMoi = new ChiTietGioHang
                {
                    MaKh = maKH,
                    MaSp = maSP,
                    SoLuong = soLuong,
                    CreatedAt = DateTime.Now
                };

                _db.ChiTietGioHangs.Add(gioHangMoi);
            }

            _db.SaveChanges();

            return Ok(new { success = true, message = "Đã thêm vào giỏ hàng!" });
        }
    }
}
