using System;
using System.Collections.Generic;

namespace Fruitful_Gifts.Database;

public partial class Luong
{
    public int MaLuong { get; set; }

    public int? NhanVienId { get; set; }

    public DateOnly? ThangNam { get; set; }

    public decimal? LuongCoBan { get; set; }

    public int? SoNgayCong { get; set; }

    public decimal? LuongPhuCap { get; set; }

    public decimal? Thuong { get; set; }

    public decimal? Phat { get; set; }

    public decimal? TongLuong { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual NhanVien? NhanVien { get; set; }
}
