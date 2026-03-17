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
}
