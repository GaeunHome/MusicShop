using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MusicShop.Data.Entities;
using MusicShop.Service.Services.Interfaces;
using MusicShop.Service.ViewModels.Cart;
using MusicShop.Library.Helpers;

namespace MusicShop.Controllers;

/// <summary>
/// 購物車控制器 - 展示層
/// 使用三層式架構：Controller → Service → Repository
/// </summary>
[Authorize]
public class CartController : Controller
{
    private readonly ICartService _cartService;
    private readonly IOrderService _orderService;
    private readonly UserManager<AppUser> _userManager;

    public CartController(
        ICartService cartService,
        IOrderService orderService,
        UserManager<AppUser> userManager)
    {
        _cartService = cartService;
        _orderService = orderService;
        _userManager = userManager;
    }

    // GET: /Cart
    public async Task<IActionResult> Index()
    {
        var userId = GetAuthorizedUserId();

        var cartItems = await _cartService.GetUserCartAsync(userId);
        var total = await _cartService.GetCartTotalAsync(userId);

        ViewBag.Total = total;
        return View(cartItems);
    }

    // POST: /Cart/Add
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Add(int albumId, int quantity = 1)
    {
        var userId = GetAuthorizedUserId();

        try
        {
            await _cartService.AddToCartAsync(userId, albumId, quantity);
            TempData["Success"] = "已加入購物車！";
        }
        catch (InvalidOperationException ex)
        {
            TempData["Error"] = ex.Message;
        }
        catch (ArgumentException ex)
        {
            TempData["Error"] = ex.Message;
        }

        return RedirectToAction("Index");
    }

    // POST: /Cart/UpdateQuantity
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
                TempData["Success"] = "已移除商品";
            }
            else
            {
                await _cartService.UpdateCartItemQuantityAsync(cartItemId, userId, quantity);
                TempData["Success"] = "已更新數量";
            }
        }
        catch (InvalidOperationException ex)
        {
            TempData["Error"] = ex.Message;
        }
        catch (UnauthorizedAccessException ex)
        {
            TempData["Error"] = ex.Message;
        }

        return RedirectToAction("Index");
    }

    // POST: /Cart/UpdateQuantityAjax
    // AJAX 更新購物車數量（返回 JSON）
    [HttpPost]
    public async Task<IActionResult> UpdateQuantityAjax(int cartItemId, int quantity)
    {
        var userId = _userManager.GetUserId(User);
        if (string.IsNullOrEmpty(userId))
            return Json(new { success = false, message = "未登入" });

        try
        {
            var result = await _cartService.UpdateCartItemQuantityAjaxAsync(cartItemId, userId, quantity);
            return Json(result);
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = ex.Message });
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
            TempData["Success"] = "已移除商品";
        }
        catch (InvalidOperationException ex)
        {
            TempData["Error"] = ex.Message;
        }
        catch (UnauthorizedAccessException ex)
        {
            TempData["Error"] = ex.Message;
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
            TempData["Success"] = "已清空購物車";
        }
        catch (Exception ex)
        {
            TempData["Error"] = ex.Message;
        }

        return RedirectToAction("Index");
    }

    // GET: /Cart/Checkout
    public async Task<IActionResult> Checkout()
    {
        var userId = GetAuthorizedUserId();

        var cartItems = await _cartService.GetUserCartAsync(userId);
        var cartItemsList = cartItems.ToList();

        if (!cartItemsList.Any())
        {
            TempData["Error"] = "購物車是空的！";
            return RedirectToAction("Index");
        }

        // 取得當前使用者資訊
        var user = await _userManager.GetUserAsync(User);

        // 計算總金額
        var total = await _cartService.GetCartTotalAsync(userId);

        // 建立 CheckoutViewModel，預填使用者資料
        var viewModel = new CheckoutViewModel
        {
            // 預填收件人資訊（從個人帳戶）
            ReceiverName = user?.FullName ?? string.Empty,
            ReceiverPhone = user?.PhoneNumber ?? string.Empty,

            // 購物車項目
            CartItems = cartItemsList,
            TotalAmount = total,

            // 預設值
            DeliveryMethod = DeliveryMethod.HomeDelivery,
            PaymentMethod = PaymentMethod.CashOnDelivery,
            InvoiceType = InvoiceType.Duplicate
        };

        // 傳遞縣市清單給前端
        ViewBag.Cities = TaiwanDistricts.Cities;

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
            var order = await _orderService.CreateOrderWithFullInfoAsync(userId, model);
            TempData["Success"] = $"訂單成立！訂單編號：#{order.Id}";
            return RedirectToAction("OrderComplete", new { orderId = order.Id });
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
            TempData["Error"] = ex.Message;
            return RedirectToAction("Index");
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"建立訂單時發生錯誤：{ex.Message}";
            return RedirectToAction("Index");
        }
    }

    // GET: /Cart/OrderComplete
    public async Task<IActionResult> OrderComplete(int orderId)
    {
        var userId = GetAuthorizedUserId();

        try
        {
            var order = await _orderService.GetOrderDetailAsync(orderId, userId);

            if (order == null)
                return NotFound();

            return View(order);
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
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
    /// 取得已授權的使用者 ID，若未登入則拋出異常
    /// </summary>
    /// <returns>使用者 ID</returns>
    /// <exception cref="UnauthorizedAccessException">使用者未登入</exception>
    private string GetAuthorizedUserId()
    {
        var userId = _userManager.GetUserId(User);
        if (string.IsNullOrEmpty(userId))
            throw new UnauthorizedAccessException("使用者未登入");
        return userId;
    }

    /// <summary>
    /// 準備結帳視圖資料並返回（用於驗證失敗時）
    /// </summary>
    /// <param name="model">結帳視圖模型</param>
    /// <param name="userId">使用者 ID</param>
    /// <returns>結帳視圖</returns>
    private async Task<IActionResult> ReturnCheckoutViewWithDataAsync(CheckoutViewModel model, string userId)
    {
        var cartItems = await _cartService.GetUserCartAsync(userId);
        model.CartItems = cartItems.ToList();
        // 優化：直接從已取得的 cartItems 計算總計，避免重複查詢資料庫
        model.TotalAmount = cartItems.Sum(c => c.Album!.Price * c.Quantity);
        ViewBag.Cities = TaiwanDistricts.Cities;
        return View("Checkout", model);
    }
}
