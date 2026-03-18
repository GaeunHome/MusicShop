using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using MusicShop.Service.Services.Interfaces;
using MusicShop.Web.Infrastructure;

namespace MusicShop.Controllers;

/// <summary>
/// 首頁控制器 - 展示層
/// </summary>
public class HomeController : BaseController
{
    private readonly IAlbumService _albumService;
    private readonly IBannerService _bannerService;
    private readonly IWishlistService _wishlistService;
    private readonly IFeaturedArtistService _featuredArtistService;

    public HomeController(IAlbumService albumService, IBannerService bannerService,
        IWishlistService wishlistService, IFeaturedArtistService featuredArtistService)
    {
        _albumService = albumService;
        _bannerService = bannerService;
        _wishlistService = wishlistService;
        _featuredArtistService = featuredArtistService;
    }

    public async Task<IActionResult> Index()
    {
        // 取得首頁幻燈片 ViewModel（啟用中，依順序）
        ViewBag.Banners = await _bannerService.GetActiveBannerDisplaysAsync();

        // 傳遞使用者收藏清單 ID（供商品卡片判斷愛心狀態）
        var userId = GetCurrentUserId();
        ViewBag.WishlistIds = await _wishlistService.GetWishlistAlbumIdsAsync(userId ?? string.Empty);

        // 取得精選藝人特區資料
        ViewBag.FeaturedArtists = await _featuredArtistService.GetActiveFeaturedArtistDisplaysAsync();

        // 從服務層取得最新上架的 8 個專輯 ViewModel（首頁展示用）
        var latestAlbums = await _albumService.GetLatestAlbumCardsAsync(8);

        return View(latestAlbums.ToList());
    }

    public IActionResult Privacy()
    {
        return View();
    }

    /// <summary>
    /// 錯誤頁面 - 根據狀態碼顯示對應的錯誤訊息
    /// </summary>
    /// <param name="statusCode">HTTP 狀態碼（可選，預設為 500）</param>
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error(int? statusCode = null)
    {
        var model = new ErrorViewModel
        {
            RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
        };

        ViewBag.StatusCode = statusCode ?? 500;
        ViewBag.ErrorTitle = statusCode switch
        {
            400 => "操作無效",
            403 => "存取被拒絕",
            404 => "找不到頁面",
            409 => "資料衝突",
            429 => "請求過於頻繁",
            _ => "系統發生錯誤"
        };
        ViewBag.ErrorMessage = statusCode switch
        {
            400 => "您的請求無法處理，請檢查輸入內容後重試。",
            403 => "您沒有權限存取此頁面。",
            404 => "您要找的頁面不存在或已被移除。",
            409 => "此資料已被其他人修改，請重新整理頁面後再試。",
            429 => "您的請求過於頻繁，請稍後再試。",
            _ => "很抱歉，系統發生未預期的錯誤，請稍後再試。"
        };

        return View(model);
    }
}
