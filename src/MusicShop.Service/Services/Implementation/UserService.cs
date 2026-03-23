using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MusicShop.Data;
using MusicShop.Data.Entities;
using MusicShop.Library.Enums;
using MusicShop.Library.Helpers;
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
    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger<UserService> _logger;

    /// <summary>
    /// 密碼歷史保留筆數（防止重複使用最近 N 組密碼）
    /// </summary>
    private const int PasswordHistoryCount = 1;

    public UserService(
        UserManager<AppUser> userManager,
        SignInManager<AppUser> signInManager,
        ApplicationDbContext dbContext,
        ILogger<UserService> logger)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _dbContext = dbContext;
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
    /// 註冊後不會自動登入，需先驗證 Email
    /// </summary>
    public async Task<(bool Success, string UserId, IEnumerable<string> Errors)> RegisterAsync(RegisterViewModel model)
    {
        // Email 安全性驗證：檢查拋棄式信箱與 MX 記錄
        if (EmailValidationHelper.IsDisposableEmail(model.Email))
            return (false, string.Empty, new[] { "不支援使用拋棄式信箱註冊，請使用正式的 Email 地址" });

        if (!await EmailValidationHelper.HasValidMxRecordAsync(model.Email))
            return (false, string.Empty, new[] { "此 Email 網域無效或無法接收郵件，請確認您的 Email 地址" });

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

            // 記錄初始密碼到歷史
            await SavePasswordHistoryAsync(user.Id, user.PasswordHash!);

            _logger.LogInformation("使用者 {Email} 註冊成功，等待 Email 驗證", user.Email);
            return (true, user.Id, Enumerable.Empty<string>());
        }

        return (false, string.Empty, result.Errors.Select(e => e.Description));
    }

    /// <summary>
    /// 登入使用者，回傳詳細的登入狀態供前端顯示對應提示
    /// </summary>
    public async Task<(bool Success, string? FullName, bool IsLockedOut, int LockoutMinutes, int RemainingAttempts, bool RequiresTwoFactor)> LoginAsync(string email, string password, bool rememberMe)
    {
        // 先檢查 Email 是否已驗證
        var checkUser = await _userManager.FindByEmailAsync(email);
        if (checkUser != null && !await _userManager.IsEmailConfirmedAsync(checkUser))
        {
            _logger.LogWarning("使用者 {Email} 嘗試登入但 Email 尚未驗證", email);
            return (false, null, false, 0, -1, false); // -1 表示未驗證
        }

        // lockoutOnFailure: true（CWE-307 防護的關鍵開關）
        // 設為 true 時，每次密碼錯誤會遞增 AspNetUsers.AccessFailedCount，
        // 達到 Program.cs 設定的 MaxFailedAccessAttempts（3 次）後觸發帳號鎖定。
        // 設為 false 則永遠不鎖定，等同停用 Lockout 功能。
        // 登入成功時 AccessFailedCount 自動歸零。
        var result = await _signInManager.PasswordSignInAsync(email, password, rememberMe, lockoutOnFailure: true);

        // ─── 登入成功 ─────────────────────────────────────
        if (result.Succeeded)
        {
            var user = await _userManager.FindByEmailAsync(email);
            return (true, user?.FullName, false, 0, 0, false);
        }

        // ─── 需要兩步驟驗證 ─────────────────────────────────
        if (result.RequiresTwoFactor)
        {
            _logger.LogInformation("使用者 {Email} 密碼驗證通過，等待兩步驟驗證", email);
            return (false, null, false, 0, 0, true);
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
            return (false, null, true, Math.Max(remainingMinutes, 1), 0, false);
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

        return (false, null, false, 0, remainingAttempts, false);
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
                EmailConfirmed = user.EmailConfirmed,
                TwoFactorEnabled = user.TwoFactorEnabled,
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
                _logger.LogInformation("使用者 {Email} 的 Admin 角色已被移除", user.Email);
                return (true, $"已移除 {user.Email} 的管理員權限");
            }
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            _logger.LogError("移除 {Email} 的 Admin 角色失敗：{Errors}", user.Email, errors);
            return (false, $"移除管理員權限失敗：{errors}");
        }
        else
        {
            var result = await _userManager.AddToRoleAsync(user, "Admin");
            if (result.Succeeded)
            {
                _logger.LogInformation("使用者 {Email} 已被指派 Admin 角色", user.Email);
                return (true, $"已將 {user.Email} 設為管理員");
            }
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            _logger.LogError("指派 {Email} 的 Admin 角色失敗：{Errors}", user.Email, errors);
            return (false, $"指派管理員權限失敗：{errors}");
        }
    }

    /// <summary>
    /// 管理員手動確認使用者 Email
    /// </summary>
    public async Task<(bool Success, string Message)> AdminConfirmEmailAsync(string userId)
    {
        if (string.IsNullOrEmpty(userId))
            return (false, "無效的使用者 ID");

        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            return (false, "找不到指定的使用者");

        if (user.EmailConfirmed)
            return (false, $"{user.Email} 的 Email 已經驗證過了");

        // 產生 Token 後立即確認，繞過寄信流程
        var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
        var result = await _userManager.ConfirmEmailAsync(user, token);

        if (result.Succeeded)
        {
            _logger.LogInformation("管理員手動確認使用者 Email：{Email}", user.Email);
            return (true, $"已手動確認 {user.Email} 的 Email");
        }

        var errors = string.Join(", ", result.Errors.Select(e => e.Description));
        return (false, $"確認 Email 失敗：{errors}");
    }

    /// <summary>
    /// 更新使用者密碼（含密碼歷史檢查）
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

        // 檢查新密碼是否與最近使用過的密碼重複
        var isReused = await IsPasswordReusedAsync(user, newPassword);
        if (isReused)
            return (false, $"新密碼不能與最近 {PasswordHistoryCount} 次使用的密碼相同");

        var result = await _userManager.ChangePasswordAsync(user, currentPassword, newPassword);

        if (result.Succeeded)
        {
            // 重新載入使用者以取得新的 PasswordHash
            await _userManager.UpdateAsync(user);
            var updatedUser = await _userManager.FindByIdAsync(userId);
            await SavePasswordHistoryAsync(userId, updatedUser!.PasswordHash!);

            _logger.LogInformation("使用者 {Email} 已成功更新密碼", user.Email);
            return (true, "密碼更新成功");
        }

        var errors = string.Join(", ", result.Errors.Select(e => e.Description));
        _logger.LogWarning("使用者 {Email} 更新密碼失敗：{Errors}", user.Email, errors);

        if (result.Errors.Any(e => e.Code == "PasswordMismatch"))
            return (false, "目前密碼不正確");

        return (false, $"密碼更新失敗：{errors}");
    }

    // ==================== Email 驗證 ====================

    public async Task<(string? Token, string? UserId)> GenerateEmailConfirmationTokenAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) return (null, null);

        var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
        return (token, user.Id);
    }

    public async Task<(bool Success, string Message)> ConfirmEmailAsync(string userId, string token)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            return (false, "無效的驗證連結");

        if (await _userManager.IsEmailConfirmedAsync(user))
            return (true, "Email 已驗證過，您可以直接登入");

        var result = await _userManager.ConfirmEmailAsync(user, token);

        if (result.Succeeded)
        {
            // 驗證成功後自動登入
            await _signInManager.SignInAsync(user, isPersistent: false);
            _logger.LogInformation("使用者 {Email} 已完成 Email 驗證", user.Email);
            return (true, "Email 驗證成功！歡迎加入 MusicShop");
        }

        _logger.LogWarning("使用者 {Email} Email 驗證失敗", user.Email);
        return (false, "驗證連結已失效，請重新申請");
    }

    public async Task<(string? Token, string? UserId, string? Email)> ResendConfirmationTokenAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) return (null, null, null);

        if (await _userManager.IsEmailConfirmedAsync(user))
            return (null, null, null);

        var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
        return (token, user.Id, user.Email);
    }

    public async Task<bool> IsEmailConfirmedAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) return false;
        return await _userManager.IsEmailConfirmedAsync(user);
    }

    // ==================== 忘記密碼 ====================

    public async Task<(string? Token, string? UserId)> GeneratePasswordResetTokenAsync(string email)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user == null)
        {
            // 不回傳具體錯誤，避免帳號列舉攻擊
            return (null, null);
        }

        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        _logger.LogInformation("使用者 {Email} 申請密碼重設", email);
        return (token, user.Id);
    }

    public async Task<(bool Success, string Message)> ResetPasswordAsync(string userId, string token, string newPassword)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            return (false, "無效的重設連結");

        // 檢查密碼歷史
        var isReused = await IsPasswordReusedAsync(user, newPassword);
        if (isReused)
            return (false, $"新密碼不能與最近 {PasswordHistoryCount} 次使用的密碼相同");

        var result = await _userManager.ResetPasswordAsync(user, token, newPassword);

        if (result.Succeeded)
        {
            // 記錄新密碼歷史
            var updatedUser = await _userManager.FindByIdAsync(userId);
            await SavePasswordHistoryAsync(userId, updatedUser!.PasswordHash!);

            _logger.LogInformation("使用者 {Email} 已成功重設密碼", user.Email);
            return (true, "密碼重設成功，請使用新密碼登入");
        }

        var errors = string.Join(", ", result.Errors.Select(e => e.Description));
        _logger.LogWarning("使用者 {Email} 重設密碼失敗：{Errors}", user.Email, errors);

        if (result.Errors.Any(e => e.Code == "InvalidToken"))
            return (false, "重設連結已失效，請重新申請");

        return (false, $"密碼重設失敗：{errors}");
    }

    // ==================== 兩步驟驗證 (2FA) ====================

    /// <summary>
    /// 取得使用者 2FA 設定狀態
    /// </summary>
    public async Task<TwoFactorStatusViewModel> GetTwoFactorStatusAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId)
            ?? throw new InvalidOperationException("找不到使用者");

        var hasAuthenticator = await _userManager.GetAuthenticatorKeyAsync(user) != null;

        return new TwoFactorStatusViewModel
        {
            IsEnabled = await _userManager.GetTwoFactorEnabledAsync(user),
            PreferredMethod = user.PreferredTwoFactorMethod.ToString(),
            HasAuthenticator = hasAuthenticator
        };
    }

    /// <summary>
    /// 開始設定 TOTP 驗證器（重設金鑰並產生 AuthenticatorUri）
    /// 僅在 GET 時呼叫，POST 驗證失敗時應使用 GetAuthenticatorSetupAsync 避免重設金鑰
    /// </summary>
    public async Task<SetupAuthenticatorViewModel> SetupAuthenticatorAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId)
            ?? throw new InvalidOperationException("找不到使用者");

        // 重設驗證器金鑰（產生新金鑰）
        await _userManager.ResetAuthenticatorKeyAsync(user);

        return await BuildAuthenticatorViewModelAsync(user);
    }

    /// <summary>
    /// 取得現有的 TOTP 驗證器設定（不重設金鑰）
    /// 用於 POST 驗證失敗時重新顯示 QR Code，避免金鑰被重設導致使用者需重新掃描
    /// </summary>
    public async Task<SetupAuthenticatorViewModel> GetAuthenticatorSetupAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId)
            ?? throw new InvalidOperationException("找不到使用者");

        return await BuildAuthenticatorViewModelAsync(user);
    }

    /// <summary>
    /// 組建驗證器設定 ViewModel（共用邏輯）
    /// </summary>
    private async Task<SetupAuthenticatorViewModel> BuildAuthenticatorViewModelAsync(AppUser user)
    {
        var unformattedKey = await _userManager.GetAuthenticatorKeyAsync(user)
            ?? throw new InvalidOperationException("無法取得驗證器金鑰");

        var formattedKey = FormatAuthenticatorKey(unformattedKey);

        var email = await _userManager.GetEmailAsync(user) ?? "user";
        var authenticatorUri = $"otpauth://totp/MusicShop:{email}?secret={unformattedKey}&issuer=MusicShop&digits=6";

        return new SetupAuthenticatorViewModel
        {
            SharedKey = formattedKey,
            AuthenticatorUri = authenticatorUri
        };
    }

    /// <summary>
    /// 驗證並啟用 TOTP 驗證器
    /// </summary>
    public async Task<(bool Success, string Message)> EnableAuthenticatorAsync(string userId, string verificationCode)
    {
        var user = await _userManager.FindByIdAsync(userId)
            ?? throw new InvalidOperationException("找不到使用者");

        // 移除空格與連字號
        var code = verificationCode.Replace(" ", "").Replace("-", "");

        var isValid = await _userManager.VerifyTwoFactorTokenAsync(
            user,
            _userManager.Options.Tokens.AuthenticatorTokenProvider,
            code);

        if (!isValid)
            return (false, "驗證碼不正確，請重新輸入");

        // 啟用 2FA
        await _userManager.SetTwoFactorEnabledAsync(user, true);
        user.PreferredTwoFactorMethod = TwoFactorMethod.Authenticator;
        await _userManager.UpdateAsync(user);

        _logger.LogInformation("使用者 {Email} 已啟用 TOTP 兩步驟驗證", user.Email);
        return (true, "兩步驟驗證已啟用");
    }

    /// <summary>
    /// 產生 Email 2FA 設定驗證碼
    /// </summary>
    public async Task<(bool Success, string Message, string? Code)> GenerateEmailTwoFactorSetupCodeAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId)
            ?? throw new InvalidOperationException("找不到使用者");

        if (!await _userManager.IsEmailConfirmedAsync(user))
            return (false, "請先驗證您的 Email", null);

        var code = await _userManager.GenerateTwoFactorTokenAsync(user, "Email");
        return (true, "驗證碼已產生", code);
    }

    /// <summary>
    /// 確認 Email 2FA 設定
    /// </summary>
    public async Task<(bool Success, string Message)> EnableEmailTwoFactorAsync(string userId, string verificationCode)
    {
        var user = await _userManager.FindByIdAsync(userId)
            ?? throw new InvalidOperationException("找不到使用者");

        var isValid = await _userManager.VerifyTwoFactorTokenAsync(user, "Email", verificationCode);

        if (!isValid)
            return (false, "驗證碼不正確或已過期，請重新取得");

        // 啟用 2FA
        await _userManager.SetTwoFactorEnabledAsync(user, true);
        user.PreferredTwoFactorMethod = TwoFactorMethod.Email;
        await _userManager.UpdateAsync(user);

        _logger.LogInformation("使用者 {Email} 已啟用 Email 兩步驟驗證", user.Email);
        return (true, "兩步驟驗證已啟用");
    }

    /// <summary>
    /// 停用 2FA
    /// </summary>
    public async Task<(bool Success, string Message)> DisableTwoFactorAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId)
            ?? throw new InvalidOperationException("找不到使用者");

        if (!await _userManager.GetTwoFactorEnabledAsync(user))
            return (false, "兩步驟驗證尚未啟用");

        await _userManager.SetTwoFactorEnabledAsync(user, false);
        user.PreferredTwoFactorMethod = TwoFactorMethod.None;
        await _userManager.UpdateAsync(user);

        // 重設驗證器金鑰
        await _userManager.ResetAuthenticatorKeyAsync(user);

        _logger.LogInformation("使用者 {Email} 已停用兩步驟驗證", user.Email);
        return (true, "兩步驟驗證已停用");
    }

    /// <summary>
    /// 取得等待 2FA 驗證的使用者資訊
    /// </summary>
    public async Task<(string? UserId, string? PreferredMethod, string? Email)> GetTwoFactorUserInfoAsync()
    {
        var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();
        if (user == null)
            return (null, null, null);

        return (user.Id, user.PreferredTwoFactorMethod.ToString(), user.Email);
    }

    /// <summary>
    /// 使用 TOTP 驗證碼完成 2FA 登入
    /// </summary>
    public async Task<(bool Success, string? FullName, bool IsLockedOut, int LockoutMinutes)> TwoFactorAuthenticatorLoginAsync(string code, bool rememberMe)
    {
        var cleanCode = code.Replace(" ", "").Replace("-", "");
        var result = await _signInManager.TwoFactorAuthenticatorSignInAsync(cleanCode, rememberMe, rememberClient: false);

        return await HandleTwoFactorResultAsync(result);
    }

    /// <summary>
    /// 使用 Email 驗證碼完成 2FA 登入
    /// </summary>
    public async Task<(bool Success, string? FullName, bool IsLockedOut, int LockoutMinutes)> TwoFactorEmailLoginAsync(string code, bool rememberMe)
    {
        var result = await _signInManager.TwoFactorSignInAsync("Email", code, rememberMe, rememberClient: false);

        return await HandleTwoFactorResultAsync(result);
    }

    /// <summary>
    /// 產生 Email 2FA 登入驗證碼
    /// </summary>
    public async Task<(bool Success, string Message, string? Code)> GenerateTwoFactorEmailCodeAsync()
    {
        var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();
        if (user == null)
            return (false, "找不到待驗證的使用者，請重新登入", null);

        var code = await _userManager.GenerateTwoFactorTokenAsync(user, "Email");
        return (true, "驗證碼已產生", code);
    }

    // ==================== 2FA 私有輔助方法 ====================

    /// <summary>
    /// 處理 2FA 登入結果
    /// 注意：2FA 登入成功後 SignInManager 的 TwoFactorUser 會清空，
    /// 因此 FullName 無法在此取得，由 Controller 層透過 TempData 處理歡迎訊息
    /// </summary>
    private Task<(bool Success, string? FullName, bool IsLockedOut, int LockoutMinutes)> HandleTwoFactorResultAsync(Microsoft.AspNetCore.Identity.SignInResult result)
    {
        if (result.Succeeded)
            return Task.FromResult((true, (string?)null, false, 0));

        if (result.IsLockedOut)
        {
            var lockoutMinutes = (int)_userManager.Options.Lockout.DefaultLockoutTimeSpan.TotalMinutes;
            return Task.FromResult((false, (string?)null, true, lockoutMinutes));
        }

        return Task.FromResult((false, (string?)null, false, 0));
    }

    /// <summary>
    /// 格式化驗證器金鑰（每 4 字元加一個空格）
    /// </summary>
    private static string FormatAuthenticatorKey(string unformattedKey)
    {
        var result = new System.Text.StringBuilder();
        var currentPosition = 0;

        while (currentPosition + 4 < unformattedKey.Length)
        {
            result.Append(unformattedKey.AsSpan(currentPosition, 4)).Append(' ');
            currentPosition += 4;
        }

        if (currentPosition < unformattedKey.Length)
            result.Append(unformattedKey.AsSpan(currentPosition));

        return result.ToString().ToUpperInvariant();
    }

    // ==================== 密碼歷史私有方法 ====================

    /// <summary>
    /// 檢查新密碼是否與最近使用過的密碼重複
    /// </summary>
    private async Task<bool> IsPasswordReusedAsync(AppUser user, string newPassword)
    {
        var passwordHasher = new PasswordHasher<AppUser>();

        // 取得最近 N 筆密碼歷史
        var recentPasswords = await _dbContext.PasswordHistories
            .Where(ph => ph.UserId == user.Id)
            .OrderByDescending(ph => ph.CreatedAt)
            .Take(PasswordHistoryCount)
            .ToListAsync();

        // 比對每一筆歷史密碼
        foreach (var history in recentPasswords)
        {
            var verifyResult = passwordHasher.VerifyHashedPassword(user, history.PasswordHash, newPassword);
            if (verifyResult != PasswordVerificationResult.Failed)
                return true;
        }

        return false;
    }

    /// <summary>
    /// 儲存密碼歷史記錄
    /// </summary>
    private async Task SavePasswordHistoryAsync(string userId, string passwordHash)
    {
        _dbContext.PasswordHistories.Add(new PasswordHistory
        {
            UserId = userId,
            PasswordHash = passwordHash,
            CreatedAt = DateTime.UtcNow
        });
        await _dbContext.SaveChangesAsync();

        // 清理超出保留筆數的舊記錄
        var oldRecords = await _dbContext.PasswordHistories
            .Where(ph => ph.UserId == userId)
            .OrderByDescending(ph => ph.CreatedAt)
            .Skip(PasswordHistoryCount)
            .ToListAsync();

        if (oldRecords.Count > 0)
        {
            _dbContext.PasswordHistories.RemoveRange(oldRecords);
            await _dbContext.SaveChangesAsync();
        }
    }

    // ==================== 社群登入 ====================

    public async Task<(bool Success, string? FullName, bool IsNewUser)> ExternalLoginAsync(
        string provider, string providerKey, string? email, string? name)
    {
        // 先嘗試透過外部登入資訊找到已關聯的使用者
        var user = await _userManager.FindByLoginAsync(provider, providerKey);
        var isNewUser = false;

        if (user == null && !string.IsNullOrEmpty(email))
        {
            // 以 Email 查詢是否已有帳號
            user = await _userManager.FindByEmailAsync(email);

            if (user != null)
            {
                // 帳號已存在，建立外部登入關聯
                var addLoginResult = await _userManager.AddLoginAsync(user,
                    new UserLoginInfo(provider, providerKey, provider));
                if (!addLoginResult.Succeeded)
                {
                    _logger.LogWarning("無法關聯 {Provider} 登入至現有帳號 {Email}", provider, email);
                    return (false, null, false);
                }
                _logger.LogInformation("已將 {Provider} 登入關聯至現有帳號 {Email}", provider, email);
            }
        }

        if (user == null)
        {
            // 建立新帳號
            user = new AppUser
            {
                UserName = email ?? $"{provider}_{providerKey}",
                Email = email,
                FullName = name ?? "使用者",
                EmailConfirmed = !string.IsNullOrEmpty(email) // 社群登入的 Email 已經過驗證
            };

            var createResult = await _userManager.CreateAsync(user);
            if (!createResult.Succeeded)
            {
                _logger.LogWarning("社群登入建立帳號失敗：{Errors}",
                    string.Join(", ", createResult.Errors.Select(e => e.Description)));
                return (false, null, false);
            }

            await _userManager.AddLoginAsync(user,
                new UserLoginInfo(provider, providerKey, provider));
            await _userManager.AddToRoleAsync(user, "User");

            isNewUser = true;
            _logger.LogInformation("透過 {Provider} 建立新帳號：{Email}", provider, email ?? user.UserName);
        }

        // 登入
        await _signInManager.SignInAsync(user, isPersistent: false);
        _logger.LogInformation("使用者 {Email} 透過 {Provider} 登入成功", user.Email ?? user.UserName, provider);

        return (true, user.FullName, isNewUser);
    }

    public async Task<IEnumerable<string>> GetExternalAuthenticationSchemesAsync()
    {
        var schemes = await _signInManager.GetExternalAuthenticationSchemesAsync();
        return schemes.Select(s => s.Name);
    }
}
