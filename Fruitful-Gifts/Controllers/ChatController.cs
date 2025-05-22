using Fruitful_Gifts.Database;
using Microsoft.AspNetCore.Mvc;

namespace Fruitful_Gifts.Controllers
{
    public class ChatController : Controller
    {
        private readonly FruitfulGiftsContext _context;

        public ChatController(FruitfulGiftsContext context)
        {
            _context = context;
        }

        [HttpPost]
        public JsonResult NhanTinNhan([FromBody] LienHe model)
        {
            if (!string.IsNullOrEmpty(model.NoiDung))
            {
                var lienHe = new LienHe
                {
                    HoTen = "Khách vãng lai",
                    Email = null,
                    Sdt = null,
                    NoiDung = model.NoiDung,
                    ThoiGianGui = DateTime.Now,
                    TrangThai = false
                };

                _context.LienHes.Add(lienHe);
                _context.SaveChanges();

                // Gửi phản hồi giả lập
                return Json(new { reply = "Cảm ơn bạn đã liên hệ. Chúng tôi sẽ phản hồi sớm!" });
            }

            return Json(new { reply = "" });
        }
    }
}
