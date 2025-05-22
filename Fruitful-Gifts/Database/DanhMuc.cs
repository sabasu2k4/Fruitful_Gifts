using System;
using System.Collections.Generic;

namespace Fruitful_Gifts.Database;

public partial class DanhMuc
{
    public int MaDm { get; set; }

    public string TenDm { get; set; } = null!;

    public string? HinhAnh { get; set; }

    public int? TrangThai { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public int? DanhMucChaId { get; set; }

    public string? Slug { get; set; }

    public virtual DanhMuc? DanhMucCha { get; set; }

    public virtual ICollection<DanhMuc> InverseDanhMucCha { get; set; } = new List<DanhMuc>();

    public virtual ICollection<SanPham> SanPhams { get; set; } = new List<SanPham>();
}
