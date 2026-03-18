using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MusicShop.Data.Entities;
using MusicShop.Service.Services.Interfaces;
using MusicShop.Service.ViewModels.Account;
using MusicShop.Service.ViewModels.Admin;

namespace MusicShop.Service.Services.Implementation;

/// <summary>
/// 使用者服務實作
/// 負責使用者相關的商業邏輯（包含 Identity 操作的封裝）
/// </summary>
public class UserService : IUserService
{
    private readonly UserManager<AppUser> _userManager;
    private readonly SignInManager<AppUser> _signInManager;
    private readonly ILogger<UserService> _logger;

    public UserService(
        UserManager<AppUser> userManager,
        SignInManager<AppUser> signInManager,
        ILogger<UserService> logger)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _logger = logger;
    }

    /// <summary>
    /// 取得使用者基本資訊（姓名、電話）
    /// </summary>
    public async Task<(string FullName, string PhoneNumber)> GetUserBasicInfoAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            return (string.Empty, string.Empty);

        return (user.FullName ?? string.Empty, user.PhoneNumber ?? string.Empty);
    }

    /// <summary>
    /// 註冊新使用者
    /// </summary>
    public async Task<(bool Success, string UserId, IEnumerable<string> Errors)> RegisterAsync(RegisterViewModel model)
    {
        var user = new AppUser
        {
            UserName = model.Email,
            Email = model.Email,
            FullName = model.FullName,
            PhoneNumber = model.PhoneNumber,
            Birthday = model.Birthday,
            Gender = model.Gender
        };

        var result = await _userManager.CreateAsync(user, model.Password);

        if (result.Succeeded)
        {
            await _userManager.AddToRoleAsync(user, "User");
            await _signInManager.SignInAsync(user, isPersistent: false);
            _logger.LogInformation($"使用者 {user.Email} 註冊成功");
            return (true, user.Id, Enumerable.Empty<string>());
        }

        return (false, string.Empty, result.Errors.Select(e => e.Description));
    }

    /// <summary>
    /// 登入使用者，回傳詳細的登入狀態供前端顯示對應提示
    /// </summary>
    public async Task<(bool Success, string? FullName, bool IsLockedOut, int LockoutMinutes, int RemainingAttempts)> LoginAsync(string email, string password, bool rememberMe)
    {
        // lockoutOnFailure: true（CWE-307 防護的關鍵開關）
        // 設為 true 時，每次密碼錯誤會遞增 AspNetUsers.AccessFailedCount，
        // 達到 Program.cs 設定的 MaxFailedAccessAttempts（5 次）後觸發帳號鎖定。
        // 設為 false 則永遠不鎖定，等同停用 Lockout 功能。
        // 登入成功時 AccessFailedCount 自動歸零。
        var result = await _signInManager.PasswordSignInAsync(email, password, rememberMe, lockoutOnFailure: true);

        // ─── 登入成功 ─────────────────────────────────────
        if (result.Succeeded)
        {
            var user = await _userManager.FindByEmailAsync(email);
            return (true, user?.FullName, false, 0, 0);
        }

        // ─── 帳號被鎖定 ───────────────────────────────────
        // 計算剩餘鎖定時間，供前端顯示「請在 X 分鐘後重試」
        if (result.IsLockedOut)
        {
            var user = await _userManager.FindByEmailAsync(email);
            var lockoutEnd = user != null ? await _userManager.GetLockoutEndDateAsync(user) : null;
            var remainingMinutes = lockoutEnd.HasValue
                ? (int)Math.Ceiling((lockoutEnd.Value - DateTimeOffset.UtcNow).TotalMinutes)
                : 0;

            _logger.LogWarning("帳號 {Email} 因連續登入失敗已被鎖定，剩餘 {Minutes} 分鐘", email, remainingMinutes);
            return (false, null, true, Math.Max(remainingMinutes, 1), 0);
        }

        // ─── 密碼錯誤（未鎖定）─────────────────────────────
        // 計算剩餘可嘗試次數，供前端顯示「還剩 X 次機會」
        var failedUser = await _userManager.FindByEmailAsync(email);
        var remainingAttempts = 0;

        if (failedUser != null)
        {
            var maxAttempts = _userManager.Options.Lockout.MaxFailedAccessAttempts;
            var failedCount = await _userManager.GetAccessFailedCountAsync(failedUser);
            remainingAttempts = maxAttempts - failedCount;
        }

        return (false, null, false, 0, remainingAttempts);
    }

    /// <summary>
    /// 登出使用者
    /// </summary>
    public async Task LogoutAsync()
    {
        await _signInManager.SignOutAsync();
    }

    /// <summary>
    /// 取得使用者個人資料（用於編輯頁面）
    /// </summary>
    public async Task<EditProfileViewModel?> GetEditProfileAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) return null;

        return new EditProfileViewModel
        {
            FullName = user.FullName ?? "",
            Email = user.Email ?? "",
            PhoneNumber = user.PhoneNumber ?? "",
            Birthday = user.Birthday,
            Gender = user.Gender,
            RegisteredAt = user.RegisteredAt
        };
    }

    /// <summary>
    /// 更新使用者個人資料
    /// </summary>
    public async Task<(bool Success, IEnumerable<string> Errors)> UpdateProfileAsync(string userId, EditProfileViewModel model)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            return (false, new[] { "找不到使用者" });

        user.FullName = model.FullName;
        user.Email = model.Email;
        user.UserName = model.Email;
        user.PhoneNumber = model.PhoneNumber;
        user.Birthday = model.Birthday;
        user.Gender = model.Gender;

        var result = await _userManager.UpdateAsync(user);

        if (result.Succeeded)
            return (true, Enumerable.Empty<string>());

        return (false, result.Errors.Select(e => e.Description));
    }

    /// <summary>
    /// 取得使用者帳戶摘要資訊
    /// </summary>
    public async Task<(string FullName, string Email, DateTime RegisteredAt)?> GetAccountSummaryAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) return null;

        return (user.FullName ?? "訪客", user.Email ?? "", user.RegisteredAt);
    }

    /// <summary>
    /// 取得所有使用者及其角色資訊
    /// </summary>
    public async Task<List<UserManagementViewModel>> GetAllUsersWithRolesAsync()
    {
        var users = await _userManager.Users.ToListAsync();
        var userViewModels = new List<UserManagementViewModel>();

        foreach (var user in users)
        {
            var roles = await _userManager.GetRolesAsync(user);

            userViewModels.Add(new UserManagementViewModel
            {
                UserId = user.Id,
                Email = user.Email ?? "",
                FullName = user.FullName ?? "",
                PhoneNumber = user.PhoneNumber ?? "",
                RegisteredAt = user.RegisteredAt,
                IsAdmin = roles.Contains("Admin"),
                Roles = roles.ToList()
            });
        }

        return userViewModels;
    }

    /// <summary>
    /// 切換使用者的管理員角色（指派或移除）
    /// </summary>
    public async Task<(bool Success, string Message)> ToggleAdminRoleAsync(string userId, string currentAdminId)
    {
        if (string.IsNullOrEmpty(userId))
            return (false, "無效的使用者 ID");

        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            return (false, "找不到指定的使用者");

        if (currentAdminId == userId)
            return (false, "您不能移除自己的管理員權限");

        var isAdmin = await _userManager.IsInRoleAsync(user, "Admin");

        if (isAdmin)
        {
            var result = await _userManager.RemoveFromRoleAsync(user, "Admin");
            if (result.Succeeded)
            {
                _logger.LogInformation($"使用者 {user.Email} 的 Admin 角色已被移除");
                return (true, $"已移除 {user.Email} 的管理員權限");
            }
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            _logger.LogError($"移除 {user.Email} 的 Admin 角色失敗：{errors}");
            return (false, $"移除管理員權限失敗：{errors}");
        }
        else
        {
            var result = await _userManager.AddToRoleAsync(user, "Admin");
            if (result.Succeeded)
            {
                _logger.LogInformation($"使用者 {user.Email} 已被指派 Admin 角色");
                return (true, $"已將 {user.Email} 設為管理員");
            }
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            _logger.LogError($"指派 {user.Email} 的 Admin 角色失敗：{errors}");
            return (false, $"指派管理員權限失敗：{errors}");
        }
    }

    /// <summary>
    /// 更新使用者密碼
    /// </summary>
    public async Task<(bool Success, string Message)> ChangePasswordAsync(string userId, string currentPassword, string newPassword)
    {
        if (string.IsNullOrEmpty(userId))
            return (false, "無效的使用者 ID");

        if (string.IsNullOrEmpty(currentPassword))
            return (false, "請輸入目前密碼");

        if (string.IsNullOrEmpty(newPassword))
            return (false, "請輸入新密碼");

        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            return (false, "找不到指定的使用者");

        var result = await _userManager.ChangePasswordAsync(user, currentPassword, newPassword);

        if (result.Succeeded)
        {
            _logger.LogInformation($"使用者 {user.Email} 已成功更新密碼");
            return (true, "密碼更新成功");
        }

        var errors = string.Join(", ", result.Errors.Select(e => e.Description));
        _logger.LogWarning($"使用者 {user.Email} 更新密碼失敗：{errors}");

        if (result.Errors.Any(e => e.Code == "PasswordMismatch"))
            return (false, "目前密碼不正確");

        return (false, $"密碼更新失敗：{errors}");
    }
}
