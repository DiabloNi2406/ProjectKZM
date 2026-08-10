using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProjectKZM.Models
{
    public class NguoiDung
    {
        [Key]
        public int MaNguoiDung { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập họ tên")]
        [StringLength(100)]
        [Display(Name = "Họ tên")]
        public string HoTen { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập Email")]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập mật khẩu")]
        [StringLength(100)]
        [DataType(DataType.Password)]
        [Display(Name = "Mật khẩu")]
        public string MatKhau { get; set; } = string.Empty;

        [StringLength(15)]
        [Display(Name = "Số điện thoại")]
        public string? SoDienThoai { get; set; }

        [StringLength(255)]
        [Display(Name = "Địa chỉ")]
        public string? DiaChi { get; set; }

        [Display(Name = "Ảnh đại diện")]
        public string? AnhDaiDien { get; set; }

        //================ Khóa ngoại ===================

        [Display(Name = "Vai trò")]
        public int MaVaiTro { get; set; }

        [ForeignKey(nameof(MaVaiTro))]
        public virtual VaiTro? VaiTro { get; set; }

        //================ Navigation ===================

        public virtual ICollection<GioHang> GioHangs { get; set; }
            = new List<GioHang>();

        public virtual ICollection<DonHang> DonHangs { get; set; }
            = new List<DonHang>();
    }
}