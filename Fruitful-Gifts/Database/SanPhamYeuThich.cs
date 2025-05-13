using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Fruitful_Gifts.Database;

public partial class SanPhamYeuThich
{
    public int MaKh { get; set; }

    public int MaSp { get; set; }

    [Key]
    public int Id { get; set; }

    public virtual KhachHang MaKhNavigation { get; set; } = null!;

    public virtual SanPham MaSpNavigation { get; set; } = null!;
}
