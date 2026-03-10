using System.ComponentModel.DataAnnotations;

namespace MusicShop.Service.ViewModels.Account;

/// <summary>
/// 更新密碼資料模型
/// </summary>
public class ChangePasswordViewModel
{
    [Required(ErrorMessage = "請輸入目前密碼")]
    [DataType(DataType.Password)]
    [Display(Name = "目前密碼")]
    public string CurrentPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "請輸入新密碼")]
    [StringLength(100, ErrorMessage = "{0} 長度至少需要 {2} 個字元", MinimumLength = 6)]
    [DataType(DataType.Password)]
    [Display(Name = "新密碼")]
    public string NewPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "請再次輸入新密碼")]
    [DataType(DataType.Password)]
    [Display(Name = "確認新密碼")]
    [Compare("NewPassword", ErrorMessage = "新密碼與確認密碼不一致")]
    public string ConfirmPassword { get; set; } = string.Empty;
}
