using System;
using System.Collections.Generic;

namespace Fruitful_Gifts.Database;

public partial class NhaCungCap
{
    public int MaNcc { get; set; }

    public string? TenNcc { get; set; }

    public string? Sdt { get; set; }

    public string? Email { get; set; }

    public int? TrangThai { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual ICollection<NhapHang> NhapHangs { get; set; } = new List<NhapHang>();

    public virtual ICollection<SanPham> SanPhams { get; set; } = new List<SanPham>();
}
