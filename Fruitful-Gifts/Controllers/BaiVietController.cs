using Fruitful_Gifts.Database;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using X.PagedList.Extensions;

namespace Fruitful_Gifts.Controllers
{
    public class BaiVietController : Controller
    {
        private readonly FruitfulGiftsContext _context;

        public BaiVietController(FruitfulGiftsContext context)
        {
            _context = context;
        }
        public IActionResult Index(int ?page)
        {
            int pageNumber = page ?? 1;
            int pageSize = 6;

            var baiVietList = _context.BaiViets
          .Where(bv => bv.Slug != "gioi-thieu-ve-chung-toi" && bv.TrangThai == 1)
          .OrderByDescending(bv => bv.CreatedAt);


            var pagedList = baiVietList.ToPagedList(pageNumber, pageSize);

            return View(pagedList);
        }

        public IActionResult ChiTiet(int mabv)
        {
            var baiViet = _context.BaiViets.FirstOrDefault(x => x.MaBv == mabv);
            if (baiViet == null)
            {
                return NotFound();
            }

            return View(baiViet); 
        }

    }
}
