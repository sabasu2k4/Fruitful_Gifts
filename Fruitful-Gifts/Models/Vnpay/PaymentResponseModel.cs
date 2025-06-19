namespace Fruitful_Gifts.Models.Vnpay
{
    public class PaymentResponseModel
    {
        public string OrderDescription { get; set; }
        public string TransactionId { get; set; }
        public string OrderId { get; set; }
        public string PaymentMethod { get; set; }
        public string PaymentId { get; set; }
        public string Token { get; set; }
        public string VnPayResponseCode { get; set; }
        public decimal Amount { get; set; }
        public string TransactionCode { get; set; }   // vnp_TxnRef
        public string BankCode { get; set; }          // vnp_BankCode
        public string ResponseCode { get; set; }      // vnp_ResponseCode
        
        public bool Success => ResponseCode == "00";  // logic đơn giản
    }

}
