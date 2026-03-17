using System.Net;

namespace MusicShop.Web.Infrastructure;

/// <summary>
/// 全域例外處理中間件
/// 捕獲未處理的例外，記錄錯誤並回傳使用者友善的錯誤頁面
/// </summary>
public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;

    /// <summary>
    /// 初始化 <see cref="GlobalExceptionMiddleware"/> 的新執行個體
    /// </summary>
    /// <param name="next">請求管線中的下一個中間件委派</param>
    /// <param name="logger">日誌記錄器</param>
    public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    /// <summary>
    /// 處理 HTTP 請求，捕獲未處理的例外並進行適當的錯誤回應
    /// </summary>
    /// <param name="context">目前的 HTTP 內容</param>
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "未授權的存取嘗試：{Path}", context.Request.Path);
            context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
            context.Response.Redirect("/Account/AccessDenied");
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "找不到請求的資源：{Path}", context.Request.Path);
            context.Response.StatusCode = (int)HttpStatusCode.NotFound;
            context.Response.Redirect("/Home/Error?statusCode=404");
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "業務邏輯錯誤：{Path}", context.Request.Path);

            if (!context.Response.HasStarted)
            {
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                context.Response.Redirect("/Home/Error?statusCode=400");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "未預期的系統錯誤：{Path}", context.Request.Path);

            if (!context.Response.HasStarted)
            {
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                context.Response.Redirect("/Home/Error?statusCode=500");
            }
        }
    }
}

/// <summary>
/// <see cref="GlobalExceptionMiddleware"/> 的擴充方法，
/// 提供簡潔的中間件註冊語法
/// </summary>
public static class GlobalExceptionMiddlewareExtensions
{
    /// <summary>
    /// 將全域例外處理中間件加入應用程式的請求管線
    /// </summary>
    /// <param name="app">應用程式建構器</param>
    /// <returns>應用程式建構器，支援鏈式呼叫</returns>
    public static IApplicationBuilder UseGlobalExceptionHandler(this IApplicationBuilder app)
    {
        return app.UseMiddleware<GlobalExceptionMiddleware>();
    }
}
