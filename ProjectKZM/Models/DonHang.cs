using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProjectKZM.Models
{
    public class DonHang
    {
        [Key]
        public int MaDonHang { get; set; }

        public DateTime NgayDat { get; set; } = DateTime.Now;

        [Column(TypeName = "decimal(18,2)")]
        public decimal TongTien { get; set; }

        [Required]
        [StringLength(50)]
        public string TrangThai { get; set; } = "Chờ xác nhận";

        [StringLength(50)]
        public string? PhuongThucThanhToan { get; set; }

        [StringLength(255)]
        public string? DiaChiGiaoHang { get; set; }

        [ForeignKey("NguoiDung")]
        public int MaNguoiDung { get; set; }

        // Navigation Property
        public NguoiDung? NguoiDung { get; set; }

        public ICollection<ChiTietDonHang> ChiTietDonHangs { get; set; } = new List<ChiTietDonHang>();
    }
}
