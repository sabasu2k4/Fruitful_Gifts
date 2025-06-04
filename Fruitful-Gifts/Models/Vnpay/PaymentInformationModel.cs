namespace Fruitful_Gifts.Models.Vnpay
{
    public class PaymentInformationModel
    {
        public string OrderType { get; set; }
        public decimal Amount { get; set; }
        public string OrderDescription { get; set; }
        public string? Name { get; set; }
        public string? Email { get; set; }
        public string PaymentMethod { get; set; }
        public string? Sdt { get; set; }            // thêm
        public string? DiaChi { get; set; }
        public decimal PhiVanChuyenBanHang { get; set; }  // thêm phí vận chuyển
        public string? PhuongThucBan { get; set; }
    }

}
