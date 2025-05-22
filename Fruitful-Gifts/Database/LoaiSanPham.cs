using System;
using System.Collections.Generic;

namespace Fruitful_Gifts.Database;

public partial class LoaiSanPham
{
    public int MaLoai { get; set; }

    public string? TenLoai { get; set; }

    public virtual ICollection<SanPham> SanPhams { get; set; } = new List<SanPham>();
}
