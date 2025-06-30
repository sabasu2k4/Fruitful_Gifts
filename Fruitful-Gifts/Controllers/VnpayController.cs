using Fruitful_Gifts.Database;
using Fruitful_Gifts.Libraries;
using Fruitful_Gifts.Models.Vnpay;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
namespace Fruitful_Gifts.Controllers
{
    public class PaymentController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly FruitfulGiftsContext _context;

        public PaymentController(IConfiguration configuration, FruitfulGiftsContext context)
        {
            _configuration = configuration;
            _context = context;
        }

        // Sửa phương thức lấy khách hàng theo tên đăng nhập hoặc email (giữ nguyên, không đổi)
        private KhachHang GetCustomerByUserName(string userName)
        {
            return _context.TaiKhoans
                .Include(tk => tk.KhachHang)
                .Where(tk => tk.TenDangNhap == userName || (tk.KhachHang != null && tk.KhachHang.Email == userName))
                .Select(tk => tk.KhachHang)
                .FirstOrDefault();
        }

        [HttpGet]
        public IActionResult Checkout()
        {
            int? maKh = HttpContext.Session.GetInt32("MaKh");
            if (maKh == null) return RedirectToAction("DangNhap", "TaiKhoan");

            // Force refresh dữ liệu từ database
            var danhSach = _context.ChiTietGioHangs
                .Include(x => x.MaSpNavigation)
                .Include(x => x.MaGqNavigation)
                .Where(x => x.MaKh == maKh && x.TrangThai == 1)
                .AsNoTracking() // Đảm bảo lấy dữ liệu mới nhất
                .ToList();

            if (!danhSach.Any())
            {
                TempData["ErrorMessage"] = "Vui lòng chọn ít nhất một sản phẩm để thanh toán";
                return RedirectToAction("Index", "GioHang");
            }

            // Tính toán lại Amount
            decimal tongTien = danhSach.Sum(x =>
                (x.SoLuong ?? 0) * (x.MaGqNavigation?.GiaBan ?? x.MaSpNavigation?.GiaBan ?? 0m)
            );
            decimal phiVanChuyen = tongTien < 500000 ? 30000 : 0;

            var model = new PaymentInformationModel
            {
                TienHang = tongTien,
                Amount = tongTien + phiVanChuyen,
                PhiVanChuyenBanHang = phiVanChuyen,
                // 
            };

           
            string userName = HttpContext.Session.GetString("UserName");
            if (!string.IsNullOrEmpty(userName))
            {
                var khachHang = GetCustomerByUserName(userName);
                if (khachHang != null)
                {
                    model.Name = khachHang.TenKh;
                    model.Email = khachHang.Email;
                    model.DiaChi = khachHang.DiaChi;
                    model.Sdt = khachHang.Sdt;
                    model.OrderType = "donhang";
                }
            }

            return View(model);
        }


        // POST: Xử lý thanh toán 
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Checkout(PaymentInformationModel model, string PaymentMethod)
        {
            int? maKh = HttpContext.Session.GetInt32("MaKh");
            if (maKh == null)
            {
                return RedirectToAction("DangNhap", "TaiKhoan");
            }

            // ❗ Kiểm tra lại giỏ hàng với trạng thái đã chọn (TrangThai == 1)
            var danhSach = await _context.ChiTietGioHangs
                .Include(x => x.MaSpNavigation)
                .Include(x => x.MaGqNavigation)
                .Where(c => c.MaKh == maKh && c.TrangThai == 1)
                .ToListAsync();

            if (!danhSach.Any()) // Thay đổi điều kiện kiểm tra
            {
                TempData["ErrorMessage"] = "Bạn chưa chọn sản phẩm nào để thanh toán";
                return RedirectToAction("Index", "GioHang"); 
            }

          
            decimal tongTien = danhSach.Sum(x =>
                (x.SoLuong ?? 0) * (
                    x.MaGqNavigation?.GiaBan ?? x.MaSpNavigation?.GiaBan ?? 0
                )
            );

          
            decimal phiVanChuyen = tongTien < 500000 ? 30000 : 0;

            model.TienHang = tongTien;
            model.Amount = tongTien + phiVanChuyen;
            model.PhiVanChuyenBanHang = phiVanChuyen;
            model.PhuongThucBan = "online";
           

            if (string.IsNullOrWhiteSpace(model.OrderDescription))
            {
                model.OrderDescription = "Không có ghi chú.";
            }

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                System.Diagnostics.Debug.WriteLine("ModelState Invalid:");
                errors.ForEach(e => System.Diagnostics.Debug.WriteLine($" - {e}"));

                TempData["ErrorMessage"] = "Thông tin thanh toán không hợp lệ.";
                return View(model);
            }


