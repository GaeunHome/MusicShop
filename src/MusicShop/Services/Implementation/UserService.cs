using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MusicShop.Models;
using MusicShop.Services.Interface;
using MusicShop.ViewModels.Admin;

namespace MusicShop.Services.Implementation;

/// <summary>
/// 使用者服務實作
/// 負責使用者相關的商業邏輯
/// </summary>
public class UserService : IUserService
{
    private readonly UserManager<AppUser> _userManager;
    private readonly ILogger<UserService> _logger;

    public UserService(
        UserManager<AppUser> userManager,
        ILogger<UserService> logger)
    {
        _userManager = userManager;
        _logger = logger;
    }

    /// <summary>
    /// 取得所有使用者及其角色資訊
    /// </summary>
    public async Task<List<UserManagementViewModel>> GetAllUsersWithRolesAsync()
    {
        // 取得所有使用者
        var users = await _userManager.Users.ToListAsync();

        // 建立 ViewModel 列表
        var userViewModels = new List<UserManagementViewModel>();

        foreach (var user in users)
        {
            // 取得使用者的所有角色
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
        // 檢查 userId 是否有效
        if (string.IsNullOrEmpty(userId))
        {
            return (false, "無效的使用者 ID");
        }

        // 找到指定的使用者
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return (false, "找不到指定的使用者");
        }

        // 檢查目前登入的管理員是否試圖移除自己的管理員權限
        if (currentAdminId == userId)
        {
            return (false, "您不能移除自己的管理員權限");
        }

        // 檢查使用者目前是否為管理員
        var isAdmin = await _userManager.IsInRoleAsync(user, "Admin");

        if (isAdmin)
        {
            // 移除 Admin 角色
            var result = await _userManager.RemoveFromRoleAsync(user, "Admin");
            if (result.Succeeded)
            {
                _logger.LogInformation($"使用者 {user.Email} 的 Admin 角色已被移除");
                return (true, $"已移除 {user.Email} 的管理員權限");
            }
            else
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                _logger.LogError($"移除 {user.Email} 的 Admin 角色失敗：{errors}");
                return (false, $"移除管理員權限失敗：{errors}");
            }
        }
        else
        {
            // 指派 Admin 角色
            var result = await _userManager.AddToRoleAsync(user, "Admin");
            if (result.Succeeded)
            {
                _logger.LogInformation($"使用者 {user.Email} 已被指派 Admin 角色");
                return (true, $"已將 {user.Email} 設為管理員");
            }
            else
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                _logger.LogError($"指派 {user.Email} 的 Admin 角色失敗：{errors}");
                return (false, $"指派管理員權限失敗：{errors}");
            }
        }
    }

    /// <summary>
    /// 更新使用者密碼
    /// </summary>
    public async Task<(bool Success, string Message)> ChangePasswordAsync(string userId, string currentPassword, string newPassword)
    {
        // 參數驗證
        if (string.IsNullOrEmpty(userId))
        {
            return (false, "無效的使用者 ID");
        }

        if (string.IsNullOrEmpty(currentPassword))
        {
            return (false, "請輸入目前密碼");
        }

        if (string.IsNullOrEmpty(newPassword))
        {
            return (false, "請輸入新密碼");
        }

        // 找到使用者
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return (false, "找不到指定的使用者");
        }

        // 使用 UserManager 更改密碼（會自動驗證目前密碼）
        var result = await _userManager.ChangePasswordAsync(user, currentPassword, newPassword);

        if (result.Succeeded)
        {
            _logger.LogInformation($"使用者 {user.Email} 已成功更新密碼");
            return (true, "密碼更新成功");
        }
        else
        {
            // 取得錯誤訊息
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            _logger.LogWarning($"使用者 {user.Email} 更新密碼失敗：{errors}");

            // 檢查是否為目前密碼錯誤
            if (result.Errors.Any(e => e.Code == "PasswordMismatch"))
            {
                return (false, "目前密碼不正確");
            }

            return (false, $"密碼更新失敗：{errors}");
        }
    }
}
