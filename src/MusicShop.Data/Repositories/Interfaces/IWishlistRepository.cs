using MusicShop.Data.Entities;

namespace MusicShop.Data.Repositories.Interfaces
{
    /// <summary>
    /// 收藏清單資料存取介面
    /// </summary>
    public interface IWishlistRepository
    {
        /// <summary>取得使用者的完整收藏清單（含 Album、Artist）</summary>
        Task<IEnumerable<WishlistItem>> GetByUserIdAsync(string userId);

        /// <summary>取得特定使用者 + 專輯的收藏項目（用於判斷是否已收藏）</summary>
        Task<WishlistItem?> GetByUserAndAlbumAsync(string userId, int albumId);

        /// <summary>取得使用者所有已收藏的專輯 ID（用於在列表頁高亮愛心）</summary>
        Task<HashSet<int>> GetAlbumIdsByUserAsync(string userId);

        /// <summary>新增收藏</summary>
        Task AddAsync(WishlistItem item);

        /// <summary>移除收藏</summary>
        Task RemoveAsync(WishlistItem item);
    }
}
