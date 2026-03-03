using MusicShop.Models;

namespace MusicShop.Repositories.Interface
{
    /// <summary>
    /// 訂單資料存取介面
    /// </summary>
    public interface IOrderRepository
    {
        /// <summary>
        /// 取得使用者的訂單列表
        /// </summary>
        Task<IEnumerable<Order>> GetOrdersByUserIdAsync(string userId);

        /// <summary>
        /// 取得訂單詳細資訊（包含訂單項目）
        /// </summary>
        Task<Order?> GetOrderByIdAsync(int id);

        /// <summary>
        /// 取得所有訂單（管理員用）
        /// </summary>
        Task<IEnumerable<Order>> GetAllOrdersAsync();

        /// <summary>
        /// 建立訂單
        /// </summary>
        Task<Order> CreateOrderAsync(Order order);

        /// <summary>
        /// 更新訂單
        /// </summary>
        Task UpdateOrderAsync(Order order);

        /// <summary>
        /// 刪除訂單
        /// </summary>
        Task DeleteOrderAsync(int id);

        /// <summary>
        /// 檢查訂單是否存在
        /// </summary>
        Task<bool> OrderExistsAsync(int id);

        /// <summary>
        /// 檢查訂單是否屬於該使用者
        /// </summary>
        Task<bool> IsOrderOwnedByUserAsync(int orderId, string userId);
    }
}
