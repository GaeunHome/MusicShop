using DnsClient;

namespace MusicShop.Library.Helpers;

/// <summary>
/// Email 驗證工具類
/// 提供拋棄式信箱檢查與 MX 記錄驗證，確保 Email 地址真實可用
/// </summary>
public static class EmailValidationHelper
{
    /// <summary>
    /// 已知的拋棄式（免洗）信箱網域清單
    /// 這些服務提供臨時 Email，常被用於繞過註冊驗證
    /// </summary>
    private static readonly HashSet<string> DisposableDomains = new(StringComparer.OrdinalIgnoreCase)
    {
        // 主流免洗信箱服務
        "mailinator.com",
        "guerrillamail.com",
        "guerrillamail.net",
        "guerrillamail.org",
        "tempmail.com",
        "temp-mail.org",
        "throwaway.email",
        "yopmail.com",
        "yopmail.fr",
        "sharklasers.com",
        "guerrillamailblock.com",
        "grr.la",
        "dispostable.com",
        "trashmail.com",
        "trashmail.net",
        "trashmail.me",
        "mailnesia.com",
        "maildrop.cc",
        "discard.email",
        "fakeinbox.com",
        "mailcatch.com",
        "tempail.com",
        "tempr.email",
        "10minutemail.com",
        "20minutemail.com",
        "minutemail.com",
        "emailondeck.com",
        "mohmal.com",
        "getnada.com",
        "inboxkitten.com",
        "mailsac.com",
        "mytemp.email",
        "tmpmail.net",
        "tmpmail.org",
        "binkmail.com",
        "safetymail.info",
        "filzmail.com",
        "spam4.me",
        "spamgourmet.com",
        "harakirimail.com"
    };

    /// <summary>
    /// MX 記錄查詢逾時時間
    /// </summary>
    private static readonly TimeSpan DnsTimeout = TimeSpan.FromSeconds(5);

    /// <summary>
    /// 檢查 Email 是否為拋棄式信箱
    /// </summary>
    /// <param name="email">要檢查的 Email 地址</param>
    /// <returns>true 表示是拋棄式信箱，應拒絕註冊</returns>
    public static bool IsDisposableEmail(string email)
    {
        var domain = ExtractDomain(email);
        return domain != null && DisposableDomains.Contains(domain);
    }

    /// <summary>
    /// 驗證 Email 網域是否有有效的 MX 記錄（表示該網域能接收郵件）
    /// </summary>
    /// <param name="email">要驗證的 Email 地址</param>
    /// <returns>true 表示有有效的 MX 記錄</returns>
    public static async Task<bool> HasValidMxRecordAsync(string email)
    {
        var domain = ExtractDomain(email);
        if (domain == null)
            return false;

        try
        {
            var lookupClient = new LookupClient(new LookupClientOptions
            {
                Timeout = DnsTimeout,
                UseCache = true
            });

            // 查詢 MX 記錄
            var result = await lookupClient.QueryAsync(domain, QueryType.MX);

            if (result.Answers.MxRecords().Any())
                return true;

            // 部分小型網域沒設定 MX 記錄但有 A 記錄也能收信（隱式 MX）
            var aResult = await lookupClient.QueryAsync(domain, QueryType.A);
            return aResult.Answers.ARecords().Any();
        }
        catch
        {
            // DNS 查詢失敗（網路問題等），不阻擋使用者註冊
            // 寧可放行也不要因為 DNS 暫時故障拒絕合法使用者
            return true;
        }
    }

    /// <summary>
    /// 從 Email 地址擷取網域部分
    /// </summary>
    private static string? ExtractDomain(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return null;

        var atIndex = email.LastIndexOf('@');
        if (atIndex < 0 || atIndex >= email.Length - 1)
            return null;

        return email[(atIndex + 1)..];
    }
}
