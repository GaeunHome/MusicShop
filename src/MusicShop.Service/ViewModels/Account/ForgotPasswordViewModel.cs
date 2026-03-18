using System.ComponentModel.DataAnnotations;

namespace MusicShop.Service.ViewModels.Account;

/// <summary>
/// 忘記密碼 - 輸入 Email 請求重設連結
/// </summary>
public class ForgotPasswordViewModel
{
    [Required(ErrorMessage = "請輸入註冊時使用的 Email")]
    [EmailAddress(ErrorMessage = "Email 格式不正確")]
    [Display(Name = "Email")]
    public string Email { get; set; } = string.Empty;
}
