using System.ComponentModel.DataAnnotations;

namespace ProjectKZM.Models
{
    public class TheLoai
    {
        [Key]
        public int MaTheLoai { get; set; }

        [Required(ErrorMessage = "Tên thể loại không được để trống")]
        [StringLength(100)]
        public string TenTheLoai { get; set; } = string.Empty;

        [StringLength(500)]
        public string? MoTa { get; set; }

        public string? Hinh { get; set; }

        // Navigation Property
        public ICollection<Sach> Sachs { get; set; } = new List<Sach>();
    }
}
