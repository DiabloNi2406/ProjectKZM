using System.ComponentModel.DataAnnotations;

namespace ProjectKZM.ViewModels
{
    public class DatLaiMatKhauViewModel
    {
        [Required(ErrorMessage = "Vui lòng nhập mật khẩu mới.")]
        [StringLength(
            100,
            MinimumLength = 6,
            ErrorMessage =
                "Mật khẩu phải có từ 6 đến 100 ký tự.")]
        [DataType(DataType.Password)]
        [Display(Name = "Mật khẩu mới")]
        public string MatKhauMoi { get; set; } = string.Empty;


        [Required(ErrorMessage = "Vui lòng xác nhận mật khẩu.")]
        [DataType(DataType.Password)]
        [Compare(
            "MatKhauMoi",
            ErrorMessage =
                "Mật khẩu xác nhận không khớp.")]
        [Display(Name = "Xác nhận mật khẩu")]
        public string XacNhanMatKhau { get; set; } = string.Empty;
    }
}