using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using System.Security.Claims;

namespace MusicShop.Web.Controllers.Api;

/// <summary>
/// API 控制器基底類別，提供共用的使用者輔助方法
/// 套用 "api" 速率限制策略（每分鐘 30 次）
/// </summary>
[EnableRateLimiting("api")]
public abstract class BaseApiController : ControllerBase
{
    /// <summary>
    /// 取得當前使用者 ID（可能為 null）
    /// </summary>
    protected string? GetCurrentUserId()
    {
        return User.FindFirstValue(ClaimTypes.NameIdentifier);
    }
}
