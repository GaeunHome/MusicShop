using Microsoft.EntityFrameworkCore;
using MusicShop.Data.Entities;
using MusicShop.Data.Repositories.Interfaces;

namespace MusicShop.Data.Repositories.Implementation
{
    /// <summary>
    /// 收藏清單資料存取實作
    /// 注意：寫入操作不呼叫 SaveChangesAsync，由 UnitOfWork 統一管理。
    /// </summary>
    public class WishlistRepository : IWishlistRepository
    {
        private readonly ApplicationDbContext _context;

        public WishlistRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// 取得使用者的收藏清單（含專輯與藝人資訊）
        /// </summary>
        public async Task<IEnumerable<WishlistItem>> GetByUserIdAsync(string userId)
        {
            return await _context.WishlistItems
                .AsNoTracking()
                .Include(wishItem => wishItem.Album)
                    .ThenInclude(album => album!.Artist)
                .Where(wishItem => wishItem.UserId == userId && wishItem.Album != null)
                .OrderByDescending(wishItem => wishItem.AddedAt)
                .ToListAsync();
        }

        /// <summary>
        /// 查詢使用者是否已收藏指定專輯（保持追蹤以支援移除）
        /// </summary>
        public async Task<WishlistItem?> GetByUserAndAlbumAsync(string userId, int albumId)
        {
            return await _context.WishlistItems
                .FirstOrDefaultAsync(wishItem => wishItem.UserId == userId && wishItem.AlbumId == albumId);
        }

        /// <summary>
        /// 取得使用者收藏的所有專輯 ID（用於頁面上快速判斷愛心狀態）
        /// </summary>
        /// <remarks>
        /// 回傳 HashSet 而非 List，因為呼叫端需要對每張專輯卡片判斷「是否已收藏」，
        /// HashSet 提供 O(1) 的 Contains 查找，避免在商品列表頁（可能數十筆）逐一線性搜尋。
        /// </remarks>
        public async Task<HashSet<int>> GetAlbumIdsByUserAsync(string userId)
        {
            var albumIds = await _context.WishlistItems
                .Where(wishItem => wishItem.UserId == userId)
                .Select(wishItem => wishItem.AlbumId)
                .ToListAsync();

            return albumIds.ToHashSet();
        }

        public async Task AddAsync(WishlistItem item)
        {
            await _context.WishlistItems.AddAsync(item);
        }

        public Task RemoveAsync(WishlistItem item)
        {
            _context.WishlistItems.Remove(item);
            return Task.CompletedTask;
        }
    }
}
