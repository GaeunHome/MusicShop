// ─────────────────────────────────────────────────────────────
// AdminController.Order.cs - Order management partial class
// Responsibility: Order listing, detail view, and status update actions
// ─────────────────────────────────────────────────────────────

using Microsoft.AspNetCore.Mvc;
using MusicShop.Library.Enums;
using MusicShop.Web.Infrastructure;

namespace MusicShop.Controllers
{
    public partial class AdminController
    {
        // ─── 訂單管理 ───────────────────────────────────────
        public async Task<IActionResult> Orders()
        {
            var orders = await _orderService.GetAdminOrderListViewModelsAsync();
            return View("Order/Index", orders);
        }

        public async Task<IActionResult> OrderDetail(int id)
        {
            try
            {
                var vm = await _orderService.GetAdminOrderDetailViewModelAsync(id);
                if (vm == null) return NotFound();
                return View("Order/Detail", vm);
            }
            catch (Exception ex)
            {
                TempData[TempDataKeys.Error] = ex.Message;
                return RedirectToAction("Orders");
            }
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateOrderStatus(int orderId, int status)
        {
            try
            {
                await _orderService.UpdateOrderStatusAsync(orderId, (OrderStatus)status);
                TempData[TempDataKeys.Success] = "訂單狀態更新成功！";
            }
            catch (Exception ex)
            {
                TempData[TempDataKeys.Error] = ex.Message;
            }

            return RedirectToAction("OrderDetail", new { id = orderId });
        }
    }
}
