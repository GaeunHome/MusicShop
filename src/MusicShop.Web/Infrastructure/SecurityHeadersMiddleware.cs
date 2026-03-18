namespace MusicShop.Web.Infrastructure;

/// <summary>
/// 安全標頭中間件
/// 為所有 HTTP 回應加入安全相關的標頭，防止常見的 Web 攻擊
/// 對應 OWASP Top 10: A05（安全設定錯誤）
/// </summary>
public class SecurityHeadersMiddleware
{
    private readonly RequestDelegate _next;

    public SecurityHeadersMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var headers = context.Response.Headers;

        // ─── X-Content-Type-Options ─────────────────────────────
        // 防止瀏覽器 MIME 嗅探，避免將非腳本檔案（如圖片）誤判為可執行腳本
        // 對應 CWE-16: Configuration
        headers.Append("X-Content-Type-Options", "nosniff");

        // ─── X-Frame-Options ─────────────────────────────────────
        // 防止點擊劫持（Clickjacking）：禁止網站被嵌入到其他網站的 <iframe> 中
        // DENY = 完全禁止；SAMEORIGIN = 僅允許同域名嵌入
        // 對應 CWE-1021: Improper Restriction of Rendered UI Layers
        headers.Append("X-Frame-Options", "DENY");

        // ─── Referrer-Policy ─────────────────────────────────────
        // 控制 HTTP Referer 標頭的傳送策略
        // strict-origin-when-cross-origin：同源請求傳完整 URL，跨域僅傳 origin，降級不傳
        headers.Append("Referrer-Policy", "strict-origin-when-cross-origin");

        // ─── X-Permitted-Cross-Domain-Policies ───────────────────
        // 禁止 Flash/PDF 等外掛載入跨域策略檔案
        headers.Append("X-Permitted-Cross-Domain-Policies", "none");

        // ─── Permissions-Policy ──────────────────────────────────
        // 限制瀏覽器功能（攝影機、麥克風、地理位置等）的使用權限
        // 音樂商店不需要這些功能，全部禁用以減少攻擊面
        headers.Append("Permissions-Policy", "camera=(), microphone=(), geolocation=(), payment=()");

        // ─── Content-Security-Policy ─────────────────────────────
        // 內容安全政策：限制頁面可載入的資源來源，防止 XSS 攻擊
        // 對應 CWE-79: Improper Neutralization of Input During Web Page Generation
        // - default-src 'self'：預設只允許同源資源
        // - style-src 加入 'unsafe-inline'：因 Bootstrap 使用行內樣式
        // - script-src 加入 'unsafe-inline'：因部分 JS 使用行內腳本（後續可改為 nonce）
        // - img-src 加入 data:：因部分圖片使用 Base64 編碼
        // - font-src 加入 CDN 來源：因使用 Google Fonts / CDN 字體
        headers.Append("Content-Security-Policy",
            "default-src 'self'; " +
            "script-src 'self' 'unsafe-inline' https://cdn.jsdelivr.net https://cdnjs.cloudflare.com; " +
            "style-src 'self' 'unsafe-inline' https://cdn.jsdelivr.net https://cdnjs.cloudflare.com https://fonts.googleapis.com; " +
            "img-src 'self' data: https:; " +
            "font-src 'self' https://cdn.jsdelivr.net https://cdnjs.cloudflare.com https://fonts.gstatic.com; " +
            "connect-src 'self'; " +
            "frame-ancestors 'none'; " +
            "base-uri 'self'; " +
            "form-action 'self'");

        await _next(context);
    }
}

/// <summary>
/// SecurityHeadersMiddleware 擴充方法
/// </summary>
public static class SecurityHeadersMiddlewareExtensions
{
    /// <summary>
    /// 將安全標頭中間件加入請求管線
    /// </summary>
    public static IApplicationBuilder UseSecurityHeaders(this IApplicationBuilder app)
    {
        return app.UseMiddleware<SecurityHeadersMiddleware>();
    }
}
