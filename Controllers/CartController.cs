using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MusicShop.Models;
using MusicShop.Services.Interface;

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
        var userId = _userManager.GetUserId(User);
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

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
        var userId = _userManager.GetUserId(User);
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

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
        var userId = _userManager.GetUserId(User);
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

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

    // POST: /Cart/Remove
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Remove(int cartItemId)
    {
        var userId = _userManager.GetUserId(User);
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

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
        var userId = _userManager.GetUserId(User);
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

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
        var userId = _userManager.GetUserId(User);
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var cartItems = await _cartService.GetUserCartAsync(userId);
        var cartItemsList = cartItems.ToList();

        if (!cartItemsList.Any())
        {
            TempData["Error"] = "購物車是空的！";
            return RedirectToAction("Index");
        }

        var total = await _cartService.GetCartTotalAsync(userId);
        ViewBag.Total = total;

        return View(cartItemsList);
    }

    // POST: /Cart/PlaceOrder
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> PlaceOrder()
    {
        var userId = _userManager.GetUserId(User);
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        try
        {
            var order = await _orderService.CreateOrderFromCartAsync(userId);
            TempData["Success"] = $"訂單成立！訂單編號：#{order.Id}";
            return RedirectToAction("OrderComplete", new { orderId = order.Id });
        }
        catch (InvalidOperationException ex)
        {
            TempData["Error"] = ex.Message;
            return RedirectToAction("Index");
        }
    }

    // GET: /Cart/OrderComplete
    public async Task<IActionResult> OrderComplete(int orderId)
    {
        var userId = _userManager.GetUserId(User);
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

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
}
