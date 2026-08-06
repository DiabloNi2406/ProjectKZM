using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProjectKZM.Models
{
    public class NguoiDung
    {
        [Key]
        public int MaNguoiDung { get; set; }

        [Required(ErrorMessage = "Họ tên không được để trống")]
        [StringLength(100)]
        public string HoTen { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string MatKhau { get; set; } = string.Empty;

        [StringLength(15)]
        public string? SoDienThoai { get; set; }

        [StringLength(255)]
        public string? DiaChi { get; set; }

        public string? AnhDaiDien { get; set; }

        // Khóa ngoại
        [ForeignKey("VaiTro")]
        public int MaVaiTro { get; set; }

        // Navigation Property
        public VaiTro? VaiTro { get; set; }

        public ICollection<GioHang> GioHangs { get; set; } = new List<GioHang>();

        public ICollection<DonHang> DonHangs { get; set; } = new List<DonHang>();
    }
}
