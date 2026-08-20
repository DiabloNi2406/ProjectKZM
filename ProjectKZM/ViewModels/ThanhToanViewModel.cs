using System.ComponentModel.DataAnnotations;

namespace ProjectKZM.ViewModels
{
    public class ThanhToanViewModel
    {
        // =========================================================
        // THÔNG TIN GIAO HÀNG
        // =========================================================

        [Required(ErrorMessage = "Vui lòng nhập họ tên")]
        [StringLength(100)]
        [Display(Name = "Họ tên người nhận")]
        public string HoTen { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập số điện thoại")]
        [StringLength(15)]
        [Display(Name = "Số điện thoại")]
        public string SoDienThoai { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập địa chỉ giao hàng")]
        [StringLength(255)]
        [Display(Name = "Địa chỉ giao hàng")]
        public string DiaChiGiaoHang { get; set; } = string.Empty;

        [StringLength(500)]
        [Display(Name = "Ghi chú")]
        public string? GhiChu { get; set; }

        // =========================================================
        // PHƯƠNG THỨC THANH TOÁN
        // =========================================================

        [Required(ErrorMessage = "Vui lòng chọn phương thức thanh toán")]
        [Display(Name = "Phương thức thanh toán")]
        public string PhuongThucThanhToan { get; set; } = "VNPAY";

        // =========================================================
        // MÃ ĐƠN HÀNG
        // =========================================================

        public int? MaDonHang { get; set; }

        // =========================================================
        // DANH SÁCH SẢN PHẨM
        // =========================================================

        public List<SanPhamThanhToanViewModel> SanPhams { get; set; }
            = new List<SanPhamThanhToanViewModel>();

        // =========================================================
        // TỔNG
        // =========================================================

        public int TongSoLuong { get; set; }

        public decimal TongTien { get; set; }
    }

    public class SanPhamThanhToanViewModel
    {
        public int MaSach { get; set; }

        public string TenSach { get; set; } = string.Empty;

        public string? Hinh { get; set; }

        public int SoLuong { get; set; }

        public decimal DonGia { get; set; }

        public decimal ThanhTien
        {
            get
            {
                return DonGia * SoLuong;
            }
        }
    }
}