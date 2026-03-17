using MusicShop.Data.Entities;
using MusicShop.Library.Enums;
using MusicShop.Service.ViewModels.Admin;
using MusicShop.Service.ViewModels.Cart;
using MusicShop.Service.ViewModels.Order;

namespace MusicShop.Service.Services.Interfaces
{
    /// <summary>
    /// 訂單商業邏輯介面
    /// </summary>
    public interface IOrderService
    {
        /// <summary>
        /// 從購物車建立訂單（包含完整收件人、配送、付款、發票資訊）
        /// 回傳訂單 ID，Controller 不應直接接觸 Order Entity
        /// </summary>
        /// <param name="userId">使用者 ID</param>
        /// <param name="checkoutInfo">結帳資訊</param>
        /// <returns>建立的訂單 ID</returns>
        Task<int> CreateOrderWithFullInfoAsync(string userId, CheckoutViewModel checkoutInfo);

        /// <summary>
        /// 取得使用者的訂單列表（返回 Entity，僅供 Service 層內部使用）
        /// Controller 應使用 GetOrderListViewModelsByUserAsync 取代
        /// </summary>
        Task<IEnumerable<Order>> GetUserOrdersAsync(string userId);

        /// <summary>
        /// 取得訂單詳細資訊（返回 Entity，僅供 Service 層內部使用）
        /// Controller 應使用 GetOrderDetailViewModelAsync 取代
        /// </summary>
        Task<Order?> GetOrderDetailAsync(int orderId, string userId);

        /// <summary>
        /// 取得訂單詳細資訊（管理員用，返回 Entity，僅供 Service 層內部使用）
        /// Controller 應使用 GetAdminOrderDetailViewModelAsync 取代
        /// </summary>
        Task<Order?> GetOrderByIdAsync(int orderId);

        /// <summary>
        /// 取得所有訂單（管理員用，返回 Entity，僅供 Service 層內部使用）
        /// Controller 應使用 GetAdminOrderListViewModelsAsync 取代
        /// </summary>
        Task<IEnumerable<Order>> GetAllOrdersAsync();

        /// <summary>
        /// 更新訂單狀態
        /// </summary>
        Task UpdateOrderStatusAsync(int orderId, OrderStatus status);

        /// <summary>
        /// 取消訂單
        /// </summary>
        Task CancelOrderAsync(int orderId, string userId);

        /// <summary>
        /// 檢查訂單是否屬於該使用者
        /// </summary>
        Task<bool> IsOrderOwnedByUserAsync(int orderId, string userId);

        /// <summary>
        /// 取得使用者訂單列表 ViewModel（供展示層使用）
        /// </summary>
        Task<List<OrderListItemViewModel>> GetOrderListViewModelsByUserAsync(string userId);

        /// <summary>
        /// 取得訂單詳情 ViewModel（供展示層使用，含使用者驗證）
        /// </summary>
        Task<OrderDetailViewModel?> GetOrderDetailViewModelAsync(int orderId, string userId);

        /// <summary>
        /// 取得訂單完成確認 ViewModel（供展示層使用）
        /// </summary>
        Task<OrderConfirmationViewModel?> GetOrderConfirmationViewModelAsync(int orderId, string userId);

        /// <summary>
        /// 取得最近訂單 ViewModel（供帳號首頁使用）
        /// </summary>
        Task<List<MusicShop.Service.ViewModels.Account.RecentOrderViewModel>> GetRecentOrderViewModelsAsync(string userId, int count = 5);

        /// <summary>
        /// 取得後台訂單列表 ViewModel（管理員用）
        /// </summary>
        Task<List<AdminOrderListItemViewModel>> GetAdminOrderListViewModelsAsync();

        /// <summary>
        /// 取得後台訂單詳情 ViewModel（管理員用，不驗證使用者）
        /// </summary>
        Task<AdminOrderDetailViewModel?> GetAdminOrderDetailViewModelAsync(int orderId);

        /// <summary>
        /// 取得使用者訂單統計（總訂單數、總消費金額）
        /// </summary>
        Task<(int TotalOrders, decimal TotalSpent)> GetUserOrderStatsAsync(string userId);
    }
}
