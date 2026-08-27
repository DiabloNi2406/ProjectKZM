namespace ProjectKZM.ViewModels
{
    public class ThongBaoViewModel
    {
        public int MaThongBao { get; set; }

        public string TieuDe { get; set; } = string.Empty;

        public string NoiDung { get; set; } = string.Empty;

        public string? DuongDan { get; set; }

        public string? Loai { get; set; }

        public int? MaDonHang { get; set; }

        public bool DaDoc { get; set; }

        public DateTime NgayTao { get; set; }
    }
}