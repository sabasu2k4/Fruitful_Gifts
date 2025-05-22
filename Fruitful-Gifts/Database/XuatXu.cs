using System;
using System.Collections.Generic;

namespace Fruitful_Gifts.Database;

public partial class XuatXu
{
    public int MaXuatXu { get; set; }

    public string? TenNuoc { get; set; }

    public virtual ICollection<SanPham> SanPhams { get; set; } = new List<SanPham>();
}
