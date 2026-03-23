namespace MusicShop.Service.Services.Interfaces;

/// <summary>
/// ECPay 金流服務介面
/// 負責建立付款參數、產生 CheckMacValue、驗證回調
/// </summary>
public interface IEcpayPaymentService
{
    /// <summary>
    /// 產生 ECPay 付款表單參數
    /// </summary>
    /// <param name="orderId">訂單 ID</param>
    /// <param name="returnUrl">伺服器端通知 URL（ReturnURL）</param>
    /// <param name="orderResultUrl">使用者導回 URL（OrderResultURL）</param>
    /// <returns>ECPay 付款表單所需的所有參數（含 CheckMacValue）</returns>
    Task<EcpayPaymentFormData> BuildPaymentFormDataAsync(int orderId, string returnUrl, string orderResultUrl);

    /// <summary>
    /// 驗證 ECPay 回調的 CheckMacValue
    /// </summary>
    /// <param name="callbackParams">ECPay 回傳的所有參數</param>
    /// <returns>是否驗證通過</returns>
    bool VerifyCheckMacValue(Dictionary<string, string> callbackParams);

    /// <summary>
    /// 處理 ECPay 付款結果通知
    /// </summary>
    /// <param name="callbackParams">ECPay 回傳的所有參數（用於金額驗證）</param>
    /// <returns>處理結果</returns>
    Task ProcessPaymentResultAsync(Dictionary<string, string> callbackParams);
}

/// <summary>
/// ECPay 付款表單資料
/// </summary>
public class EcpayPaymentFormData
{
    /// <summary>
    /// ECPay AioCheckOut 端點 URL
    /// </summary>
    public string ActionUrl { get; set; } = string.Empty;

    /// <summary>
    /// 表單參數（Key-Value 對）
    /// </summary>
    public Dictionary<string, string> Parameters { get; set; } = new();
}
