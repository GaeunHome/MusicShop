using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Web;
using Microsoft.Extensions.Configuration;
using MusicShop.Service.Services.Interfaces;

namespace MusicShop.Service.Services.Implementation
{
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

            var section = configuration.GetSection("Ecpay");
            _merchantId = section["MerchantID"] ?? throw new InvalidOperationException("缺少 Ecpay:MerchantID 設定");
            _hashKey = section["HashKey"] ?? throw new InvalidOperationException("缺少 Ecpay:HashKey 設定");
            _hashIV = section["HashIV"] ?? throw new InvalidOperationException("缺少 Ecpay:HashIV 設定");

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
        /// 演算法：排序參數 → URL encode（小寫）→ MD5 → 大寫
        /// </summary>
        private string GenerateCheckMacValue(Dictionary<string, string> parameters)
        {
            // 1. 依 Key 字母排序（不分大小寫）
            var sortedParams = parameters
                .OrderBy(p => p.Key, StringComparer.OrdinalIgnoreCase)
                .Select(p => $"{p.Key}={p.Value}");

            // 2. 前後加上 HashKey / HashIV
            var raw = $"HashKey={_hashKey}&{string.Join("&", sortedParams)}&HashIV={_hashIV}";

            // 3. URL encode（.NET HttpUtility.UrlEncode，與 ECPay 規範相符）
            var encoded = HttpUtility.UrlEncode(raw).ToLower();

            // 4. MD5 雜湊（ECPay 規範使用 MD5，非 SHA-256）
            var hashBytes = MD5.HashData(Encoding.UTF8.GetBytes(encoded));

            // 5. 轉成大寫十六進位字串
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
}
