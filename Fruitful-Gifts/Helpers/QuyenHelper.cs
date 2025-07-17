namespace Fruitful_Gifts.Helpers
{
    public static class QuyenHelper
    {
        public static bool CoQuyen(this HttpContext context, params string[] quyenYeuCau)
        {
            var quyenHienTai = context.Session.GetString("Quyen");
            return quyenYeuCau.Contains(quyenHienTai);
        }

        public static bool LaAdmin(this HttpContext context)
        {
            return context.Session.GetString("VaiTro") == "Admin";
        }
    }
}
