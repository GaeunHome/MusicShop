using System.ComponentModel.DataAnnotations;
using MusicShop.Library.Enums;

namespace MusicShop.Service.ViewModels.Account
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "請輸入姓名")]
        [StringLength(50, ErrorMessage = "姓名長度不可超過50個字元")]
        [Display(Name = "姓名")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "請輸入Email")]
        [EmailAddress(ErrorMessage = "Email格式不正確")]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "請輸入手機號碼")]
        [Phone(ErrorMessage = "手機號碼格式不正確")]
        [RegularExpression(@"^09\d{8}$", ErrorMessage = "請輸入正確的台灣手機號碼格式（09開頭共10碼）")]
        [Display(Name = "手機號碼")]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "請選擇生日")]
        [DataType(DataType.Date)]
        [Display(Name = "生日")]
        public DateTime Birthday { get; set; }

        [Required(ErrorMessage = "請選擇性別")]
        [Display(Name = "性別")]
        public Gender Gender { get; set; }

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