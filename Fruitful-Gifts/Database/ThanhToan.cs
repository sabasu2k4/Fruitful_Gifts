using System;
using System.Collections.Generic;

namespace Fruitful_Gifts.Database;

public partial class ThanhToan
{
    public int Id { get; set; }

    public int MaDh { get; set; }

    public int MaPt { get; set; }

    public string? TransactionCode { get; set; }

    public string? BankCode { get; set; }

    public string? ResponseCode { get; set; }

    public decimal Amount { get; set; }

    public string? PaymentStatus { get; set; }

    public DateTime? PaymentTime { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual DonHang MaDhNavigation { get; set; } = null!;

    public virtual PhuongThucThanhToan MaPtNavigation { get; set; } = null!;
}
