using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using MusicShop.Data.Entities;
using MusicShop.Service.Services.Interfaces;
using MusicShop.Service.ViewModels.Album;
using MusicShop.Web.Models;

namespace MusicShop.Controllers;

/// <summary>
/// 首頁控制器 - 展示層
/// </summary>
public class HomeController : Controller
{
    private readonly IAlbumService _albumService;

    public HomeController(IAlbumService albumService)
    {
        _albumService = albumService;
    }

    public async Task<IActionResult> Index()
    {
        // 從服務層取得最新上架的 8 個專輯（首頁展示用）
        var latestAlbums = await _albumService.GetLatestAlbumsAsync(8);

        // 轉換為 AlbumCardViewModel
        var viewModel = latestAlbums
            .Select(album => new AlbumCardViewModel { Album = album })
            .ToList();

        return View(viewModel);
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
