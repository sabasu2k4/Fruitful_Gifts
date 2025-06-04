using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Fruitful_Gifts.Database;

public partial class DanhMucGioQua
{
    public int MaDm { get; set; }

    public string TenDm { get; set; } = null!;

    public string? HinhAnh { get; set; }

    public int? TrangThai { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public int? DanhMucChaId { get; set; }

    public string? Slug { get; set; }

    public virtual DanhMucGioQua? DanhMucCha { get; set; }

    public virtual ICollection<GioQua> GioQuas { get; set; } = new List<GioQua>();

    public virtual ICollection<DanhMucGioQua> InverseDanhMucCha { get; set; } = new List<DanhMucGioQua>();

    // Dùng để nhận file ảnh upload, không lưu vào DB
    [NotMapped]
    public IFormFile? ImageUpload { get; set; }
}
