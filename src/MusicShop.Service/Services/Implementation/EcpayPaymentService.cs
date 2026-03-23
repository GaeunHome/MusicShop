using System.Security.Cryptography;
using System.Text;
using System.Web;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MusicShop.Data.UnitOfWork;
using MusicShop.Library.Enums;
using MusicShop.Service.Services.Interfaces;

namespace MusicShop.Service.Services.Implementation;

/// <summary>
/// ECPay 金流服務實作
/// 負責產生付款參數、CheckMacValue（SHA-256）、處理付款回調
/// </summary>
public class EcpayPaymentService : IEcpayPaymentService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICouponService _couponService;
    private readonly IConfiguration _configuration;
    private readonly ILogger<EcpayPaymentService> _logger;

    private string MerchantID => _configuration["EcpayPayment:MerchantID"] ?? "";
    private string HashKey => _configuration["EcpayPayment:HashKey"] ?? "";
    private string HashIV => _configuration["EcpayPayment:HashIV"] ?? "";
    private bool IsTest => _configuration.GetValue<bool>("EcpayPayment:IsTest");

    private string PaymentBaseUrl => IsTest
        ? "https://payment-stage.ecpay.com.tw"
        : "https://payment.ecpay.com.tw";

    public EcpayPaymentService(
        IUnitOfWork unitOfWork,
        ICouponService couponService,
        IConfiguration configuration,
        ILogger<EcpayPaymentService> logger)
    {
        _unitOfWork = unitOfWork;
        _couponService = couponService;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<EcpayPaymentFormData> BuildPaymentFormDataAsync(
        int orderId, string returnUrl, string orderResultUrl)
    {
        var order = await _unitOfWork.Orders.GetOrderByIdAsync(orderId)
            ?? throw new InvalidOperationException($"找不到訂單 ID: {orderId}");

        if (order.Status != OrderStatus.Pending)
            throw new InvalidOperationException("只有待處理的訂單可以進行付款");

        if (order.PaymentMethod != PaymentMethod.CreditCard)
            throw new InvalidOperationException("此訂單的付款方式不是信用卡");

        // 產生唯一的 MerchantTradeNo（最長 20 字元）
        // 使用毫秒級時間戳確保唯一性，避免同一秒內的碰撞
        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() % 10000000000; // 取後 10 位
        var merchantTradeNo = $"MS{orderId}T{timestamp}";
        if (merchantTradeNo.Length > 20)
            merchantTradeNo = merchantTradeNo[..20];

        // 將 MerchantTradeNo 儲存到訂單
        order.MerchantTradeNo = merchantTradeNo;
        await _unitOfWork.Orders.UpdateOrderAsync(order);
        await _unitOfWork.SaveChangesAsync();

        // 計算實付金額（整數，無小數）
        var totalAmount = (int)(order.TotalAmount - order.DiscountAmount);
        if (totalAmount <= 0) totalAmount = 1;

        // 組合商品名稱（用 # 分隔，最長 200 字元）
        var itemNames = order.OrderItems
            .Select(oi => $"{oi.Album?.Title ?? "商品"} x{oi.Quantity}")
            .ToList();
        var itemName = string.Join("#", itemNames);
        if (itemName.Length > 200)
            itemName = itemName[..197] + "...";

        // 交易時間（台灣時間 UTC+8）
        var taiwanTime = TimeZoneInfo.ConvertTimeFromUtc(
            DateTime.UtcNow,
            TimeZoneInfo.FindSystemTimeZoneById("Asia/Taipei"));

        // 組建 ECPay 參數
        var parameters = new Dictionary<string, string>
        {
            ["MerchantID"] = MerchantID,
            ["MerchantTradeNo"] = merchantTradeNo,
            ["MerchantTradeDate"] = taiwanTime.ToString("yyyy/MM/dd HH:mm:ss"),
            ["PaymentType"] = "aio",
            ["TotalAmount"] = totalAmount.ToString(),
            ["TradeDesc"] = "MusicShop 線上訂單",
            ["ItemName"] = itemName,
            ["ReturnURL"] = returnUrl,
            ["OrderResultURL"] = orderResultUrl,
            ["ChoosePayment"] = "Credit",
            ["EncryptType"] = "1",
            ["NeedExtraPaidInfo"] = "N"
        };

        // 產生 CheckMacValue
        parameters["CheckMacValue"] = GenerateCheckMacValue(parameters);

        _logger.LogInformation(
            "ECPay 付款參數已產生：OrderId={OrderId}, MerchantTradeNo={MerchantTradeNo}, Amount={Amount}",
            orderId, merchantTradeNo, totalAmount);

        return new EcpayPaymentFormData
        {
            ActionUrl = $"{PaymentBaseUrl}/Cashier/AioCheckOut/V5",
            Parameters = parameters
        };
    }

    public bool VerifyCheckMacValue(Dictionary<string, string> callbackParams)
    {
        if (!callbackParams.TryGetValue("CheckMacValue", out var receivedMac))
            return false;

        // 複製參數並移除 CheckMacValue 後重新計算
        var paramsToVerify = new Dictionary<string, string>(callbackParams);
        paramsToVerify.Remove("CheckMacValue");

        var calculatedMac = GenerateCheckMacValue(paramsToVerify);

        // 使用固定時間比較防止計時攻擊
        return CryptographicOperations.FixedTimeEquals(
            Encoding.UTF8.GetBytes(receivedMac.ToUpper()),
            Encoding.UTF8.GetBytes(calculatedMac));
    }

    public async Task ProcessPaymentResultAsync(Dictionary<string, string> callbackParams)
    {
        // 驗證必要參數存在
        var requiredFields = new[] { "MerchantID", "MerchantTradeNo", "RtnCode", "TradeAmt" };
        foreach (var field in requiredFields)
        {
            if (!callbackParams.ContainsKey(field) || string.IsNullOrEmpty(callbackParams[field]))
            {
                _logger.LogWarning("ECPay 回調缺少必要參數：{Field}", field);
                return;
            }
        }

        var merchantTradeNo = callbackParams["MerchantTradeNo"];
        var rtnCode = int.TryParse(callbackParams["RtnCode"], out var code) ? code : 0;

        // 驗證 MerchantID 是否與設定一致
        if (callbackParams["MerchantID"] != MerchantID)
        {
            _logger.LogWarning(
                "ECPay 回調 MerchantID 不符：Expected={Expected}, Got={Got}",
                MerchantID, callbackParams["MerchantID"]);
            return;
        }

        var order = await _unitOfWork.Orders.GetOrderByMerchantTradeNoAsync(merchantTradeNo);
        if (order == null)
        {
            _logger.LogWarning("ECPay 回調找不到訂單：MerchantTradeNo={MerchantTradeNo}", merchantTradeNo);
            return;
        }

        // 驗證回調金額與訂單金額一致，防止金額篡改
        var expectedAmount = (int)(order.TotalAmount - order.DiscountAmount);
        if (expectedAmount <= 0) expectedAmount = 1;
        if (int.TryParse(callbackParams["TradeAmt"], out var callbackAmount) && callbackAmount != expectedAmount)
        {
            _logger.LogWarning(
                "ECPay 回調金額不符：OrderId={OrderId}, Expected={Expected}, Got={Got}",
                order.Id, expectedAmount, callbackAmount);
            return;
        }

        // 冪等性：已付款的訂單不重複處理
        if (order.Status != OrderStatus.Pending)
        {
            _logger.LogInformation(
                "ECPay 回調：訂單已處理，跳過。OrderId={OrderId}, Status={Status}",
                order.Id, order.Status);
            return;
        }

        if (rtnCode == 1)
        {
            order.Status = OrderStatus.Paid;
            order.UpdatedAt = DateTime.UtcNow;
            await _unitOfWork.Orders.UpdateOrderAsync(order);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation(
                "ECPay 付款成功：OrderId={OrderId}, MerchantTradeNo={MerchantTradeNo}, Amount={Amount}",
                order.Id, merchantTradeNo, callbackAmount);
        }
        else
        {
            // 付款失敗：取消訂單並回滾（恢復庫存、退還優惠券）
            await CancelOrderOnPaymentFailureAsync(order);

            _logger.LogWarning(
                "ECPay 付款失敗，訂單已取消：OrderId={OrderId}, MerchantTradeNo={MerchantTradeNo}, RtnCode={RtnCode}",
                order.Id, merchantTradeNo, rtnCode);
        }
    }

    /// <summary>
    /// 信用卡付款失敗時取消訂單
    /// 恢復庫存、退還優惠券，與 OrderService.CancelOrderAsync 邏輯一致
    /// </summary>
    private async Task CancelOrderOnPaymentFailureAsync(Data.Entities.Order order)
    {
        try
        {
            await _unitOfWork.BeginTransactionAsync();

            // 1. 恢復庫存
            foreach (var orderItem in order.OrderItems)
            {
                var album = await _unitOfWork.Albums.GetAlbumByIdAsync(orderItem.AlbumId);
                if (album != null)
                {
                    album.Stock += orderItem.Quantity;
                    await _unitOfWork.Albums.UpdateAlbumAsync(album);
                }
            }

            // 2. 退還優惠券
            if (order.UserCouponId.HasValue)
                await _couponService.ReleaseCouponAsync(order.UserCouponId.Value);

            // 3. 更新訂單狀態為已取消
            order.Status = OrderStatus.Cancelled;
            order.UpdatedAt = DateTime.UtcNow;
            await _unitOfWork.Orders.UpdateOrderAsync(order);

            await _unitOfWork.CommitAsync();

            _logger.LogInformation(
                "付款失敗訂單已回滾：OrderId={OrderId}, 已恢復庫存{CouponInfo}",
                order.Id, order.UserCouponId.HasValue ? "並退還優惠券" : "");
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackAsync();
            _logger.LogError(ex, "付款失敗訂單回滾時發生錯誤：OrderId={OrderId}", order.Id);
            throw;
        }
    }

    /// <summary>
    /// 產生 CheckMacValue（SHA-256）
    /// 此為綠界金流 API 規範的驗證碼演算法，步驟順序與格式不可更動
    /// 注意：金流使用 SHA-256，物流使用 MD5，兩者不同
    /// </summary>
    private string GenerateCheckMacValue(Dictionary<string, string> parameters)
    {
        // 1. 依 Key 字母排序（不分大小寫）
        var sortedParams = parameters
            .OrderBy(p => p.Key, StringComparer.OrdinalIgnoreCase)
            .Select(p => $"{p.Key}={p.Value}");

        // 2. 前後加上 HashKey 與 HashIV
        var raw = $"HashKey={HashKey}&{string.Join("&", sortedParams)}&HashIV={HashIV}";

        // 3. URL encode 後轉小寫（ECPay 規範使用 .NET HttpUtility.UrlEncode）
        var encoded = HttpUtility.UrlEncode(raw).ToLower();

        // 4. SHA-256 雜湊後轉大寫
        var hashBytes = SHA256.HashData(Encoding.UTF8.GetBytes(encoded));
        return BitConverter.ToString(hashBytes).Replace("-", "").ToUpper();
    }
}
