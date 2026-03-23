namespace MusicShop.Service.ViewModels.Account;

/// <summary>
/// 兩步驟驗證狀態 ViewModel
/// </summary>
public class TwoFactorStatusViewModel
{
    /// <summary>
    /// 是否已啟用 2FA
    /// </summary>
    public bool IsEnabled { get; set; }

    /// <summary>
    /// 偏好的驗證方式（"Authenticator" 或 "Email"）
    /// </summary>
    public string? PreferredMethod { get; set; }

    /// <summary>
    /// 是否已設定驗證器
    /// </summary>
    public bool HasAuthenticator { get; set; }
}
