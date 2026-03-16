using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MusicShop.Data.Entities;
using MusicShop.Service.Services.Interfaces;

namespace MusicShop.Controllers;

/// <summary>
/// 訂單控制器 - 展示層
/// 使用三層式架構：Controller → Service → Repository
/// </summary>
[Authorize]
public class OrderController : Controller
{
    private readonly IOrderService _orderService;
    private readonly UserManager<AppUser> _userManager;

    public OrderController(IOrderService orderService, UserManager<AppUser> userManager)
    {
        _orderService = orderService;
        _userManager = userManager;
    }

    // GET: /Order
    // 顯示使用者的訂單列表
    public async Task<IActionResult> Index()
    {
        var userId = _userManager.GetUserId(User);
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var orders = await _orderService.GetOrderListViewModelsByUserAsync(userId);
        return View(orders);
    }

    // GET: /Order/Detail/5
    // 顯示訂單詳細資訊
    public async Task<IActionResult> Detail(int id)
    {
        var userId = _userManager.GetUserId(User);
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        try
        {
            var orderViewModel = await _orderService.GetOrderDetailViewModelAsync(id, userId);

            if (orderViewModel == null)
                return NotFound();

            return View(orderViewModel);
        }
        catch (UnauthorizedAccessException)
        {
            TempData["Error"] = "無權限查看此訂單";
            return RedirectToAction("Index");
        }
    }

    // POST: /Order/Cancel/5
    // 取消訂單
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancel(int id)
    {
        var userId = _userManager.GetUserId(User);
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        try
        {
            await _orderService.CancelOrderAsync(id, userId);
            TempData["Success"] = "訂單已取消";
        }
        catch (InvalidOperationException ex)
        {
            TempData["Error"] = ex.Message;
        }
        catch (UnauthorizedAccessException ex)
        {
            TempData["Error"] = ex.Message;
        }

        return RedirectToAction("Detail", new { id });
    }

    // ==================== 管理員功能 ====================

    // GET: /Order/Manage
    // 管理所有訂單（管理員專用）
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Manage()
    {
        var orders = await _orderService.GetAllOrdersAsync();
        return View(orders);
    }

    // POST: /Order/UpdateStatus
    // 更新訂單狀態（管理員專用）
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateStatus(int orderId, OrderStatus status)
    {
        try
        {
            await _orderService.UpdateOrderStatusAsync(orderId, status);
            TempData["Success"] = "訂單狀態已更新";
        }
        catch (InvalidOperationException ex)
        {
            TempData["Error"] = ex.Message;
        }

        return RedirectToAction("Manage");
    }
}
