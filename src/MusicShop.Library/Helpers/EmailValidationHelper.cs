using System.Net;
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
    /// 公共 DNS 伺服器清單，當系統預設 DNS 查詢失敗時作為備援
    /// 避免因內部 DNS 未正確設定外部網域而誤擋合法使用者
    /// </summary>
    private static readonly IPEndPoint[] PublicDnsServers =
    [
        new(IPAddress.Parse("8.8.8.8"), 53),        // Google DNS
        new(IPAddress.Parse("1.1.1.1"), 53)          // Cloudflare DNS
    ];

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
    /// 先用系統預設 DNS 查詢，若查無結果則依序使用公共 DNS 備援查詢
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
            // 先用系統預設 DNS 查詢
            if (await CheckMxWithClientAsync(new LookupClient(new LookupClientOptions
            {
                Timeout = DnsTimeout,
                UseCache = true
            }), domain))
                return true;

            // 系統 DNS 查無結果，改用公共 DNS 備援查詢
            foreach (var dnsServer in PublicDnsServers)
            {
                if (await CheckMxWithClientAsync(new LookupClient(new LookupClientOptions(dnsServer)
                {
                    Timeout = DnsTimeout,
                    UseCache = true
                }), domain))
                    return true;
            }

            return false;
        }
        catch (DnsResponseException)
        {
            // DNS 回應異常（伺服器錯誤、格式錯誤等），不阻擋使用者註冊
            return true;
        }
        catch (OperationCanceledException)
        {
            // DNS 查詢逾時，不阻擋使用者註冊
            return true;
        }
        catch (System.Net.Sockets.SocketException)
        {
            // 網路連線問題，不阻擋使用者註冊
            return true;
        }
    }

    /// <summary>
    /// 使用指定的 DNS 客戶端檢查網域是否有 MX 或 A 記錄
    /// </summary>
    private static async Task<bool> CheckMxWithClientAsync(LookupClient client, string domain)
    {
        // 查詢 MX 記錄
        var result = await client.QueryAsync(domain, QueryType.MX);
        if (result.Answers.MxRecords().Any())
            return true;

        // 部分小型網域沒設定 MX 記錄但有 A 記錄也能收信（隱式 MX）
        var aResult = await client.QueryAsync(domain, QueryType.A);
        return aResult.Answers.ARecords().Any();
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
