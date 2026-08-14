using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProjectKZM.Models
{
    public class YeuThich
    {
        [Key]
        public int MaYeuThich { get; set; }

        public int MaNguoiDung { get; set; }

        public int MaSach { get; set; }

        public DateTime NgayThem { get; set; } = DateTime.Now;


        // ==========================================
        // QUAN HỆ NGƯỜI DÙNG
        // ==========================================

        [ForeignKey("MaNguoiDung")]
        public virtual NguoiDung? NguoiDung { get; set; }


        // ==========================================
        // QUAN HỆ SÁCH
        // ==========================================

        [ForeignKey("MaSach")]
        public virtual Sach? Sach { get; set; }
    }
}