using System;
using System.Collections.Generic;

namespace Fruitful_Gifts.Database;

public partial class KhoHang
{
    public int MaKho { get; set; }

    public int MaSp { get; set; }

    public int SoLuongTon { get; set; }

    public string? DonViTinh { get; set; }

    public string? GhiChu { get; set; }

    public int? TrangThai { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual SanPham MaSpNavigation { get; set; } = null!;
}
