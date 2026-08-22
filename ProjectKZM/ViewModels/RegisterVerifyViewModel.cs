using System.ComponentModel.DataAnnotations;

namespace ProjectKZM.ViewModels
{
    public class RegisterVerifyViewModel
    {
        [Required(ErrorMessage = "Vui lòng nhập mã OTP.")]
        [StringLength(
            6,
            MinimumLength = 6,
            ErrorMessage = "OTP phải gồm 6 chữ số.")]
        [RegularExpression(
            @"^\d{6}$",
            ErrorMessage = "OTP phải gồm 6 chữ số.")]
        [Display(Name = "Mã xác minh")]
        public string OTP { get; set; } = string.Empty;
    }
}