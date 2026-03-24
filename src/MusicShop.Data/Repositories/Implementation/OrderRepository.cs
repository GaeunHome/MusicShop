using Microsoft.EntityFrameworkCore;
using MusicShop.Data;
using MusicShop.Data.Entities;
using MusicShop.Data.Repositories.Interfaces;
using MusicShop.Library.Enums;

namespace MusicShop.Data.Repositories.Implementation
{
    /// <summary>
    /// 訂單資料存取實作
    /// 注意：寫入操作不呼叫 SaveChangesAsync，由 UnitOfWork 統一管理。
    /// </summary>
    public class OrderRepository : IOrderRepository
    {
        private readonly ApplicationDbContext _context;

        public OrderRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// 取得使用者的訂單列表（含訂單項目與專輯），依下單時間倒序
        /// </summary>
        public async Task<IEnumerable<Order>> GetOrdersByUserIdAsync(string userId)
        {
            return await _context.Orders
                .AsNoTracking()
                .Include(order => order.OrderItems)
                    .ThenInclude(orderItem => orderItem.Album)
                .Where(order => order.UserId == userId)
                .OrderByDescending(order => order.OrderDate)
                .ToListAsync();
        }

        /// <summary>
        /// 根據 ID 取得訂單詳情（含完整關聯：訂單項目→專輯→藝人→分類、使用者）
        /// 保持追蹤以支援狀態更新
        /// </summary>
        public async Task<Order?> GetOrderByIdAsync(int id)
        {
            return await _context.Orders
                .Include(order => order.OrderItems)
                    .ThenInclude(orderItem => orderItem.Album)
                        .ThenInclude(album => album!.Artist)
                            .ThenInclude(artist => artist!.ArtistCategory)
                .Include(order => order.OrderItems)
                    .ThenInclude(orderItem => orderItem.Album)
                        .ThenInclude(album => album!.ProductType)
                .Include(order => order.User)
                .Include(order => order.UserCoupon)
                    .ThenInclude(uc => uc!.Coupon)
                .FirstOrDefaultAsync(order => order.Id == id);
        }

        /// <summary>
        /// 取得所有訂單（後台管理用，含使用者與訂單項目）
        /// </summary>
        public async Task<IEnumerable<Order>> GetAllOrdersAsync()
        {
            return await _context.Orders
                .AsNoTracking()
                .Include(order => order.User)
                .Include(order => order.OrderItems)
                    .ThenInclude(orderItem => orderItem.Album)
                .OrderByDescending(order => order.OrderDate)
                .ToListAsync();
        }

        /// <summary>
        /// 取得分頁訂單列表（後台管理用），依下單時間倒序
        /// </summary>
        public async Task<(IEnumerable<Order> Orders, int TotalCount)> GetOrdersPagedAsync(int page, int pageSize)
        {
            var query = _context.Orders
                .AsNoTracking()
                .Include(order => order.User)
                .Include(order => order.OrderItems)
                    .ThenInclude(orderItem => orderItem.Album)
                .OrderByDescending(order => order.OrderDate);

            var totalCount = await query.CountAsync();

            var orders = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (orders, totalCount);
        }

        public async Task<Order> CreateOrderAsync(Order order)
        {
            await _context.Orders.AddAsync(order);
            return order;
        }

        public Task UpdateOrderAsync(Order order)
        {
            _context.Orders.Update(order);
            return Task.CompletedTask;
        }

        public async Task DeleteOrderAsync(int id)
        {
            var targetOrder = await _context.Orders.FindAsync(id);
            if (targetOrder != null)
            {
                _context.Orders.Remove(targetOrder);
            }
        }

        public async Task<bool> OrderExistsAsync(int id)
        {
            return await _context.Orders.AnyAsync(order => order.Id == id);
        }

        /// <summary>
        /// 驗證訂單是否屬於指定使用者（權限檢查用）
        /// </summary>
        public async Task<bool> IsOrderOwnedByUserAsync(int orderId, string userId)
        {
            return await _context.Orders
                .AnyAsync(order => order.Id == orderId && order.UserId == userId);
        }

        /// <summary>
        /// 根據綠界交易編號查詢訂單（含訂單項目與專輯）
        /// </summary>
        public async Task<Order?> GetOrderByMerchantTradeNoAsync(string merchantTradeNo)
        {
            return await _context.Orders
                .Include(order => order.OrderItems)
                    .ThenInclude(orderItem => orderItem.Album)
                .FirstOrDefaultAsync(order => order.MerchantTradeNo == merchantTradeNo);
        }

        /// <summary>
        /// 取得使用者的訂單統計（排除已取消訂單）
        /// </summary>
        /// <remarks>
        /// 此處拆為兩次獨立查詢（COUNT + SUM），因為 EF Core 不支援在單一查詢中
        /// 同時回傳 Count 與 Sum 的匿名型別投影至 ValueTuple。
        /// 目前每位使用者的訂單量不大，兩次查詢的效能影響可忽略；
        /// 若未來訂單量大幅增長，可考慮改用原生 SQL 或 GroupBy 投影合併為單次查詢。
        /// </remarks>
        public async Task<(int TotalOrders, decimal TotalSpent)> GetUserOrderStatsAsync(string userId)
        {
            var orderCount = await _context.Orders
                .CountAsync(order => order.UserId == userId && order.Status != OrderStatus.Cancelled);

            var spentAmount = await _context.Orders
                .Where(order => order.UserId == userId && order.Status != OrderStatus.Cancelled)
                .SumAsync(order => order.TotalAmount);

            return (orderCount, spentAmount);
        }
    }
}
