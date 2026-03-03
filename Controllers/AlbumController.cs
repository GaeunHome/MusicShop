using Microsoft.AspNetCore.Mvc;
using MusicShop.Services.Interface;

namespace MusicShop.Controllers;

/// <summary>
/// 專輯控制器 - 展示層
/// 負責處理使用者請求與回應
/// </summary>
public class AlbumController : Controller
{
    private readonly IAlbumService _albumService;
    private readonly ICategoryService _categoryService;

    public AlbumController(IAlbumService albumService, ICategoryService categoryService)
    {
        _albumService = albumService;
        _categoryService = categoryService;
    }

    // GET: /Album
    public async Task<IActionResult> Index(string? search, int? categoryId)
    {
        // 從服務層取得專輯資料
        var albums = await _albumService.GetAlbumsAsync(search, categoryId);

        // 從服務層取得分類清單
        var categories = await _categoryService.GetAllCategoriesAsync();

        // 傳遞資料給 View
        ViewBag.Search = search;
        ViewBag.CategoryId = categoryId;
        ViewBag.Categories = categories;

        return View(albums);
    }

    // GET: /Album/Detail/5
    public async Task<IActionResult> Detail(int id)
    {
        // 從服務層取得專輯詳細資料
        var album = await _albumService.GetAlbumDetailAsync(id);

        if (album == null)
            return NotFound();

        return View(album);
    }
}
