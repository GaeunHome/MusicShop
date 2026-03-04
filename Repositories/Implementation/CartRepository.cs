using Microsoft.EntityFrameworkCore;
using MusicShop.Data;
using MusicShop.Models;
using MusicShop.Repositories.Interface;

namespace MusicShop.Repositories.Implementation
{
    /// <summary>
    /// 購物車資料存取實作
    /// </summary>
    public class CartRepository : ICartRepository
    {
        private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;

        public CartRepository(IDbContextFactory<ApplicationDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public async Task<IEnumerable<CartItem>> GetCartItemsByUserIdAsync(string userId)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            return await context.CartItems
                .Include(c => c.Album)
                    .ThenInclude(a => a!.ArtistCategory)
                .Include(c => c.Album)
                    .ThenInclude(a => a!.ProductType)
                .Where(c => c.UserId == userId)
                .OrderByDescending(c => c.AddedAt)
                .ToListAsync();
        }

        public async Task<CartItem?> GetCartItemByIdAsync(int id)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            return await context.CartItems
                .Include(c => c.Album)
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<CartItem?> GetCartItemByUserAndAlbumAsync(string userId, int albumId)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            return await context.CartItems
                .FirstOrDefaultAsync(c => c.UserId == userId && c.AlbumId == albumId);
        }

        public async Task<CartItem> AddToCartAsync(CartItem cartItem)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            context.CartItems.Add(cartItem);
            await context.SaveChangesAsync();
            return cartItem;
        }

        public async Task UpdateCartItemAsync(CartItem cartItem)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            context.CartItems.Update(cartItem);
            await context.SaveChangesAsync();
        }

        public async Task RemoveCartItemAsync(int id)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            var cartItem = await context.CartItems.FindAsync(id);
            if (cartItem != null)
            {
                context.CartItems.Remove(cartItem);
                await context.SaveChangesAsync();
            }
        }

        public async Task ClearCartAsync(string userId)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            var cartItems = await context.CartItems
                .Where(c => c.UserId == userId)
                .ToListAsync();

            context.CartItems.RemoveRange(cartItems);
            await context.SaveChangesAsync();
        }

        public async Task<bool> CartItemExistsAsync(int id)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            return await context.CartItems.AnyAsync(c => c.Id == id);
        }
    }
}
