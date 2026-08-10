using System.ComponentModel.DataAnnotations;

namespace ProjectKZM.Models
{
    public class TheLoai
    {
        [Key]
        public int MaTheLoai { get; set; }

        [Required]
        [StringLength(200)]
        public string TenTheLoai { get; set; } = string.Empty;

        public string? MoTa { get; set; }

        public string? Hinh { get; set; }

        // Quan hệ nhiều-nhiều với Sach
        public ICollection<SachTheLoai> SachTheLoais { get; set; }
            = new List<SachTheLoai>();
    }
}