// ─────────────────────────────────────────────────────────────
// OrderController.cs - 後台訂單管理
// Area: Admin
// ─────────────────────────────────────────────────────────────

using Microsoft.AspNetCore.Mvc;
using MusicShop.Library.Enums;
using MusicShop.Library.Helpers;
using MusicShop.Service.Services.Interfaces;
using MusicShop.Web.Infrastructure;

namespace MusicShop.Web.Areas.Admin.Controllers;

/// <summary>
/// 後台訂單管理控制器，負責訂單列表、詳情檢視與狀態更新
/// </summary>
public class OrderController : AdminBaseController
{
    private readonly IOrderService _orderService;

    public OrderController(IOrderService orderService)
    {
        _orderService = orderService;
    }

    // ─── 訂單列表 ─────────────────────────────────────────
    public async Task<IActionResult> Index(int page = 1)
    {
        var pagedOrders = await _orderService.GetAdminOrderListPagedAsync(page, DisplayConstants.AdminOrderPageSize);
        return View(pagedOrders);
    }

    // ─── 訂單詳情 ─────────────────────────────────────────
    public async Task<IActionResult> Detail(int id)
    {
        try
        {
            var vm = await _orderService.GetAdminOrderDetailViewModelAsync(id);
            if (vm == null) return NotFound();
            return View(vm);
        }
        catch (Exception ex)
        {
            TempData[TempDataKeys.Error] = ex.Message;
            return RedirectToAction(nameof(Index));
        }
    }

    // ─── 更新訂單狀態 ─────────────────────────────────────
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateStatus(int orderId, int status)
    {
        try
        {
            if (!Enum.IsDefined(typeof(OrderStatus), status))
            {
                TempData[TempDataKeys.Error] = "無效的訂單狀態";
                return RedirectToAction(nameof(Detail), new { id = orderId });
            }

            await _orderService.UpdateOrderStatusAsync(orderId, (OrderStatus)status);
            TempData[TempDataKeys.Success] = "訂單狀態更新成功！";
        }
        catch (Exception ex)
        {
            TempData[TempDataKeys.Error] = ex.Message;
        }

        return RedirectToAction(nameof(Detail), new { id = orderId });
    }
}
