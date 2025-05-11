using System;
using System.Collections.Generic;

namespace Fruitful_Gifts.Database;

public partial class DanhMuc
{
    public int MaDm { get; set; }

    public string? TenDm { get; set; }

    public string? HinhAnh { get; set; }

    public bool? TrangThai { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual ICollection<SanPham> SanPhams { get; set; } = new List<SanPham>();
}
