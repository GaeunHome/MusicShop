using Microsoft.EntityFrameworkCore;
using MusicShop.Data;
using MusicShop.Data.Entities;
using MusicShop.Data.Repositories.Interfaces;

namespace MusicShop.Data.Repositories.Implementation
{
    /// <summary>
    /// 購物車資料存取實作
    /// 注意：寫入操作不呼叫 SaveChangesAsync，由 UnitOfWork 統一管理。
    /// </summary>
    public class CartRepository : ICartRepository
    {
        private readonly ApplicationDbContext _context;

        public CartRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// 取得使用者的購物車項目（含專輯、藝人、商品類型資訊）
        /// </summary>
        public async Task<IEnumerable<CartItem>> GetCartItemsByUserIdAsync(string userId)
        {
            return await _context.CartItems
                .AsNoTracking()
                .Include(item => item.Album)
                    .ThenInclude(album => album!.Artist)
                        .ThenInclude(artist => artist!.ArtistCategory)
                .Include(item => item.Album)
                    .ThenInclude(album => album!.ProductType)
                .Where(item => item.UserId == userId)
                .OrderByDescending(item => item.AddedAt)
                .ToListAsync();
        }

        /// <summary>
        /// 根據 ID 取得購物車項目（保持追蹤以支援更新數量）
        /// </summary>
        public async Task<CartItem?> GetCartItemByIdAsync(int id)
        {
            return await _context.CartItems
                .Include(item => item.Album)
                .FirstOrDefaultAsync(item => item.Id == id);
        }

        /// <summary>
        /// 根據使用者和專輯 ID 查詢購物車項目（用於新增前檢查是否已存在）
        /// </summary>
        public async Task<CartItem?> GetCartItemByUserAndAlbumAsync(string userId, int albumId)
        {
            return await _context.CartItems
                .FirstOrDefaultAsync(item => item.UserId == userId && item.AlbumId == albumId);
        }

        public async Task<CartItem> AddToCartAsync(CartItem cartItem)
        {
            await _context.CartItems.AddAsync(cartItem);
            return cartItem;
        }

        public Task UpdateCartItemAsync(CartItem cartItem)
        {
            _context.CartItems.Update(cartItem);
            return Task.CompletedTask;
        }

        public async Task RemoveCartItemAsync(int id)
        {
            var targetItem = await _context.CartItems.FindAsync(id);
            if (targetItem != null)
            {
                _context.CartItems.Remove(targetItem);
            }
        }

        /// <summary>
        /// 清空使用者的整個購物車（結帳完成後呼叫）
        /// </summary>
        public async Task ClearCartAsync(string userId)
        {
            var userCartItems = await _context.CartItems
                .Where(item => item.UserId == userId)
                .ToListAsync();

            _context.CartItems.RemoveRange(userCartItems);
        }

        public async Task<bool> CartItemExistsAsync(int id)
        {
            return await _context.CartItems.AnyAsync(item => item.Id == id);
        }

        /// <summary>
        /// 計算使用者購物車總金額
        /// </summary>
        public async Task<decimal> GetCartTotalAsync(string userId)
        {
            return await _context.CartItems
                .Where(item => item.UserId == userId && item.Album != null)
                .SumAsync(item => item.Album!.Price * item.Quantity);
        }

        /// <summary>
        /// 計算使用者購物車商品總數量（用於導覽列徽章顯示）
        /// </summary>
        public async Task<int> GetCartItemCountAsync(string userId)
        {
            return await _context.CartItems
                .Where(item => item.UserId == userId)
                .SumAsync(item => item.Quantity);
        }
    }
}
