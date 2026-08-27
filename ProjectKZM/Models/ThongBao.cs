using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProjectKZM.Models
{
    public class ThongBao
    {
        [Key]
        public int MaThongBao { get; set; }


        public int MaNguoiDung { get; set; }

        [ForeignKey(nameof(MaNguoiDung))]
        public NguoiDung? NguoiDung { get; set; }


        [Required]
        [StringLength(200)]
        public string TieuDe { get; set; } = string.Empty;


        [Required]
        [StringLength(500)]
        public string NoiDung { get; set; } = string.Empty;


        [StringLength(300)]
        public string? DuongDan { get; set; }


        [StringLength(50)]
        public string? Loai { get; set; }


        public int? MaDonHang { get; set; }


        public bool DaDoc { get; set; }


        public DateTime NgayTao { get; set; }
    }
}