using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MusicShop.Library.Enums;
using MusicShop.Service.Services.Interfaces;
using MusicShop.Service.ViewModels.Cart;
using MusicShop.Library.Helpers;
using MusicShop.Web.Infrastructure;
namespace MusicShop.Controllers;

/// <summary>
/// 購物車控制器 - 展示層
/// 使用三層式架構：Controller → Service → Repository
/// </summary>
// [Authorize] 屬性確保只有登入使用者才能存取購物車相關功能
[Authorize]
public class CartController : BaseController // 繼承自 BaseController，提供共用的 GetAuthorizedUserId 方法
{
    private readonly ICartService _cartService;
    private readonly IOrderService _orderService;
    private readonly IUserService _userService;
    private readonly IEcpayLogisticsService _ecpayLogisticsService;
    private readonly ICouponService _couponService;

    public CartController(
        ICartService cartService,
        IOrderService orderService,
        IUserService userService,
        IEcpayLogisticsService ecpayLogisticsService,
        ICouponService couponService)
    {
        _cartService = cartService;
        _orderService = orderService;
        _userService = userService;
        _ecpayLogisticsService = ecpayLogisticsService;
        _couponService = couponService;
    }

    // GET: /Cart
    public async Task<IActionResult> Index()
    {
        var userId = GetAuthorizedUserId();

        var cartItems = await _cartService.GetCartItemViewModelsAsync(userId);
        var total = cartItems.Sum(c => c.SubTotal);

        ViewBag.Total = total;
        return View(cartItems);
    }

    // POST: /Cart/UpdateQuantity
    // 傳統表單提交版本：整頁重新導向，適用於非 JS 環境的降級處理
    // 與下方 UpdateQuantityAjax 的差異在於回傳方式（Redirect vs JSON）
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateQuantity(int cartItemId, int quantity)
    {
        var userId = GetAuthorizedUserId();

        try
        {
            if (quantity <= 0)
            {
                // 數量 <= 0 時，移除項目
                await _cartService.RemoveFromCartAsync(cartItemId, userId);
                TempData[TempDataKeys.Success] = "已移除商品";
            }
            else
            {
                await _cartService.UpdateCartItemQuantityAsync(cartItemId, userId, quantity);
                TempData[TempDataKeys.Success] = "已更新數量";
            }
        }
        catch (InvalidOperationException ex)
        {
            TempData[TempDataKeys.Error] = ex.Message;
        }
        catch (UnauthorizedAccessException ex)
        {
            TempData[TempDataKeys.Error] = ex.Message;
        }

        return RedirectToAction("Index");
    }

    // POST: /Cart/UpdateQuantityAjax
    // AJAX 版本：回傳 JSON 供前端局部更新數量與小計，避免整頁刷新
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateQuantityAjax(int cartItemId, int quantity)
    {
        var userId = GetCurrentUserId();
        if (string.IsNullOrEmpty(userId))
            return Json(new { success = false, message = "未登入" });

        try
        {
            var result = await _cartService.UpdateCartItemQuantityAjaxAsync(cartItemId, userId, quantity);

            // 格式化金額（Service 層回傳原始 decimal，由 Web 層負責顯示格式）
            return Json(new
            {
                result.Success,
                result.Message,
                result.Quantity,
                Subtotal = result.Subtotal.ToTaiwanPrice(),
                CartTotal = result.CartTotal.ToTaiwanPrice(),
                result.CartItemCount
            });
        }
        catch (Exception)
        {
            return Json(new { success = false, message = "更新數量時發生錯誤，請稍後再試" });
        }
    }

    // POST: /Cart/Remove
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Remove(int cartItemId)
    {
        var userId = GetAuthorizedUserId();

        try
        {
            await _cartService.RemoveFromCartAsync(cartItemId, userId);
            TempData[TempDataKeys.Success] = "已移除商品";
        }
        catch (InvalidOperationException ex)
        {
            TempData[TempDataKeys.Error] = ex.Message;
        }
        catch (UnauthorizedAccessException ex)
        {
            TempData[TempDataKeys.Error] = ex.Message;
        }

        return RedirectToAction("Index");
    }

    // POST: /Cart/Clear
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Clear()
    {
        var userId = GetAuthorizedUserId();

        try
        {
            await _cartService.ClearCartAsync(userId);
            TempData[TempDataKeys.Success] = "已清空購物車";
        }
        catch (Exception)
        {
            TempData[TempDataKeys.Error] = "清空購物車時發生錯誤，請稍後再試";
        }

        return RedirectToAction("Index");
    }

