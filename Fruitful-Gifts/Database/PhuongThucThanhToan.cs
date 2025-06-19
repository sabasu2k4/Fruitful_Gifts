using System;
using System.Collections.Generic;

namespace Fruitful_Gifts.Database;

public partial class PhuongThucThanhToan
{
    public int MaPt { get; set; }

    public string? TenPt { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual ICollection<DonHang> DonHangs { get; set; } = new List<DonHang>();

    public virtual ICollection<ThanhToan> ThanhToans { get; set; } = new List<ThanhToan>();
}
