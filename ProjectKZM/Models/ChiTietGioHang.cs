using System.ComponentModel.DataAnnotations.Schema;

namespace ProjectKZM.Models
{
    public class ChiTietGioHang
    {
        [ForeignKey("GioHang")]
        public int MaGioHang { get; set; }

        [ForeignKey("Sach")]
        public int MaSach { get; set; }

        public int SoLuong { get; set; }

        public GioHang? GioHang { get; set; }

        public Sach? Sach { get; set; }
    }
}