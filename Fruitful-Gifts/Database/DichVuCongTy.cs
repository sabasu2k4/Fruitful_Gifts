using System;
using System.Collections.Generic;

namespace Fruitful_Gifts.Database;

public partial class DichVuCongTy
{
    public int Id { get; set; }

    public string? TieuDe { get; set; }

    public string? MoTa { get; set; }

    public string? HinhAnh { get; set; }

    public bool? TrangThai { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public bool? IsHienThi { get; set; }
}
