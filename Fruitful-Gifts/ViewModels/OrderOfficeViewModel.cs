using Microsoft.Build.Framework;
using System;
using System.Collections.Generic;

namespace Fruitful_Gifts.ViewModels
{
    // Thêm vào namespace Fruitful_Gifts.ViewModels
    public class OfflineOrderViewModel
    {
        public int? CustomerId { get; set; }

        public string PhoneNumber { get; set; }

        public string Address { get; set; }

        public string Note { get; set; }

        [Required]
        public int PaymentMethodId { get; set; }

        public List<OfflineOrderItemViewModel> Items { get; set; } = new();
    }

    public class OfflineOrderItemViewModel
    {
        public int ProductId { get; set; }

        public int GiftBasketId { get; set; }

        public decimal Price { get; set; }

        public int Quantity { get; set; }
    }
    // Nên tách ra file riêng
    public class InventoryCheckResult
    {
        public string ItemName { get; set; }
        public bool IsAvailable { get; set; }
        public string Message { get; set; }
        public List<InventoryDetail> Details { get; set; } = new List<InventoryDetail>();
    }

    public class InventoryDetail
    {
        public string ProductName { get; set; }
        public decimal Required { get; set; }
        public decimal Available { get; set; }
    }
}
