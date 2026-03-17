using MusicShop.Data.Entities;

namespace MusicShop.Data.Repositories.Interfaces
{
    /// <summary>
    /// 購物車資料存取介面
    /// </summary>
    public interface ICartRepository
    {
        /// <summary>
        /// 取得使用者的購物車項目
        /// </summary>
        Task<IEnumerable<CartItem>> GetCartItemsByUserIdAsync(string userId);

        /// <summary>
        /// 取得特定購物車項目
        /// </summary>
        Task<CartItem?> GetCartItemByIdAsync(int id);

        /// <summary>
        /// 檢查使用者購物車中是否已有該專輯
        /// </summary>
        Task<CartItem?> GetCartItemByUserAndAlbumAsync(string userId, int albumId);

        /// <summary>
        /// 加入商品到購物車
        /// </summary>
        Task<CartItem> AddToCartAsync(CartItem cartItem);

        /// <summary>
        /// 更新購物車項目數量
        /// </summary>
        Task UpdateCartItemAsync(CartItem cartItem);

        /// <summary>
        /// 移除購物車項目
        /// </summary>
        Task RemoveCartItemAsync(int id);

        /// <summary>
        /// 清空使用者購物車
        /// </summary>
        Task ClearCartAsync(string userId);

        /// <summary>
        /// 檢查購物車項目是否存在
        /// </summary>
        Task<bool> CartItemExistsAsync(int id);

        /// <summary>
        /// 取得使用者購物車總金額
        /// </summary>
        Task<decimal> GetCartTotalAsync(string userId);

        /// <summary>
        /// 取得使用者購物車商品總數量
        /// </summary>
        Task<int> GetCartItemCountAsync(string userId);
    }
}
