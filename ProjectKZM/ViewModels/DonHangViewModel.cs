using System;

namespace ProjectKZM.ViewModels
{
    public class DonHangViewModel
    {
        public int MaDonHang { get; set; }

        public DateTime NgayDat { get; set; }

        public decimal TongTien { get; set; }

        public string TrangThai { get; set; } = string.Empty;

        public string? PhuongThucThanhToan { get; set; }

        public string? DiaChiGiaoHang { get; set; }

        public string? GhiChu { get; set; }

        public int MaNguoiDung { get; set; }

        public string TenKhachHang { get; set; } = string.Empty;

        public string EmailKhachHang { get; set; } = string.Empty;

        public int TongSoLuong { get; set; }
    }
}