using Microsoft.EntityFrameworkCore;
using MusicShop.Data;
using MusicShop.Data.Entities;
using MusicShop.Data.Repositories.Interfaces;
using MusicShop.Library.Enums;

namespace MusicShop.Data.Repositories.Implementation;

/// <summary>
/// 統計資料存取實作
/// </summary>
public class StatisticsRepository : IStatisticsRepository
{
    private readonly ApplicationDbContext _context;

    public StatisticsRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<int> GetAlbumCountAsync()
    {
        return await _context.Albums.CountAsync();
    }

    public async Task<int> GetCategoryCountAsync()
    {
        // 統計藝人分類和商品類型的總數
        var artistCategoryCount = await _context.ArtistCategories.CountAsync();
        var productTypeCount = await _context.ProductTypes.CountAsync();
        return artistCategoryCount + productTypeCount;
    }

    public async Task<int> GetOrderCountAsync()
    {
        return await _context.Orders.CountAsync();
    }

    public async Task<int> GetUserCountAsync()
    {
        return await _context.Users.CountAsync();
    }

    public async Task<decimal> GetTotalSalesAsync()
    {
        return await _context.Orders
            .Where(o => o.Status == OrderStatus.Completed || o.Status == OrderStatus.Paid)
            .SumAsync(o => o.TotalAmount);
    }

    public async Task<int> GetPendingOrderCountAsync()
    {
        return await _context.Orders
            .CountAsync(o => o.Status == OrderStatus.Pending);
    }

    public async Task<int> GetArtistCountAsync()
    {
        return await _context.Artists.CountAsync();
    }

    public async Task<int> GetBannerCountAsync()
    {
        return await _context.Banners.CountAsync();
    }

    public async Task<int> GetFeaturedArtistCountAsync()
    {
        return await _context.FeaturedArtists.CountAsync();
    }

    public async Task<int> GetCouponCountAsync()
    {
        return await _context.Coupons.CountAsync();
    }

    public async Task<List<(DateTime Date, decimal Amount, int Count)>> GetDailySalesTrendAsync(int days)
    {
        var startDate = DateTime.UtcNow.Date.AddDays(-days);

        // 先查詢 DB 取得原始資料，再在記憶體中分組
        var orders = await _context.Orders
            .AsNoTracking()
            .Where(o => o.OrderDate >= startDate
                     && o.Status != OrderStatus.Cancelled)
            .Select(o => new { o.OrderDate, o.TotalAmount })
            .ToListAsync();

        return orders
            .GroupBy(o => o.OrderDate.Date)
            .Select(g => (Date: g.Key, Amount: g.Sum(o => o.TotalAmount), Count: g.Count()))
            .OrderBy(x => x.Date)
            .ToList();
    }

    public async Task<List<(string AlbumTitle, int Quantity)>> GetTopSellingAlbumsAsync(int count)
    {
        return await _context.OrderItems
            .AsNoTracking()
            .Include(oi => oi.Album)
            .Where(oi => oi.Order!.Status != OrderStatus.Cancelled)
            .GroupBy(oi => new { oi.AlbumId, oi.Album!.Title })
            .Select(g => new { AlbumTitle = g.Key.Title, Quantity = g.Sum(oi => oi.Quantity) })
            .OrderByDescending(x => x.Quantity)
            .Take(count)
            .Select(x => ValueTuple.Create(x.AlbumTitle, x.Quantity))
            .ToListAsync();
    }
}
