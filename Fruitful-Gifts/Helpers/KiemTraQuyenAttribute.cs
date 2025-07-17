using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;

namespace Fruitful_Gifts.Helpers
{
    public class KiemTraQuyenAttribute : ActionFilterAttribute
    {
        private readonly string[] _quyenChoPhep;

        public KiemTraQuyenAttribute(params string[] quyenChoPhep)
        {
            _quyenChoPhep = quyenChoPhep;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var httpContext = context.HttpContext;
            var quyenHienTai = httpContext.Session.GetString("Quyen");
            var laAdmin = httpContext.Session.GetString("VaiTro") == "Admin";

            if (!laAdmin && !_quyenChoPhep.Contains(quyenHienTai))
            {
                context.Result = new RedirectToActionResult("KhongCoQuyen", "Home", null);
            }

            base.OnActionExecuting(context);
        }
    }
}
