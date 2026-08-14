using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProjectKZM.Models
{
    public class Sach
    {
        [Key]
        public int MaSach { get; set; }

        [Required]
        [StringLength(200)]
        public string TenSach { get; set; } = string.Empty;

        [StringLength(100)]
        public string? TacGia { get; set; }

        [StringLength(100)]
        public string? NhaXuatBan { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Gia { get; set; }

        public int SoLuong { get; set; }

        public string? Chuong { get; set; }

        public string? Hinh { get; set; }

        public string? MoTa { get; set; }

        // ==============================
        // LOẠI SÁCH
        // ==============================

        public int? MaLoaiSach { get; set; }

        [ForeignKey("MaLoaiSach")]
        public LoaiSach? LoaiSach { get; set; }

        // ==============================
        // THỂ LOẠI
        // ==============================

        public ICollection<SachTheLoai> SachTheLoais { get; set; }
            = new List<SachTheLoai>();

        // ==============================
        // GIỎ HÀNG / ĐƠN HÀNG
        // ==============================

        public ICollection<ChiTietGioHang> ChiTietGioHangs { get; set; }
            = new List<ChiTietGioHang>();

        public ICollection<ChiTietDonHang> ChiTietDonHangs { get; set; }
            = new List<ChiTietDonHang>();

        // ==============================
        // YÊU THÍCH
        // ==============================

        public ICollection<YeuThich> YeuThichs { get; set; }
            = new List<YeuThich>();
    }
}