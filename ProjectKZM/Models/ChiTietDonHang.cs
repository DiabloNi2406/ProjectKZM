using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProjectKZM.Models
{
    public class ChiTietDonHang
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int MaChiTietDonHang { get; set; }

        [ForeignKey("DonHang")]
        public int MaDonHang { get; set; }

        [ForeignKey("Sach")]
        public int MaSach { get; set; }

        public int SoLuong { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal DonGia { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal ThanhTien { get; set; }

        // Navigation Property

        public DonHang? DonHang { get; set; }

        public Sach? Sach { get; set; }
    }
}