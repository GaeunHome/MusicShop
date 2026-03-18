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
    /// RemainingAttempts：剩餘可嘗試次數（失敗但未鎖定時）
    /// </returns>
    Task<(bool Success, string? FullName, bool IsLockedOut, int LockoutMinutes, int RemainingAttempts)> LoginAsync(string email, string password, bool rememberMe);

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
    /// 取得使用者帳戶摘要資訊（姓名、Email、註冊日期）
    /// </summary>
    Task<(string FullName, string Email, DateTime RegisteredAt)?> GetAccountSummaryAsync(string userId);

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
    /// 更新使用者密碼
    /// </summary>
    /// <param name="userId">使用者 ID</param>
    /// <param name="currentPassword">目前密碼</param>
    /// <param name="newPassword">新密碼</param>
    /// <returns>操作結果訊息</returns>
    Task<(bool Success, string Message)> ChangePasswordAsync(string userId, string currentPassword, string newPassword);
}
