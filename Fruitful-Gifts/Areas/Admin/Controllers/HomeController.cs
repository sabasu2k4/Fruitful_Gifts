using Microsoft.AspNetCore.Mvc;

namespace Fruitful_Gifts.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View(); // Views/Admin/Home/Index.cshtml
        }
    }
}
