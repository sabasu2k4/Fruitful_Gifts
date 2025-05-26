using System;
using System.Collections.Generic;

namespace Fruitful_Gifts.Database;

public partial class Header
{
    public int Id { get; set; }

    public string? TieuDe { get; set; }

    public string? DuongLienKet { get; set; }

    public string? Icon { get; set; }

    public int? ViTriHienThi { get; set; }

    public int? TrangThai { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }
}
