using System.ComponentModel.DataAnnotations;

namespace ProjectKZM.Models
{
    public class LoaiSach
    {
        [Key]
        public int MaLoaiSach { get; set; }

        [Required]
        [StringLength(100)]
        public string TenLoaiSach { get; set; } = string.Empty;

        // Quan hệ 1 LoaiSach - nhiều Sach
        public ICollection<Sach> Sachs { get; set; } = new List<Sach>();
    }
}