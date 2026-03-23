using System.ComponentModel.DataAnnotations;

namespace MusicShop.Service.ViewModels.Account;

/// <summary>
/// TOTP 驗證器設定 ViewModel
/// </summary>
public class SetupAuthenticatorViewModel
{
    /// <summary>
    /// 手動輸入金鑰（Base32 編碼）
    /// </summary>
    public string SharedKey { get; set; } = string.Empty;

    /// <summary>
    /// otpauth:// URI（供 Controller 層產生 QR Code 圖片）
    /// </summary>
    public string AuthenticatorUri { get; set; } = string.Empty;

    /// <summary>
    /// QR Code 圖片（Base64 編碼的 PNG，由 Controller 層產生）
    /// </summary>
    public string QrCodeBase64 { get; set; } = string.Empty;

    /// <summary>
    /// 使用者輸入的驗證碼
    /// </summary>
    [Required(ErrorMessage = "請輸入驗證碼")]
    [StringLength(7, MinimumLength = 6, ErrorMessage = "驗證碼為 6 位數字")]
    [Display(Name = "驗證碼")]
    public string VerificationCode { get; set; } = string.Empty;
}
