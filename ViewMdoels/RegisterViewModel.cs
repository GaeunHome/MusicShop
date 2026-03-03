using System.ComponentModel.DataAnnotations;

namespace MusicShop.ViewModels
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "請輸入姓名")]
        [Display(Name = "姓名")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "請輸入Email")]
        [EmailAddress(ErrorMessage = "Email格式不正確")]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "請輸入密碼")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "密碼至少6個字元")]
        [DataType(DataType.Password)]
        [Display(Name = "密碼")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "請確認密碼")]
        [Compare("Password", ErrorMessage = "兩次密碼不一致")]
        [DataType(DataType.Password)]
        [Display(Name = "確認密碼")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}