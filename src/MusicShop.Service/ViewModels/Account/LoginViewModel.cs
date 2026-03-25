using System.ComponentModel.DataAnnotations;

namespace MusicShop.Service.ViewModels.Account
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "請輸入Email")]
        [EmailAddress(ErrorMessage = "Email格式不正確")]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "請輸入密碼")]
        [DataType(DataType.Password)]
        [Display(Name = "密碼")]
        public string Password { get; set; } = string.Empty;

        [Display(Name = "記住我")]
        public bool RememberMe { get; set; }

        [Required(ErrorMessage = "請輸入驗證碼")]
        [StringLength(6, MinimumLength = 4, ErrorMessage = "驗證碼長度不正確")]
        [Display(Name = "驗證碼")]
        public string CaptchaCode { get; set; } = string.Empty;
    }
}