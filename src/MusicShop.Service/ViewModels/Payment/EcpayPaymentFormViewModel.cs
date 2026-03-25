namespace MusicShop.Service.ViewModels.Payment;

/// <summary>
/// ECPay 付款表單 ViewModel
/// 用於 View 層呈現 ECPay 付款轉導頁面所需的表單資料
/// </summary>
public class EcpayPaymentFormViewModel
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
