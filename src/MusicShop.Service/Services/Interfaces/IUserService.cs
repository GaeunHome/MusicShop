using Microsoft.AspNetCore.Identity;
using MusicShop.Service.ViewModels.Account;
using MusicShop.Service.ViewModels.Admin;

namespace MusicShop.Service.Services.Interfaces;

/// <summary>
/// 使用者服務介面
/// 負責使用者相關的商業邏輯（包含 Identity 操作的封裝）
/// </summary>
public interface IUserService
{
    /// <summary>
    /// 取得使用者基本資訊（姓名、電話）
    /// </summary>
    Task<(string FullName, string PhoneNumber)> GetUserBasicInfoAsync(string userId);

    /// <summary>
    /// 註冊新使用者
    /// </summary>
    Task<(bool Success, string UserId, IEnumerable<string> Errors)> RegisterAsync(RegisterViewModel model);

    /// <summary>
    /// 登入使用者
    /// </summary>
    /// <returns>
    /// Success：是否登入成功
    /// FullName：使用者姓名（成功時）
    /// IsLockedOut：帳號是否被鎖定
    /// LockoutMinutes：剩餘鎖定分鐘數（鎖定時）
    /// RemainingAttempts：剩餘可嘗試次數（失敗但未鎖定時，-1 表示 Email 未驗證）
    /// RequiresTwoFactor：是否需要兩步驟驗證
    /// </returns>
    Task<(bool Success, string? FullName, bool IsLockedOut, int LockoutMinutes, int RemainingAttempts, bool RequiresTwoFactor)> LoginAsync(string email, string password, bool rememberMe);

    /// <summary>
    /// 登出使用者
    /// </summary>
    Task LogoutAsync();

    /// <summary>
    /// 取得使用者個人資料（用於編輯頁面）
    /// </summary>
    Task<EditProfileViewModel?> GetEditProfileAsync(string userId);

    /// <summary>
    /// 更新使用者個人資料
    /// </summary>
    Task<(bool Success, IEnumerable<string> Errors)> UpdateProfileAsync(string userId, EditProfileViewModel model);

    /// <summary>
    /// 取得使用者帳戶摘要資訊（姓名、Email、註冊日期、個人資料完整度）
    /// </summary>
    Task<(string FullName, string? Email, DateTime RegisteredAt, bool HasPhone, bool HasBirthday)?> GetAccountSummaryAsync(string userId);

    /// <summary>
    /// 取得所有使用者及其角色資訊
    /// </summary>
    /// <returns>使用者管理 ViewModel 列表</returns>
    Task<List<UserManagementViewModel>> GetAllUsersWithRolesAsync();

    /// <summary>
    /// 切換使用者的管理員角色（指派或移除）
    /// </summary>
    /// <param name="userId">使用者 ID</param>
    /// <param name="currentAdminId">當前管理員 ID（防止移除自己的權限）</param>
    /// <returns>操作結果訊息</returns>
    Task<(bool Success, string Message)> ToggleAdminRoleAsync(string userId, string currentAdminId);

    /// <summary>
    /// 管理員手動確認使用者 Email（用於舊帳號或特殊情況）
    /// </summary>
    Task<(bool Success, string Message)> AdminConfirmEmailAsync(string userId);

    /// <summary>
    /// 更新使用者密碼
    /// </summary>
    /// <param name="userId">使用者 ID</param>
    /// <param name="currentPassword">目前密碼</param>
    /// <param name="newPassword">新密碼</param>
    /// <returns>操作結果訊息</returns>
    Task<(bool Success, string Message)> ChangePasswordAsync(string userId, string currentPassword, string newPassword);

    // ==================== Email 驗證 ====================

    /// <summary>
    /// 產生 Email 驗證 Token
    /// </summary>
    Task<(string? Token, string? UserId)> GenerateEmailConfirmationTokenAsync(string userId);

    /// <summary>
    /// 確認 Email 驗證
    /// </summary>
    Task<(bool Success, string Message)> ConfirmEmailAsync(string userId, string token);

    /// <summary>
    /// 重新寄送驗證信
    /// </summary>
    Task<(string? Token, string? UserId, string? Email)> ResendConfirmationTokenAsync(string userId);

    /// <summary>
    /// 檢查 Email 是否已驗證
    /// </summary>
    Task<bool> IsEmailConfirmedAsync(string userId);

    // ==================== 兩步驟驗證 (2FA) ====================

    /// <summary>
    /// 取得使用者 2FA 設定狀態
    /// </summary>
    Task<TwoFactorStatusViewModel> GetTwoFactorStatusAsync(string userId);

    /// <summary>
    /// 開始設定 TOTP 驗證器（重設金鑰並產生新的 AuthenticatorUri）
    /// </summary>
    Task<SetupAuthenticatorViewModel> SetupAuthenticatorAsync(string userId);

    /// <summary>
    /// 取得現有的 TOTP 驗證器設定（不重設金鑰，用於驗證失敗後重新顯示）
    /// </summary>
    Task<SetupAuthenticatorViewModel> GetAuthenticatorSetupAsync(string userId);

    /// <summary>
    /// 驗證並啟用 TOTP 驗證器
    /// </summary>
    Task<(bool Success, string Message)> EnableAuthenticatorAsync(string userId, string verificationCode);

    /// <summary>
    /// 寄送 Email 2FA 設定驗證碼
    /// </summary>
    Task<(bool Success, string Message, string? Code)> GenerateEmailTwoFactorSetupCodeAsync(string userId);

    /// <summary>
    /// 確認 Email 2FA 設定
    /// </summary>
    Task<(bool Success, string Message)> EnableEmailTwoFactorAsync(string userId, string verificationCode);

    /// <summary>
    /// 停用 2FA
    /// </summary>
    Task<(bool Success, string Message)> DisableTwoFactorAsync(string userId);

    /// <summary>
    /// 取得等待 2FA 驗證的使用者資訊
    /// </summary>
    Task<(string? UserId, string? PreferredMethod, string? Email)> GetTwoFactorUserInfoAsync();

    /// <summary>
    /// 使用 TOTP 驗證碼完成 2FA 登入
    /// </summary>
    Task<(bool Success, string? FullName, bool IsLockedOut, int LockoutMinutes)> TwoFactorAuthenticatorLoginAsync(string code, bool rememberMe);

    /// <summary>
    /// 使用 Email 驗證碼完成 2FA 登入
    /// </summary>
    Task<(bool Success, string? FullName, bool IsLockedOut, int LockoutMinutes)> TwoFactorEmailLoginAsync(string code, bool rememberMe);

    /// <summary>
    /// 產生並回傳 Email 2FA 登入驗證碼
    /// </summary>
    Task<(bool Success, string Message, string? Code)> GenerateTwoFactorEmailCodeAsync();

    // ==================== 忘記密碼 ====================

    /// <summary>
    /// 產生密碼重設 Token
    /// </summary>
    Task<(string? Token, string? UserId)> GeneratePasswordResetTokenAsync(string email);

    /// <summary>
    /// 使用 Token 重設密碼
    /// </summary>
    Task<(bool Success, string Message)> ResetPasswordAsync(string userId, string token, string newPassword);

    // ==================== 社群登入 ====================

    /// <summary>
    /// 透過外部登入資訊尋找或建立使用者，並完成登入
    /// </summary>
    Task<(bool Success, string? FullName, bool IsNewUser)> ExternalLoginAsync(
        string provider, string providerKey, string? email, string? name);

    /// <summary>
    /// 取得已設定的外部登入提供者名稱清單
    /// </summary>
    Task<IEnumerable<string>> GetExternalAuthenticationSchemesAsync();
}
