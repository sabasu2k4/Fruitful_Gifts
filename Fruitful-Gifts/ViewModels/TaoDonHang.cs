using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace Fruitful_Gifts.ViewModels
{
    // ViewModel cho màn hình tạo đơn
    public class CreateOfflineOrderVM
    {
        [Required(ErrorMessage = "Vui lòng nhập tên khách hàng")]
        public string CustomerName { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập số điện thoại")]
        [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
        public string PhoneNumber { get; set; }

        public List<OrderItemVM> Items { get; set; } = new List<OrderItemVM>();

        [Required(ErrorMessage = "Vui lòng chọn phương thức thanh toán")]
        public int PaymentMethodId { get; set; }

        public string? Note { get; set; }
    }

    public class OrderItemVM
    {
        [Required]
        public int Type { get; set; } // 0: Sản phẩm, 1: Giỏ quà

        [Required(ErrorMessage = "Vui lòng chọn sản phẩm/giỏ quà")]
        public int ItemId { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập số lượng")]
        [Range(0.1, 1000, ErrorMessage = "Số lượng phải từ 0.1 đến 1000")]
        public decimal Quantity { get; set; }

        [Required]
        public decimal UnitPrice { get; set; }

        public decimal TotalPrice => Quantity * UnitPrice;
    }
}

