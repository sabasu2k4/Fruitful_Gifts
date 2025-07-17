using System;

namespace Fruitful_Gifts.Database
{
    public partial class ChamCong
    {
        public int MaChamCong { get; set; }
        public int NhanVienId { get; set; }
        public DateOnly Ngay { get; set; }
        public TimeOnly GioCheckIn { get; set; }
        public string? GhiChu { get; set; }
        public DateTime CreatedAt { get; set; }

        public virtual NhanVien NhanVien { get; set; } = null!;
    }
}
