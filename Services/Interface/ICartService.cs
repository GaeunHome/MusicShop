using MusicShop.Models;

namespace MusicShop.Services.Interface
{
    /// <summary>
    /// 購物車商業邏輯介面
    /// </summary>
    public interface ICartService
    {
        /// <summary>
        /// 取得使用者的購物車項目
        /// </summary>
        Task<IEnumerable<CartItem>> GetUserCartAsync(string userId);

        /// <summary>
        /// 加入商品到購物車（若已存在則增加數量）
        /// </summary>
        Task<CartItem> AddToCartAsync(string userId, int albumId, int quantity = 1);

        /// <summary>
        /// 更新購物車項目數量
        /// </summary>
        Task UpdateCartItemQuantityAsync(int cartItemId, string userId, int quantity);

        /// <summary>
        /// 移除購物車項目
        /// </summary>
        Task RemoveFromCartAsync(int cartItemId, string userId);

        /// <summary>
        /// 清空使用者購物車
        /// </summary>
        Task ClearCartAsync(string userId);

        /// <summary>
        /// 計算購物車總金額
        /// </summary>
        Task<decimal> GetCartTotalAsync(string userId);

        /// <summary>
        /// 取得購物車項目數量
        /// </summary>
        Task<int> GetCartItemCountAsync(string userId);
    }
}
