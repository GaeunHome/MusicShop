using MusicShop.Service.Services.Interfaces;

namespace MusicShop.Web.Infrastructure;

/// <summary>
/// 維護模式中間件
/// 當系統參數 site.maintenance_mode 為 true 時，前台請求導向維護頁面
/// 排除後台（/Admin）與登入頁（/Account/Login），讓管理員可以登入後台關閉維護模式
/// </summary>
public class MaintenanceModeMiddleware
{
    private readonly RequestDelegate _next;

    /// <summary>不受維護模式影響的路徑前綴</summary>
    private static readonly string[] ExcludedPrefixes =
    [
        "/Admin",
        "/Account/Login",
        "/Account/ExternalLogin",
        "/health",
        "/lib/",
        "/css/",
        "/js/",
        "/images/"
    ];

    public MaintenanceModeMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, ISiteSettingsProvider siteSettingsProvider)
    {
        var path = context.Request.Path.Value ?? "";

        // 排除的路徑直接放行
        if (ExcludedPrefixes.Any(prefix => path.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)))
        {
            await _next(context);
            return;
        }

        // 已登入的 Admin / SuperAdmin 不受維護模式限制
        if (context.User.IsInRole("Admin") || context.User.IsInRole("SuperAdmin"))
        {
            await _next(context);
            return;
        }

        var settings = await siteSettingsProvider.GetSiteSettingsAsync();
        if (settings.MaintenanceMode)
        {
            context.Response.StatusCode = 503;
            context.Response.ContentType = "text/html; charset=utf-8";

            var message = string.IsNullOrWhiteSpace(settings.MaintenanceMessage)
                ? "網站維護中，請稍後再試"
                : settings.MaintenanceMessage;

            await context.Response.WriteAsync(GenerateMaintenanceHtml(message));
            return;
        }

        await _next(context);
    }

    private static string GenerateMaintenanceHtml(string message)
    {
        var encodedMessage = System.Net.WebUtility.HtmlEncode(message);

        return $$"""
            <!DOCTYPE html>
            <html lang="zh-TW">
            <head>
                <meta charset="utf-8" />
                <meta name="viewport" content="width=device-width, initial-scale=1.0" />
                <title>網站維護中</title>
                <style>
                    * { margin: 0; padding: 0; box-sizing: border-box; }
                    body {
                        font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, sans-serif;
                        background: #f8f9fa;
                        display: flex;
                        align-items: center;
                        justify-content: center;
                        min-height: 100vh;
                        color: #333;
                    }
                    .maintenance-container {
                        text-align: center;
                        padding: 2rem;
                        max-width: 500px;
                    }
                    .maintenance-icon {
                        font-size: 4rem;
                        margin-bottom: 1.5rem;
                    }
                    h1 {
                        font-size: 1.75rem;
                        margin-bottom: 1rem;
                        color: #212529;
                    }
                    p {
                        font-size: 1.1rem;
                        color: #6c757d;
                        line-height: 1.6;
                    }
                </style>
            </head>
            <body>
                <div class="maintenance-container">
                    <div class="maintenance-icon">🔧</div>
                    <h1>網站維護中</h1>
                    <p>{{encodedMessage}}</p>
                </div>
            </body>
            </html>
            """;
    }
}

/// <summary>
/// MaintenanceModeMiddleware 擴充方法
/// </summary>
public static class MaintenanceModeMiddlewareExtensions
{
    public static IApplicationBuilder UseMaintenanceMode(this IApplicationBuilder app)
    {
        return app.UseMiddleware<MaintenanceModeMiddleware>();
    }
}
