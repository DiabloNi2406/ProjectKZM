using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProjectKZM.Models
{
    public class Sach
    {
        [Key]
        public int MaSach { get; set; }

        [Required(ErrorMessage = "Tên sách không được để trống")]
        [StringLength(200)]
        public string TenSach { get; set; } = string.Empty;

        [StringLength(100)]
        public string? TacGia { get; set; }

        [StringLength(100)]
        public string? NhaXuatBan { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Gia { get; set; }

        public int SoLuong { get; set; }

        public string? Hinh { get; set; }

        public string? MoTa { get; set; }

        // Khóa ngoại
        [ForeignKey("TheLoai")]
        public int MaTheLoai { get; set; }

        // Navigation Property
        public TheLoai? TheLoai { get; set; }

        public ICollection<ChiTietGioHang> ChiTietGioHangs { get; set; } = new List<ChiTietGioHang>();

        public ICollection<ChiTietDonHang> ChiTietDonHangs { get; set; } = new List<ChiTietDonHang>();
    }
}