    // GET: /Cart/Checkout
    public async Task<IActionResult> Checkout()
    {
        var userId = GetAuthorizedUserId();

        var cartItemViewModels = await _cartService.GetCartItemViewModelsAsync(userId);

        if (!cartItemViewModels.Any())
        {
            TempData[TempDataKeys.Error] = "購物車是空的！";
            return RedirectToAction("Index");
        }

        // 透過 Service 層取得使用者基本資訊（預填收件人）
        var (fullName, phoneNumber) = await _userService.GetUserBasicInfoAsync(userId);

        // 計算總金額
        var total = cartItemViewModels.Sum(c => c.SubTotal);

        // 建立 CheckoutViewModel，預填使用者資料
        var viewModel = new CheckoutViewModel
        {
            // 預填收件人資訊（從個人帳戶）
            ReceiverName = fullName,
            ReceiverPhone = phoneNumber,

            // 購物車項目
            CartItems = cartItemViewModels,
            TotalAmount = total,

            // 預設值
            DeliveryMethod = DeliveryMethod.HomeDelivery,
            PaymentMethod = PaymentMethod.CashOnDelivery,
            InvoiceType = InvoiceType.Duplicate
        };

        // 傳遞縣市清單給前端
        ViewBag.Cities = TaiwanDistricts.Cities;

        // 傳遞可用優惠券
        ViewBag.AvailableCoupons = await _couponService.GetAvailableCouponsForCheckoutAsync(userId);

        return View(viewModel);
    }

    // POST: /Cart/PlaceOrder
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> PlaceOrder(CheckoutViewModel model)
    {
        var userId = GetAuthorizedUserId();

        // 驗證模型（基本欄位驗證）
        if (!ModelState.IsValid)
            return await ReturnCheckoutViewWithDataAsync(model, userId);

        // 商業邏輯驗證已移至 Service 層（OrderValidationService.ValidateCheckoutInfo）
        // 避免重複驗證邏輯，提升可維護性
        try
        {
            // 使用新的服務方法建立包含完整資訊的訂單
            // Service 層會進行完整的業務驗證（門市資訊、發票資訊、庫存等）
            // Service 層回傳訂單 ID，Controller 不直接接觸 Order Entity
            var orderId = await _orderService.CreateOrderWithFullInfoAsync(userId, model);
            TempData[TempDataKeys.Success] = $"訂單成立！訂單編號：#{orderId}";
            return RedirectToAction("OrderComplete", new { orderId });
        }
        catch (ArgumentException ex)
        {
            // 驗證錯誤（如門市資訊、發票資訊不完整）
            ModelState.AddModelError("", ex.Message);
            return await ReturnCheckoutViewWithDataAsync(model, userId);
        }
        catch (InvalidOperationException ex)
        {
            // 業務邏輯錯誤（如庫存不足）
            TempData[TempDataKeys.Error] = ex.Message;
            return RedirectToAction("Index");
        }
        catch (Exception)
        {
            TempData[TempDataKeys.Error] = "建立訂單時發生錯誤，請稍後再試";
            return RedirectToAction("Index");
        }
    }

    // GET: /Cart/OrderComplete
    public async Task<IActionResult> OrderComplete(int orderId)
    {
        var userId = GetAuthorizedUserId();

        try
        {
            var orderViewModel = await _orderService.GetOrderConfirmationViewModelAsync(orderId, userId);

            if (orderViewModel == null)
                return NotFound();

            return View(orderViewModel);
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
    }

    // GET: /Cart/GetStoreList?type=UNIMART
    // AJAX 取得 ECPay 超商門市清單
    [HttpGet]
    public async Task<IActionResult> GetStoreList(string type)
    {
        // 綠界 ECPay 物流 API 定義的超商代碼（不可自行修改）：
        // UNIMART = 7-ELEVEN、FAMI = 全家、HILIFE = 萊爾富、OKMART = OK 超商
        var allowedTypes = new HashSet<string> { "UNIMART", "FAMI", "HILIFE", "OKMART" };
        if (string.IsNullOrEmpty(type) || !allowedTypes.Contains(type.ToUpper()))
            return Json(new { success = false, message = "無效的超商類型" });

        try
        {
            var stores = await _ecpayLogisticsService.GetStoreListAsync(type.ToUpper());
            return Json(new { success = true, stores });
        }
        catch (Exception)
        {
            // 不將內部例外訊息暴露給前端，避免洩漏敏感資訊
            return Json(new { success = false, message = "取得門市資料失敗，請稍後再試" });
        }
    }

    // GET: /Cart/GetDistricts
    // AJAX 取得鄉鎮市區清單（根據縣市）
    [HttpGet]
    public IActionResult GetDistricts(string city)
    {
        if (string.IsNullOrEmpty(city))
            return Json(new List<object>());

        var districts = TaiwanDistricts.GetDistrictsByCity(city);

        var result = districts.Select(d => new
        {
            district = d.District,
            postalCode = d.PostalCode
        }).ToList();

        return Json(result);
    }

    // ==================== 私有輔助方法 ====================

    /// <summary>
    /// 準備結帳視圖資料並返回（用於驗證失敗時）
    /// </summary>
    /// <param name="model">結帳視圖模型</param>
    /// <param name="userId">使用者 ID</param>
    /// <returns>結帳視圖</returns>
    private async Task<IActionResult> ReturnCheckoutViewWithDataAsync(CheckoutViewModel model, string userId)
    {
        var cartItemViewModels = await _cartService.GetCartItemViewModelsAsync(userId);
        model.CartItems = cartItemViewModels;
        // 優化：直接從已取得的 cartItems 計算總計，避免重複查詢資料庫
        model.TotalAmount = cartItemViewModels.Sum(c => c.SubTotal);
        ViewBag.Cities = TaiwanDistricts.Cities;
        ViewBag.AvailableCoupons = await _couponService.GetAvailableCouponsForCheckoutAsync(userId);
        return View("Checkout", model);
    }
}
