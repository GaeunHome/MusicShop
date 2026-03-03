using MusicShop.Models;

namespace MusicShop.Services.Interface
{
    /// <summary>
    /// 訂單商業邏輯介面
    /// </summary>
    public interface IOrderService
    {
        /// <summary>
        /// 從購物車建立訂單
        /// </summary>
        Task<Order> CreateOrderFromCartAsync(string userId);

        /// <summary>
        /// 取得使用者的訂單列表
        /// </summary>
        Task<IEnumerable<Order>> GetUserOrdersAsync(string userId);

        /// <summary>
        /// 取得訂單詳細資訊
        /// </summary>
        Task<Order?> GetOrderDetailAsync(int orderId, string userId);

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
