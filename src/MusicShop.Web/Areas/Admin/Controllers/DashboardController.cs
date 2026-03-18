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
        ViewBag.AlbumCount = await _statisticsService.GetAlbumCountAsync();
        ViewBag.ArtistCount = await _statisticsService.GetArtistCountAsync();
        ViewBag.CategoryCount = await _statisticsService.GetCategoryCountAsync();
        ViewBag.OrderCount = await _statisticsService.GetOrderCountAsync();
        ViewBag.UserCount = await _statisticsService.GetUserCountAsync();
        ViewBag.TotalSales = await _statisticsService.GetTotalSalesAsync();
        ViewBag.PendingOrderCount = await _statisticsService.GetPendingOrderCountAsync();
        ViewBag.BannerCount = await _statisticsService.GetBannerCountAsync();
        ViewBag.FeaturedArtistCount = await _statisticsService.GetFeaturedArtistCountAsync();
        ViewBag.CouponCount = await _statisticsService.GetCouponCountAsync();
        return View();
    }
}
