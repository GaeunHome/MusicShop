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
    private readonly IArtistCategoryService _artistCategoryService;
    private readonly IProductTypeService _productTypeService;

    public AlbumController(
        IAlbumService albumService,
        IArtistCategoryService artistCategoryService,
        IProductTypeService productTypeService)
    {
        _albumService = albumService;
        _artistCategoryService = artistCategoryService;
        _productTypeService = productTypeService;
    }

    // GET: /Album
    public async Task<IActionResult> Index(
        string? search,
        int? artistCategoryId,
        int? productTypeId,
        int? parentProductTypeId)
    {
        // 從服務層取得專輯資料（支援雙分類篩選）
        var albums = await _albumService.GetAlbumsAsync(search, artistCategoryId, productTypeId, parentProductTypeId);

        // 從服務層取得分類清單
        var artistCategories = await _artistCategoryService.GetAllArtistCategoriesAsync();
        var childCategories = await _productTypeService.GetAllChildCategoriesAsync();

        // 如果有父分類 ID，取得父分類資訊以供顯示
        if (parentProductTypeId.HasValue)
        {
            var parentCategory = await _productTypeService.GetProductTypeByIdAsync(parentProductTypeId.Value);
            ViewBag.ParentCategoryName = parentCategory?.Name;
        }

        // 傳遞資料給 View
        ViewBag.Search = search;
        ViewBag.ArtistCategoryId = artistCategoryId;
        ViewBag.ProductTypeId = productTypeId;
        ViewBag.ParentProductTypeId = parentProductTypeId;
        ViewBag.ArtistCategories = artistCategories;
        ViewBag.ChildCategories = childCategories;

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
