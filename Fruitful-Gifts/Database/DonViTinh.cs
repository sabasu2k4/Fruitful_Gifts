using System;
using System.Collections.Generic;

namespace Fruitful_Gifts.Database;

public partial class DonViTinh
{
    public int MaDvt { get; set; }

    public string? TenDvt { get; set; }

    public virtual ICollection<SanPham> SanPhams { get; set; } = new List<SanPham>();
}
