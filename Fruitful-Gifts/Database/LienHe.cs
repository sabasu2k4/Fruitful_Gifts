using System;
using System.Collections.Generic;

namespace Fruitful_Gifts.Database;

public partial class LienHe
{
    public int MaLh { get; set; }

    public string? HoTen { get; set; }

    public string? Email { get; set; }

    public string? Sdt { get; set; }

    public string? NoiDung { get; set; }

    public DateTime? ThoiGianGui { get; set; }

    public bool TrangThai { get; set; }



    public int? MaNv { get; set; }

    public virtual NhanVien? MaNvNavigation { get; set; }
}
