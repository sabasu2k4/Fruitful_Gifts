using System;
using System.Collections.Generic;

namespace Fruitful_Gifts.Database;

public partial class SanPhamYeuThich
{
    public int Id { get; set; }

    public int? MaKh { get; set; }

    public int? MaSp { get; set; }

    public virtual KhachHang? MaKhNavigation { get; set; }

    public virtual SanPham? MaSpNavigation { get; set; }
}
