using System.ComponentModel.DataAnnotations;

namespace MusicShop.Service.ViewModels.Account;

/// <summary>
/// 重設密碼 - 設定新密碼
/// </summary>
public class ResetPasswordViewModel
{
    [Required]
    public string UserId { get; set; } = string.Empty;

    [Required]
    public string Token { get; set; } = string.Empty;

    [Required(ErrorMessage = "請輸入新密碼")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "密碼至少 6 個字元")]
    [DataType(DataType.Password)]
    [Display(Name = "新密碼")]
    public string NewPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "請再次輸入新密碼")]
    [DataType(DataType.Password)]
    [Display(Name = "確認新密碼")]
    [Compare("NewPassword", ErrorMessage = "新密碼與確認密碼不一致")]
    public string ConfirmPassword { get; set; } = string.Empty;
}
