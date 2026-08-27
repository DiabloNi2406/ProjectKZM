using System;
using System.Collections.Generic;

namespace ProjectKZM.ViewModels
{
    public class DonHangChiTietViewModel
    {
        public int MaDonHang { get; set; }

        public DateTime NgayDat { get; set; }

        public decimal TongTien { get; set; }

        public string TrangThai { get; set; } = string.Empty;

        public string? PhuongThucThanhToan { get; set; }

        public string? DiaChiGiaoHang { get; set; }

        public string? GhiChu { get; set; }

        public string? LyDoHuy { get; set; }

        public string TenKhachHang { get; set; } = string.Empty;

        public string EmailKhachHang { get; set; } = string.Empty;

        public List<DonHangSanPhamViewModel> SanPhams { get; set; }
            = new List<DonHangSanPhamViewModel>();
    }


    public class DonHangSanPhamViewModel
    {
        public int MaSach { get; set; }

        public string TenSach { get; set; } = string.Empty;

        public string? Hinh { get; set; }

        public int SoLuong { get; set; }

        public decimal DonGia { get; set; }

        public decimal ThanhTien { get; set; }
    }
}