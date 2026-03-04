using Microsoft.EntityFrameworkCore;
using MusicShop.Data;
using MusicShop.Models;
using MusicShop.Repositories.Interface;

namespace MusicShop.Repositories.Implementation
{
    /// <summary>
    /// 訂單資料存取實作
    /// </summary>
    public class OrderRepository : IOrderRepository
    {
        private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;

        public OrderRepository(IDbContextFactory<ApplicationDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public async Task<IEnumerable<Order>> GetOrdersByUserIdAsync(string userId)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            return await context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Album)
                .Where(o => o.UserId == userId)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();
        }

        public async Task<Order?> GetOrderByIdAsync(int id)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            return await context.Orders
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Album)
                        .ThenInclude(a => a!.ArtistCategory)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Album)
                        .ThenInclude(a => a!.ProductType)
                .Include(o => o.User)
                .FirstOrDefaultAsync(o => o.Id == id);
        }

        public async Task<IEnumerable<Order>> GetAllOrdersAsync()
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            return await context.Orders
                .Include(o => o.User)
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Album)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();
        }

        public async Task<Order> CreateOrderAsync(Order order)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            context.Orders.Add(order);
            await context.SaveChangesAsync();
            return order;
        }

        public async Task UpdateOrderAsync(Order order)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            context.Orders.Update(order);
            await context.SaveChangesAsync();
        }

        public async Task DeleteOrderAsync(int id)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            var order = await context.Orders.FindAsync(id);
            if (order != null)
            {
                context.Orders.Remove(order);
                await context.SaveChangesAsync();
            }
        }

        public async Task<bool> OrderExistsAsync(int id)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            return await context.Orders.AnyAsync(o => o.Id == id);
        }

        public async Task<bool> IsOrderOwnedByUserAsync(int orderId, string userId)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            return await context.Orders
                .AnyAsync(o => o.Id == orderId && o.UserId == userId);
        }
    }
}
