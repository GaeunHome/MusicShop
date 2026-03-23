using System.ComponentModel.DataAnnotations;

namespace MusicShop.Service.ViewModels.Account;

/// <summary>
/// 兩步驟驗證登入 ViewModel
/// </summary>
public class TwoFactorLoginViewModel
{
    /// <summary>
    /// 驗證碼（6 位數字）
    /// </summary>
    [Required(ErrorMessage = "請輸入驗證碼")]
    [StringLength(7, MinimumLength = 6, ErrorMessage = "驗證碼為 6 位數字")]
    [Display(Name = "驗證碼")]
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// 記住登入狀態
    /// </summary>
    public bool RememberMe { get; set; }

    /// <summary>
    /// 驗證方式（"Authenticator" 或 "Email"）
    /// </summary>
    public string Method { get; set; } = string.Empty;
}
