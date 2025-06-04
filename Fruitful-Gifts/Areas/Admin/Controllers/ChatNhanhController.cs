using Microsoft.AspNetCore.Mvc;

namespace Fruitful_Gifts.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Route("Admin/ChatNhanh")]
    public class ChatNhanhController : Controller
    {
        [HttpGet("")]
        public IActionResult Index()
        {
            return View();
        }
    }

}
