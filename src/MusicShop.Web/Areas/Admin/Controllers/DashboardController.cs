// ─────────────────────────────────────────────────────────────
// DashboardController.cs - 後台管理首頁
// Area: Admin
// ─────────────────────────────────────────────────────────────

using Microsoft.AspNetCore.Mvc;
using MusicShop.Service.Services.Interfaces;

namespace MusicShop.Web.Areas.Admin.Controllers;

/// <summary>
/// 後台首頁控制器，顯示統計數據儀表板
/// </summary>
public class DashboardController : AdminBaseController
{
    private readonly IStatisticsService _statisticsService;

    public DashboardController(IStatisticsService statisticsService)
    {
        _statisticsService = statisticsService;
    }

    public async Task<IActionResult> Index()
    {
        var stats = await _statisticsService.GetDashboardStatsAsync();

        ViewBag.AlbumCount = stats.AlbumCount;
        ViewBag.ArtistCount = stats.ArtistCount;
        ViewBag.CategoryCount = stats.CategoryCount;
        ViewBag.OrderCount = stats.OrderCount;
        ViewBag.UserCount = stats.UserCount;
        ViewBag.TotalSales = stats.TotalSales;
        ViewBag.PendingOrderCount = stats.PendingOrderCount;
        ViewBag.BannerCount = stats.BannerCount;
        ViewBag.FeaturedArtistCount = stats.FeaturedArtistCount;
        ViewBag.CouponCount = stats.CouponCount;
        return View();
    }

    /// <summary>
    /// 取得銷售趨勢資料（供 Chart.js 使用）
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> SalesTrend(int days = 30)
    {
        if (days < 7) days = 7;
        if (days > 365) days = 365;

        var trend = await _statisticsService.GetDailySalesTrendAsync(days);

        // 補齊沒有訂單的日期（填 0）
        var startDate = DateTime.UtcNow.Date.AddDays(-days);
        var allDates = Enumerable.Range(0, days + 1)
            .Select(i => startDate.AddDays(i))
            .ToList();

        var trendDict = trend.ToDictionary(t => t.Date, t => t);

        var labels = allDates.Select(d => d.ToString("MM/dd")).ToList();
        var amounts = allDates.Select(d => trendDict.TryGetValue(d, out var v) ? v.Amount : 0m).ToList();
        var counts = allDates.Select(d => trendDict.TryGetValue(d, out var v) ? v.Count : 0).ToList();

        return Json(new { labels, amounts, counts });
    }

    /// <summary>
    /// 取得熱門商品排行資料（供 Chart.js 使用）
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> TopSellingAlbums(int count = 10)
    {
        if (count < 5) count = 5;
        if (count > 20) count = 20;

        var topAlbums = await _statisticsService.GetTopSellingAlbumsAsync(count);

        var labels = topAlbums.Select(a => a.AlbumTitle).ToList();
        var quantities = topAlbums.Select(a => a.Quantity).ToList();

        return Json(new { labels, quantities });
    }
}
