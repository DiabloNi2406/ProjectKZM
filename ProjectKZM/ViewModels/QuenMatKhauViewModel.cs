using System.ComponentModel.DataAnnotations;

namespace ProjectKZM.ViewModels
{
    public class QuenMatKhauViewModel
    {
        [Required(ErrorMessage = "Vui lòng nhập Email.")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ.")]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;
    }
}