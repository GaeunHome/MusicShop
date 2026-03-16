using MusicShop.Data.Entities;
using MusicShop.Service.ViewModels.Wishlist;

namespace MusicShop.Service.Services.Interfaces
{
    /// <summary>
    /// 收藏清單業務邏輯介面
    /// </summary>
    public interface IWishlistService
    {
        /// <summary>取得使用者的完整收藏清單（Entity，內部使用）</summary>
        Task<IEnumerable<WishlistItem>> GetWishlistAsync(string userId);

        /// <summary>取得使用者的完整收藏清單 ViewModel（供展示層使用）</summary>
        Task<IEnumerable<WishlistItemViewModel>> GetWishlistItemViewModelsAsync(string userId);

        /// <summary>
        /// 切換收藏狀態（加入 or 移除）
        /// </summary>
        /// <returns>true = 已加入收藏；false = 已取消收藏</returns>
        Task<bool> ToggleWishlistAsync(string userId, int albumId);

        /// <summary>取得使用者所有已收藏的專輯 ID（供列表頁高亮愛心使用）</summary>
        Task<HashSet<int>> GetWishlistAlbumIdsAsync(string userId);
    }
}
