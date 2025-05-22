using System;
using System.Collections.Generic;

namespace Fruitful_Gifts.Database;

public partial class KhuyenMai
{
    public int Id { get; set; }

    public int? MaSp { get; set; }

    public decimal? MucGiamGia { get; set; }

    public int? TrangThai { get; set; }

    public DateOnly? NgayBatDau { get; set; }

    public DateOnly? NgayKetThuc { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual SanPham? MaSpNavigation { get; set; }
}
