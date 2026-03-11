using System.Diagnostics;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MusicShop.Data.Entities;
using MusicShop.Service.Services.Interfaces;
using MusicShop.Web.Models;

namespace MusicShop.Controllers;

/// <summary>
/// 首頁控制器 - 展示層
/// </summary>
public class HomeController : Controller
{
    private readonly IAlbumService _albumService;
    private readonly IBannerService _bannerService;
    private readonly IWishlistService _wishlistService;
    private readonly UserManager<AppUser> _userManager;

    public HomeController(IAlbumService albumService, IBannerService bannerService,
        IWishlistService wishlistService, UserManager<AppUser> userManager)
    {
        _albumService = albumService;
        _bannerService = bannerService;
        _wishlistService = wishlistService;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index()
    {
        // 取得首頁幻燈片（啟用中，依順序）
        ViewBag.Banners = await _bannerService.GetActiveBannersAsync();

        // 傳遞使用者收藏清單 ID（供商品卡片判斷愛心狀態）
        var userId = _userManager.GetUserId(User);
        ViewBag.WishlistIds = await _wishlistService.GetWishlistAlbumIdsAsync(userId ?? string.Empty);

        // 從服務層取得最新上架的 8 個專輯（首頁展示用）
        var latestAlbums = await _albumService.GetLatestAlbumsAsync(8);

        return View(latestAlbums.ToList());
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
