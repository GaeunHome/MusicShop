using MusicShop.Models;
using MusicShop.ViewModels.Cart;

namespace MusicShop.Services.Interface
{
    /// <summary>
    /// 訂單商業邏輯介面
    /// </summary>
    public interface IOrderService
    {
        /// <summary>
        /// 從購物車建立訂單（舊版，不含完整資訊）
        /// </summary>
        [Obsolete("請使用 CreateOrderWithFullInfoAsync 方法")]
        Task<Order> CreateOrderFromCartAsync(string userId);

        /// <summary>
        /// 從購物車建立訂單（包含完整收件人、配送、付款、發票資訊）
        /// </summary>
        /// <param name="userId">使用者 ID</param>
        /// <param name="checkoutInfo">結帳資訊</param>
        /// <returns>建立的訂單</returns>
        Task<Order> CreateOrderWithFullInfoAsync(string userId, CheckoutViewModel checkoutInfo);

        /// <summary>
        /// 取得使用者的訂單列表
        /// </summary>
        Task<IEnumerable<Order>> GetUserOrdersAsync(string userId);

        /// <summary>
        /// 取得訂單詳細資訊
        /// </summary>
        Task<Order?> GetOrderDetailAsync(int orderId, string userId);

        /// <summary>
        /// 取得訂單詳細資訊（管理員用，不驗證使用者）
        /// </summary>
        Task<Order?> GetOrderByIdAsync(int orderId);

        /// <summary>
        /// 取得所有訂單（管理員用）
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
    }
}
