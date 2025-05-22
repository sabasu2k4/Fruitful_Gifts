using System;
using System.Collections.Generic;

namespace Fruitful_Gifts.Database;

public partial class ChiTietSanPham
{
    public int MaSpCha { get; set; }

    public int MaSpCon { get; set; }

    public int? SoLuong { get; set; }

    public virtual SanPham MaSpChaNavigation { get; set; } = null!;

    public virtual SanPham MaSpConNavigation { get; set; } = null!;
}
