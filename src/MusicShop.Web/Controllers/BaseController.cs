using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace MusicShop.Controllers;

/// <summary>
/// 控制器基底類別，提供共用的使用者輔助方法
/// </summary>
public abstract class BaseController : Controller
{
    /// <summary>
    /// 取得當前使用者 ID（可能為 null，適用於不強制登入的頁面）
    /// </summary>
    protected string? GetCurrentUserId()
    {
        return User.FindFirstValue(ClaimTypes.NameIdentifier);
    }

    /// <summary>
    /// 取得已授權的使用者 ID，若未登入則拋出異常（適用於 [Authorize] 頁面）
    /// </summary>
    protected string GetAuthorizedUserId()
    {
        var userId = GetCurrentUserId();
        if (string.IsNullOrEmpty(userId))
            throw new UnauthorizedAccessException("使用者未登入");
        return userId;
    }
}
