using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MusicShop.Service.Services.Interfaces;
using MusicShop.Web.Infrastructure;

namespace MusicShop.Controllers;

/// <summary>
/// 金流控制器
/// 處理 ECPay 綠界金流的付款導向、伺服器通知與使用者導回
/// </summary>
public class PaymentController : BaseController
{
    private readonly IEcpayPaymentService _ecpayPaymentService;
    private readonly IOrderService _orderService;
    private readonly ILogger<PaymentController> _logger;

    public PaymentController(
        IEcpayPaymentService ecpayPaymentService,
        IOrderService orderService,
        ILogger<PaymentController> logger)
    {
        _ecpayPaymentService = ecpayPaymentService;
        _orderService = orderService;
        _logger = logger;
    }

    /// <summary>
    /// 導向 ECPay 付款頁面
    /// 產生付款參數後渲染自動提交表單，將使用者導向綠界付款頁
    /// </summary>
    [Authorize]
    [HttpGet]
    public async Task<IActionResult> Checkout(int orderId)
    {
        var userId = GetAuthorizedUserId();

        // 驗證訂單所有權
        if (!await _orderService.IsOrderOwnedByUserAsync(orderId, userId))
            return Forbid();

        try
        {
            // 組建絕對 URL（ECPay 需要完整的 URL）
            var returnUrl = Url.Action("PaymentNotify", "Payment", null, Request.Scheme)!;
            var orderResultUrl = Url.Action("PaymentResult", "Payment", null, Request.Scheme)!;

            var formData = await _ecpayPaymentService.BuildPaymentFormDataAsync(
                orderId, returnUrl, orderResultUrl);

            return View("RedirectToEcpay", formData);
        }
        catch (InvalidOperationException ex)
        {
            TempData[TempDataKeys.Error] = ex.Message;
            return RedirectToAction("OrderComplete", "Cart", new { orderId });
        }
    }

    /// <summary>
    /// ECPay 伺服器端付款結果通知（ReturnURL）
    /// 由 ECPay 伺服器在背景呼叫，非使用者瀏覽器請求
    /// 必須回傳 "1|OK" 否則 ECPay 會持續重送
    /// </summary>
    [HttpPost]
    [AllowAnonymous]
    [IgnoreAntiforgeryToken]
    public async Task<IActionResult> PaymentNotify()
    {
        var callbackParams = Request.Form.Keys
            .ToDictionary(key => key, key => Request.Form[key].ToString());

        // 驗證 CheckMacValue
        if (!_ecpayPaymentService.VerifyCheckMacValue(callbackParams))
        {
            _logger.LogWarning("ECPay PaymentNotify CheckMacValue 驗證失敗");
            return Content("0|ErrorMessage=CheckMacValue verification failed");
        }

        // 處理付款結果（傳入完整參數供金額驗證）
        await _ecpayPaymentService.ProcessPaymentResultAsync(callbackParams);

        // ECPay 要求回傳 "1|OK"
        return Content("1|OK");
    }

    /// <summary>
    /// ECPay 使用者導回頁面（OrderResultURL）
    /// 付款完成後 ECPay 將使用者重導回此頁面
    /// 注意：此為跨站 POST，SameSite Cookie 不會帶入，因此不能要求登入
    /// </summary>
    [HttpPost]
    [AllowAnonymous]
    [IgnoreAntiforgeryToken]
    public async Task<IActionResult> PaymentResult()
    {
        var callbackParams = Request.Form.Keys
            .ToDictionary(key => key, key => Request.Form[key].ToString());

        // 驗證 CheckMacValue
        if (!_ecpayPaymentService.VerifyCheckMacValue(callbackParams))
        {
            _logger.LogWarning("ECPay PaymentResult CheckMacValue 驗證失敗");
            TempData[TempDataKeys.Error] = "付款驗證失敗，請聯繫客服。";
            return RedirectToAction("Index", "Home");
        }

        // 處理付款結果（冪等，可能已被 PaymentNotify 處理過）
        await _ecpayPaymentService.ProcessPaymentResultAsync(callbackParams);

        // 從 MerchantTradeNo 解析訂單 ID（格式：MS{orderId}T{timestamp}）
        var merchantTradeNo = callbackParams.GetValueOrDefault("MerchantTradeNo") ?? "";
        var rtnCode = int.TryParse(callbackParams.GetValueOrDefault("RtnCode"), out var code) ? code : 0;
        var orderId = ParseOrderIdFromTradeNo(merchantTradeNo);

        if (rtnCode == 1)
            TempData[TempDataKeys.Success] = "信用卡付款成功！";
        else
            TempData[TempDataKeys.Error] = "付款未完成，如有疑問請聯繫客服。";

        return RedirectToAction("OrderComplete", "Cart", new { orderId });
    }

    /// <summary>
    /// 從 MerchantTradeNo 解析訂單 ID
    /// 格式：MS{orderId}T{timestamp}
    /// </summary>
    private static int ParseOrderIdFromTradeNo(string tradeNo)
    {
        if (string.IsNullOrEmpty(tradeNo) || !tradeNo.StartsWith("MS"))
            return 0;

        var tIndex = tradeNo.IndexOf('T', 2);
        if (tIndex <= 2) return 0;

        return int.TryParse(tradeNo[2..tIndex], out var orderId) ? orderId : 0;
    }
}
