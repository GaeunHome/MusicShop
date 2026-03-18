using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace MusicShop.Web.Controllers.Api;

/// <summary>
/// API 控制器基底類別，提供共用的使用者輔助方法
/// </summary>
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
