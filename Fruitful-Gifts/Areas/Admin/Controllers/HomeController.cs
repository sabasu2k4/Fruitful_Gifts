using Microsoft.AspNetCore.Mvc;

namespace Fruitful_Gifts.Areas.Admin.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
