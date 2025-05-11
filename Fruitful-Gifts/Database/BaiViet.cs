using System;
using System.Collections.Generic;

namespace Fruitful_Gifts.Database;

public partial class BaiViet
{
    public int MaBv { get; set; }

    public string? TieuDe { get; set; }

    public string? HinhAnh { get; set; }

    public string? NoiDung { get; set; }

    public DateOnly? NgayDang { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public bool? IsHienThi { get; set; }
}