            try
            {
                
                if (PaymentMethod == "cash")
                {
                    var maDonHang = await TaoDonHangSauThanhToan(model, 0, "cash");
                    if (maDonHang == null)
                    {
                        System.Diagnostics.Debug.WriteLine("❌ Không tạo được đơn hàng");
                        TempData["ErrorMessage"] = "Không tạo được đơn hàng";
                        return RedirectToAction("Index", "GioHang");
                    }



                    _context.ChiTietGioHangs.RemoveRange(danhSach);
                    await _context.SaveChangesAsync();

                    return View("~/Views/Payment/CashSuccess.cshtml", model);
                }
                else if (PaymentMethod == "vnpay")
                {
                    model.OrderDescription = $"Thanh toán đơn hàng ngày {DateTime.Now:dd/MM/yyyy}";
                    HttpContext.Session.SetString("OrderInfo", JsonConvert.SerializeObject(model));

                    var paymentUrl = CreatePaymentUrlVnpay(model, HttpContext);
                    if (string.IsNullOrEmpty(paymentUrl))
                    {
                        TempData["ErrorMessage"] = "Có lỗi khi khởi tạo thanh toán VNPay";
                        return RedirectToAction("Index", "GioHang");
                    }

                    return Redirect(paymentUrl);
                }

                TempData["ErrorMessage"] = "Phương thức thanh toán không hợp lệ";
                return RedirectToAction("Index", "GioHang");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("❌ Exception khi thanh toán: " + ex.Message);
                TempData["ErrorMessage"] = "Lỗi: " + ex.Message;
                return RedirectToAction("Index", "GioHang");
            }

        }




        // POST: Tạo đơn hàng tùy theo phương thức thanh toán
        [HttpPost]
        public IActionResult CreatePaymentUrl(PaymentInformationModel model, string PaymentMethod)
        {
            if (PaymentMethod == "cash")
            {
                // Xử lý đơn hàng tiền mặt
                return View("CashSuccess", model);
            }
            else if (PaymentMethod == "vnpay")
            {
                var paymentUrl = CreatePaymentUrlVnpay(model, HttpContext);
                return Redirect(paymentUrl);
            }

            return View("Fail");
        }

        // GET: Xử lý callback từ VNPay
        [HttpGet]
        public async Task<IActionResult> PaymentCallbackVnpay()
        {
            var response = PaymentExecute(Request.Query);

            if (response.Success)
            {
                string userName = HttpContext.Session.GetString("UserName");
                var khachHang = GetCustomerByUserName(userName);

                if (khachHang == null)
                {
                    // Log lỗi hoặc xử lý khi không tìm thấy khách hàng
                    // Ví dụ: _logger.LogError("Không tìm thấy khách hàng với UserName: {userName}", userName);
                    return View("Fail", response);
                }

                var orderInfoJson = HttpContext.Session.GetString("OrderInfo");
                var model = string.IsNullOrEmpty(orderInfoJson)
                    ? new PaymentInformationModel()
                    : JsonConvert.DeserializeObject<PaymentInformationModel>(orderInfoJson);

                model ??= new PaymentInformationModel();

                model.Name ??= khachHang.TenKh;
                model.Email ??= khachHang.Email;
                model.DiaChi ??= khachHang.DiaChi;
                model.Sdt ??= khachHang.Sdt;
                model.Amount = response.Amount;
               
                model.OrderDescription ??= "Thanh toán qua VNPay";

                // Tạo đơn hàng
                var maDh = await TaoDonHangSauThanhToan(model, 1, "vnpay");

                if (maDh.HasValue)
                {
                    // Lấy mã phương thức thanh toán tương ứng với VNPAY
                    var maPtEntity = await _context.PhuongThucThanhToans
                        .FirstOrDefaultAsync(pt => pt.TenPt == "Thanh toán qua ví điện tử");

                    if (maPtEntity != null)
                    {
                        var thanhToan = new ThanhToan
                        {
                            MaDh = maDh.Value,
                            MaPt = maPtEntity.MaPt,
                            TransactionCode = response.TransactionCode,
                            BankCode = response.BankCode,
                            ResponseCode = response.ResponseCode,
                            Amount = response.Amount,
                            PaymentStatus = response.ResponseCode == "00" ? "Success" : "Failed",
                            PaymentTime = DateTime.Now,
                            CreatedAt = DateTime.Now,
                            UpdatedAt = DateTime.Now
                        };

                        _context.ThanhToans.Add(thanhToan);
                        await _context.SaveChangesAsync();
                    }
                    else
                    {
                        // Log lỗi khi không tìm thấy phương thức thanh toán
                        // Ví dụ: _logger.LogError("Không tìm thấy phương thức thanh toán 'Thanh toán qua ví điện tử'");
                        // Bạn cũng có thể trả về view lỗi nếu muốn
                        return View("Fail", new { Message = "Phương thức thanh toán không hợp lệ" });
                    }
                }
                else
                {
                    // Log lỗi hoặc xử lý khi tạo đơn hàng không thành công
                    // Ví dụ: _logger.LogError("Tạo đơn hàng thất bại khi callback VNPAY");
                    return View("Fail", new { Message = "Tạo đơn hàng không thành công" });
                }

                return View("Success", response);
            }

            return View("Fail", response);
        }




        // Tạo URL thanh toán VNPay
        private string CreatePaymentUrlVnpay(PaymentInformationModel model, HttpContext context)
        {
            var timeZoneById = TimeZoneInfo.FindSystemTimeZoneById(_configuration["TimeZoneId"]);
            var timeNow = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, timeZoneById);
            var tick = DateTime.Now.Ticks.ToString();

            var pay = new VnpayLibrary();
            var urlCallBack = _configuration["Vnpay:PaymentBackReturnUrl"];

            pay.AddRequestData("vnp_Version", _configuration["Vnpay:Version"]);
            pay.AddRequestData("vnp_Command", _configuration["Vnpay:Command"]);
            pay.AddRequestData("vnp_TmnCode", _configuration["Vnpay:TmnCode"]);
            pay.AddRequestData("vnp_Amount", ((int)model.Amount * 100).ToString());
            pay.AddRequestData("vnp_CreateDate", timeNow.ToString("yyyyMMddHHmmss"));
            pay.AddRequestData("vnp_CurrCode", _configuration["Vnpay:CurrCode"]);
            pay.AddRequestData("vnp_IpAddr", pay.GetIpAddress(context));
            pay.AddRequestData("vnp_Locale", _configuration["Vnpay:Locale"]);
            pay.AddRequestData("vnp_OrderInfo", $"{model.Name} {model.OrderDescription} {model.Amount}");
            pay.AddRequestData("vnp_OrderType", model.OrderType);
            pay.AddRequestData("vnp_ReturnUrl", urlCallBack);
            pay.AddRequestData("vnp_TxnRef", tick);

            return pay.CreateRequestUrl(_configuration["Vnpay:BaseUrl"], _configuration["Vnpay:HashSecret"]);
        }

        // Xử lý callback trả về từ VNPay
        private PaymentResponseModel PaymentExecute(IQueryCollection collections)
        {
            var pay = new VnpayLibrary();
            return pay.GetFullResponseData(collections, _configuration["Vnpay:HashSecret"]);
        }

        private async Task<int?> TaoDonHangSauThanhToan(PaymentInformationModel model, int trangThaiThanhToan, string paymentMethod)
        {
            string userName = HttpContext.Session.GetString("UserName");
            var khachHang = GetCustomerByUserName(userName);

            if (khachHang == null)
                return null;

            // ❗ Lấy các sản phẩm đã được chọn trong giỏ hàng (TrangThai == 1)
            var gioHang = await _context.ChiTietGioHangs
                .Where(c => c.MaKh == khachHang.MaKh && c.TrangThai == 1)
                .ToListAsync();

            if (gioHang == null || !gioHang.Any())
                return null;

            // ====== KIỂM TRA TỒN KHO TRƯỚC =======
            foreach (var item in gioHang)
            {
                if (item.MaSp.HasValue)
                {
                    var tongTon = await _context.KhoHangs
                        .Where(k => k.MaSp == item.MaSp.Value)
                        .SumAsync(k => k.SoLuongTon);

                    if (tongTon < item.SoLuong)
                        return null; // Không đủ hàng
                }
                else if (item.MaGq.HasValue)
                {
                    var chiTietGioQua = await _context.ChiTietGioQuas
                        .Where(ct => ct.MaGq == item.MaGq.Value)
                        .Include(ct => ct.MaSpNavigation)
                        .ThenInclude(sp => sp.KhoHangs)
                        .ToListAsync();

                    foreach (var ct in chiTietGioQua)
                    {
                        var tongTon = ct.MaSpNavigation.KhoHangs.Sum(k => k.SoLuongTon);
                        var soLuongCan = ct.SoLuong * item.SoLuong;

                        if (tongTon < soLuongCan)
                            return null; // Không đủ hàng để tạo giỏ quà
                    }
                }
            }

            // ===== LẤY DANH SÁCH NHÂN VIÊN HIỆN CÓ =====
            var danhSachNhanVien = await _context.NhanViens
                .OrderBy(nv => nv.MaNv)
                .Select(nv => nv.MaNv)
                .ToListAsync();

            int? maNvGan = null;
            if (danhSachNhanVien.Any())
            {
                int soLuongDonHang = await _context.DonHangs.CountAsync();
                int index = soLuongDonHang % danhSachNhanVien.Count;
                maNvGan = danhSachNhanVien[index];
            }

            // ====== TẠO ĐƠN HÀNG ======
            decimal tongTien = 0;

            var donHang = new DonHang
            {
                MaKh = khachHang.MaKh,
                NgayDatHang = DateTime.Now,
                DiaChiNhanHang = model.DiaChi,
                SoDienThoai = model.Sdt,
                GhiChu = model.OrderDescription,
                MaPt = paymentMethod == "cash" ? 3 : (paymentMethod == "vnpay" ? 2 : 1),
                TrangThai = 1,
                TrangThaiThanhToan = trangThaiThanhToan,
                PhuongThucBan = model.PhuongThucBan,
                MaNv = maNvGan
            };

            _context.DonHangs.Add(donHang);
            await _context.SaveChangesAsync();

            foreach (var item in gioHang)
            {
                decimal giaBan = 0;

                if (item.MaSp.HasValue)
                {
                    var sanPham = await _context.SanPhams.FirstOrDefaultAsync(sp => sp.MaSp == item.MaSp.Value);
                    giaBan = sanPham?.GiaBan ?? 0;

                    // Trừ kho
                    var khoList = await _context.KhoHangs
                        .Where(k => k.MaSp == sanPham.MaSp)
                        .OrderBy(k => k.CreatedAt)
                        .ToListAsync();

                    int soLuongCanTru = item.SoLuong ?? 0;
                    foreach (var kho in khoList)
                    {
                        if (soLuongCanTru <= 0) break;

                        int tru = Math.Min(kho.SoLuongTon, soLuongCanTru);
                        kho.SoLuongTon -= tru;
                        soLuongCanTru -= tru;
                    }
                }
                else if (item.MaGq.HasValue)
                {
                    var gioQua = await _context.GioQuas.FirstOrDefaultAsync(gq => gq.MaGq == item.MaGq.Value);
                    giaBan = gioQua?.GiaBan ?? 0;

                    var chiTietGioQua = await _context.ChiTietGioQuas
                        .Where(ct => ct.MaGq == gioQua.MaGq)
                        .Include(ct => ct.MaSpNavigation)
                        .ThenInclude(sp => sp.KhoHangs)
                        .ToListAsync();

                    foreach (var ct in chiTietGioQua)
                    {
                        int soLuongCanTru = (int)(ct.SoLuong * (item.SoLuong ?? 0));
                        var khoList = ct.MaSpNavigation.KhoHangs
                            .OrderBy(k => k.CreatedAt)
                            .ToList();

                        foreach (var kho in khoList)
                        {
                            if (soLuongCanTru <= 0) break;

                            int tru = Math.Min(kho.SoLuongTon, soLuongCanTru);
                            kho.SoLuongTon -= tru;
                            soLuongCanTru -= tru;
                        }
                    }
                }

                var tongTienSanPham = giaBan * item.SoLuong;
                tongTien += tongTienSanPham ?? 0m;

                var chiTiet = new ChiTietDonHang
                {
                    MaDh = donHang.MaDh,
                    MaSp = item.MaSp,
                    MaGq = item.MaGq,
                    SoLuong = item.SoLuong,
                    GiaBan = giaBan,
                    TongTienTungSanPham = tongTienSanPham
                };

                _context.ChiTietDonHangs.Add(chiTiet);
            }

            decimal phiVanChuyen = 0;
            if (tongTien < 500000)
            {
                phiVanChuyen = 30000;
            }

            donHang.PhiVanChuyenBanHang = phiVanChuyen;
            donHang.TongTienDonHang = tongTien + phiVanChuyen;

            await _context.SaveChangesAsync();

            // ❗ Xoá các sản phẩm đã được chọn (TrangThai == 1)
            _context.ChiTietGioHangs.RemoveRange(gioHang);
            await _context.SaveChangesAsync();

            return donHang.MaDh;
        }

    }
}
public class CartItem
{
    public int? MaSp { get; set; }
    public int? MaGq { get; set; }
    public int SoLuong { get; set; }
    public decimal GiaBan { get; set; }
}
