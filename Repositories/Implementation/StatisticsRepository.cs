using Microsoft.EntityFrameworkCore;
using MusicShop.Data;
using MusicShop.Models;
using MusicShop.Repositories.Interface;

namespace MusicShop.Repositories.Implementation;

/// <summary>
/// 統計資料存取實作
/// </summary>
public class StatisticsRepository : IStatisticsRepository
{
    private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;

    public StatisticsRepository(IDbContextFactory<ApplicationDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async Task<int> GetAlbumCountAsync()
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        return await context.Albums.CountAsync();
    }

    public async Task<int> GetCategoryCountAsync()
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        // 統計藝人分類和商品類型的總數
        var artistCategoryCount = await context.ArtistCategories.CountAsync();
        var productTypeCount = await context.ProductTypes.CountAsync();
        return artistCategoryCount + productTypeCount;
    }

    public async Task<int> GetOrderCountAsync()
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        return await context.Orders.CountAsync();
    }

    public async Task<int> GetUserCountAsync()
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        return await context.Users.CountAsync();
    }

    public async Task<decimal> GetTotalSalesAsync()
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        var completedOrders = await context.Orders
            .Where(o => o.Status == OrderStatus.Completed || o.Status == OrderStatus.Paid)
            .ToListAsync();

        return completedOrders.Sum(o => o.TotalAmount);
    }

    public async Task<int> GetPendingOrderCountAsync()
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        return await context.Orders
            .CountAsync(o => o.Status == OrderStatus.Pending);
    }
}
