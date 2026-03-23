using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Web;
using Microsoft.Extensions.Configuration;
using MusicShop.Service.Services.Interfaces;

namespace MusicShop.Service.Services.Implementation;

/// <summary>
/// ECPay 物流服務實作
/// 負責產生 CheckMacValue 並呼叫綠界超商門市 API
/// </summary>
public class EcpayLogisticsService : IEcpayLogisticsService
{
    private readonly HttpClient _httpClient;
    private readonly string _merchantId;
    private readonly string _hashKey;
    private readonly string _hashIV;
    private readonly string _apiBaseUrl;

    public EcpayLogisticsService(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;

        var section = configuration.GetSection("EcpayLogistics");
        _merchantId = section["MerchantID"] ?? throw new InvalidOperationException("缺少 EcpayLogistics:MerchantID 設定");
        _hashKey = section["HashKey"] ?? throw new InvalidOperationException("缺少 EcpayLogistics:HashKey 設定");
        _hashIV = section["HashIV"] ?? throw new InvalidOperationException("缺少 EcpayLogistics:HashIV 設定");

        var isTest = !bool.TryParse(section["IsTest"], out var parsedIsTest) || parsedIsTest;
        _apiBaseUrl = isTest
            ? "https://logistics-stage.ecpay.com.tw"
            : "https://logistics.ecpay.com.tw";
    }

    /// <summary>
    /// 取得超商門市清單
    /// </summary>
    public async Task<IEnumerable<EcpayStoreInfo>> GetStoreListAsync(string cvsType)
    {
        // 組合請求參數
        var parameters = new Dictionary<string, string>
        {
            { "MerchantID", _merchantId },
            { "CvsType", cvsType }
        };

        // 產生 CheckMacValue
        parameters["CheckMacValue"] = GenerateCheckMacValue(parameters);

        // 送出 POST 請求（application/x-www-form-urlencoded）
        var response = await _httpClient.PostAsync(
            $"{_apiBaseUrl}/Helper/GetStoreList",
            new FormUrlEncodedContent(parameters));

        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();

        // 解析 ECPay 回應
        var result = JsonSerializer.Deserialize<EcpayGetStoreListResponse>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        if (result == null || result.RtnCode != 1)
            throw new InvalidOperationException($"ECPay API 錯誤：{result?.RtnMsg ?? "無回應"}");

        // StoreList 是依超商類型分組，每組內有 StoreInfo 陣列，需展平
        return result.StoreList?
            .SelectMany(g => g.StoreInfo ?? Enumerable.Empty<EcpayStoreInfo>())
            ?? Enumerable.Empty<EcpayStoreInfo>();
    }

    /// <summary>
    /// 產生 CheckMacValue（MD5）
    /// 此為綠界 ECPay 官方規範的驗證碼產生演算法，每一步驟順序與格式皆不可更動。
    /// 參考文件：https://www.ecpay.com.tw/Content/files/ecpay_030.pdf
    /// </summary>
    private string GenerateCheckMacValue(Dictionary<string, string> parameters)
    {
        // 1. 依 Key 字母排序（不分大小寫）— ECPay 規範要求參數必須按字母序排列
        var sortedParams = parameters
            .OrderBy(p => p.Key, StringComparer.OrdinalIgnoreCase)
            .Select(p => $"{p.Key}={p.Value}");

        // 2. 前後加上 HashKey / HashIV — 這是 ECPay 核發給商家的密鑰，
        //    用於確保請求來源的合法性，格式固定為 "HashKey=...&參數&HashIV=..."
        var raw = $"HashKey={_hashKey}&{string.Join("&", sortedParams)}&HashIV={_hashIV}";

        // 3. URL encode 後轉小寫 — ECPay 規範要求使用 .NET 的 UrlEncode 並統一轉小寫，
        //    這與一般 URL encoding 保留大寫的慣例不同，是 ECPay 特有的要求
        var encoded = HttpUtility.UrlEncode(raw).ToLower();

        // 4. MD5 雜湊 — ECPay 物流 API 指定使用 MD5（金流 API 則使用 SHA-256，勿混淆）
        var hashBytes = MD5.HashData(Encoding.UTF8.GetBytes(encoded));

        // 5. 轉成大寫十六進位字串 — ECPay 規範要求最終結果為大寫
        var checkMacValue = Convert.ToHexString(hashBytes).ToUpper();

        return checkMacValue;
    }

    // ECPay API 回應格式
    // 實際結構：StoreList 是依超商類型分組，每組有 CvsType + StoreInfo 陣列
    private class EcpayGetStoreListResponse
    {
        public int RtnCode { get; set; }
        public string? RtnMsg { get; set; }
        public List<EcpayStoreGroup>? StoreList { get; set; }
    }

    private class EcpayStoreGroup
    {
        public string? CvsType { get; set; }
        public List<EcpayStoreInfo>? StoreInfo { get; set; }
    }
}
