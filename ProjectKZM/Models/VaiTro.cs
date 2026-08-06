using System.ComponentModel.DataAnnotations;

namespace ProjectKZM.Models
{
    public class VaiTro
    {
        [Key]
        public int MaVaiTro { get; set; }

        [Required(ErrorMessage = "Tên vai trò không được để trống")]
        [StringLength(50)]
        public string TenVaiTro { get; set; } = string.Empty;

        // Navigation Property
        public ICollection<NguoiDung> NguoiDungs { get; set; } = new List<NguoiDung>();
    }
}
