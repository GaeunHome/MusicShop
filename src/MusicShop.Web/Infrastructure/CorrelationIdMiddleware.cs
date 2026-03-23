using Serilog.Context;

namespace MusicShop.Web.Infrastructure;

/// <summary>
/// 關聯識別碼中間件
/// 為每個 HTTP 請求產生唯一的 Correlation ID，貫穿整個請求生命週期的日誌，
/// 方便在分散式環境中追蹤同一請求的所有操作。
/// 若請求已帶有 X-Correlation-ID 標頭（如來自上游服務），則沿用該值。
/// </summary>
public class CorrelationIdMiddleware
{
    private const string CorrelationIdHeader = "X-Correlation-ID";
    private readonly RequestDelegate _next;

    public CorrelationIdMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // 優先使用上游傳入的 Correlation ID，否則產生新的
        var correlationId = context.Request.Headers[CorrelationIdHeader].FirstOrDefault()
                            ?? Guid.NewGuid().ToString("N")[..12];

        // 將 Correlation ID 加入回應標頭，方便前端或 API 消費者追蹤
        context.Response.Headers[CorrelationIdHeader] = correlationId;

        // 存入 HttpContext.Items，供 Controller / Service 層取用
        context.Items["CorrelationId"] = correlationId;

        // 推入 Serilog LogContext，該請求內所有日誌自動帶上 CorrelationId 屬性
        using (LogContext.PushProperty("CorrelationId", correlationId))
        {
            await _next(context);
        }
    }
}

/// <summary>
/// CorrelationIdMiddleware 擴充方法
/// </summary>
public static class CorrelationIdMiddlewareExtensions
{
    /// <summary>
    /// 將關聯識別碼中間件加入請求管線
    /// </summary>
    public static IApplicationBuilder UseCorrelationId(this IApplicationBuilder app)
    {
        return app.UseMiddleware<CorrelationIdMiddleware>();
    }
}
