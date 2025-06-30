using Microsoft.AspNetCore.Mvc;

namespace Fruitful_Gifts.ViewModels
{
    public class DonHangOfflineViewModel
    {
        public int MaNv { get; set; }
        public List<SelectedSanPham> SelectedSanPhams { get; set; }
        public List<SelectedGioQua> SelectedGioQuas { get; set; }
        public Func<string, int> TinhSoLuongTonGioQua { get; set; }
    }

    public class SelectedSanPham
    {
        public int MaSp { get; set; }
        public decimal SoLuong { get; set; }
    }

    public class SelectedGioQua
    {
        public int MaGq { get; set; }
        public decimal SoLuong { get; set; }
    }
}
