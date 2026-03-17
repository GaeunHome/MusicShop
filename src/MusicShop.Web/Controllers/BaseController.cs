using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace MusicShop.Controllers;

/// <summary>
/// 控制器基底類別，提供共用的輔助方法
/// </summary>
public abstract class BaseController : Controller
{
    /// <summary>
    /// 取得已授權的使用者 ID，若未登入則拋出異常
    /// </summary>
    protected string GetAuthorizedUserId()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
            throw new UnauthorizedAccessException("使用者未登入");
        return userId;
    }
}
