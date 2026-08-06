using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProjectKZM.Models
{
    public class GioHang
    {
        [Key]
        public int MaGioHang { get; set; }

        [ForeignKey("NguoiDung")]
        public int MaNguoiDung { get; set; }

        public DateTime NgayTao { get; set; } = DateTime.Now;

        // Navigation Property
        public NguoiDung? NguoiDung { get; set; }

        public ICollection<ChiTietGioHang> ChiTietGioHangs { get; set; } = new List<ChiTietGioHang>();
    }
}
