using Microsoft.EntityFrameworkCore;
using MusicShop.Data.Entities;
using MusicShop.Data.Repositories.Interfaces;

namespace MusicShop.Data.Repositories.Implementation
{
    /// <summary>
    /// 收藏清單資料存取實作
    /// </summary>
    public class WishlistRepository : IWishlistRepository
    {
        private readonly ApplicationDbContext _context;

        public WishlistRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<WishlistItem>> GetByUserIdAsync(string userId)
        {
            return await _context.WishlistItems
                .Include(w => w.Album)
                    .ThenInclude(a => a!.Artist)
                .Where(w => w.UserId == userId)
                .OrderByDescending(w => w.AddedAt)
                .ToListAsync();
        }

        public async Task<WishlistItem?> GetByUserAndAlbumAsync(string userId, int albumId)
        {
            return await _context.WishlistItems
                .FirstOrDefaultAsync(w => w.UserId == userId && w.AlbumId == albumId);
        }

        public async Task<HashSet<int>> GetAlbumIdsByUserAsync(string userId)
        {
            var ids = await _context.WishlistItems
                .Where(w => w.UserId == userId)
                .Select(w => w.AlbumId)
                .ToListAsync();

            return ids.ToHashSet();
        }

        public async Task AddAsync(WishlistItem item)
        {
            _context.WishlistItems.Add(item);
            await _context.SaveChangesAsync();
        }

        public async Task RemoveAsync(WishlistItem item)
        {
            _context.WishlistItems.Remove(item);
            await _context.SaveChangesAsync();
        }
    }
}
