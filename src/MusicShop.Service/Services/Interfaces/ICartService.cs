using MusicShop.Data.Entities;
using MusicShop.Service.ViewModels.Cart;

namespace MusicShop.Service.Services.Interfaces
{
    /// <summary>
    /// 購物車商業邏輯介面
    /// </summary>
    public interface ICartService
    {
        /// <summary>
        /// 取得使用者的購物車項目（Entity，供 Service 層使用）
        /// </summary>
        Task<IEnumerable<CartItem>> GetUserCartAsync(string userId);

        /// <summary>
        /// 取得使用者的購物車項目 ViewModel（供展示層使用）
        /// </summary>
        Task<List<CartItemViewModel>> GetCartItemViewModelsAsync(string userId);

        /// <summary>
        /// 加入商品到購物車（若已存在則增加數量）
        /// </summary>
        Task<CartItem> AddToCartAsync(string userId, int albumId, int quantity = 1);

        /// <summary>
        /// 更新購物車項目數量
        /// </summary>
        Task UpdateCartItemQuantityAsync(int cartItemId, string userId, int quantity);

        /// <summary>
        /// 更新購物車項目數量（AJAX 版本，返回更新結果）
        /// </summary>
        /// <param name="cartItemId">購物車項目 ID</param>
        /// <param name="userId">使用者 ID</param>
        /// <param name="quantity">新數量</param>
        /// <returns>包含更新後資訊的結果</returns>
        Task<CartUpdateResult> UpdateCartItemQuantityAjaxAsync(int cartItemId, string userId, int quantity);

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
