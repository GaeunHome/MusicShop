using Microsoft.EntityFrameworkCore;
using MusicShop.Data;
using MusicShop.Data.Entities;
using MusicShop.Data.Repositories.Interfaces;

namespace MusicShop.Data.Repositories.Implementation
{
    /// <summary>
    /// 訂單資料存取實作
    /// </summary>
    public class OrderRepository : IOrderRepository
    {
        private readonly ApplicationDbContext _context;

        public OrderRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Order>> GetOrdersByUserIdAsync(string userId)
        {
            return await _context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Album)
                .Where(o => o.UserId == userId)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();
        }

        public async Task<Order?> GetOrderByIdAsync(int id)
        {
            return await _context.Orders
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Album)
                        .ThenInclude(a => a!.Artist)
                            .ThenInclude(ar => ar.ArtistCategory)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Album)
                        .ThenInclude(a => a!.ProductType)
                .Include(o => o.User)
                .FirstOrDefaultAsync(o => o.Id == id);
        }

        public async Task<IEnumerable<Order>> GetAllOrdersAsync()
        {
            return await _context.Orders
                .Include(o => o.User)
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Album)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();
        }

        public async Task<Order> CreateOrderAsync(Order order)
        {
            _context.Orders.Add(order);
            await _context.SaveChangesAsync();
            return order;
        }

        public async Task<Order> CreateOrderWithTransactionAsync(Order order, string userId)
        {
            // 使用共用的 DbContext 進行交易操作
            await using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // 1. 建立訂單
                _context.Orders.Add(order);
                await _context.SaveChangesAsync();

                // 2. 扣除庫存（在同一個交易中）
                foreach (var orderItem in order.OrderItems)
                {
                    var album = await _context.Albums.FindAsync(orderItem.AlbumId);
                    if (album == null)
                    {
                        throw new InvalidOperationException($"專輯不存在，ID: {orderItem.AlbumId}");
                    }

                    if (album.Stock < orderItem.Quantity)
                    {
                        throw new InvalidOperationException($"專輯「{album.Title}」庫存不足");
                    }

                    album.Stock -= orderItem.Quantity;
                }
                await _context.SaveChangesAsync();

                // 3. 清空購物車（在同一個交易中）
                var cartItems = await _context.CartItems
                    .Where(c => c.UserId == userId)
                    .ToListAsync();
                _context.CartItems.RemoveRange(cartItems);
                await _context.SaveChangesAsync();

                // 4. 提交交易
                await transaction.CommitAsync();

                return order;
            }
            catch
            {
                // 發生錯誤時回滾交易
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task UpdateOrderAsync(Order order)
        {
            _context.Orders.Update(order);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteOrderAsync(int id)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order != null)
            {
                _context.Orders.Remove(order);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> OrderExistsAsync(int id)
        {
            return await _context.Orders.AnyAsync(o => o.Id == id);
        }

        public async Task<bool> IsOrderOwnedByUserAsync(int orderId, string userId)
        {
            return await _context.Orders
                .AnyAsync(o => o.Id == orderId && o.UserId == userId);
        }
    }
}
