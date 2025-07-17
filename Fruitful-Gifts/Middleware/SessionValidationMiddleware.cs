using Fruitful_Gifts.Database;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

public class SessionValidationMiddleware
{
    private readonly RequestDelegate _next;

    public SessionValidationMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, FruitfulGiftsContext _context)
    {
        var path = context.Request.Path.ToString().ToLower();

        // Bỏ qua các route không cần kiểm tra
        if (path.Contains("/taikhoan/dangnhap") || path.Contains("/taikhoan/dangxuat")
            || path.Contains("/css") || path.Contains("/js") || path.Contains("/lib") || path.Contains("/images"))
        {
            await _next(context);
            return;
        }

        var userId = context.Session.GetInt32("TaiKhoanId");
        var sessionId = context.Session.GetString("SessionId");

        if (userId.HasValue && !string.IsNullOrEmpty(sessionId))
        {
            var taiKhoan = _context.TaiKhoans.FirstOrDefault(tk => tk.TaiKhoanId == userId);
            if (taiKhoan == null || string.IsNullOrEmpty(taiKhoan.SessionId) || taiKhoan.SessionId != sessionId)
            {
                var tempDataFactory = context.RequestServices.GetService<ITempDataDictionaryFactory>();
                var tempData = tempDataFactory.GetTempData(context);
                tempData["LoggedOutReason"] = "Tài khoản của bạn đã được đăng nhập từ thiết bị khác.";
                context.Session.Clear();
                context.Response.Redirect("/TaiKhoan/DangNhap");
                return;
            }
        }

        await _next(context);
    }

}
