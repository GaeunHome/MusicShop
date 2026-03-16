using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MusicShop.Data.Entities;
using MusicShop.Service.Services.Interfaces;
using MusicShop.Service.ViewModels.Album;

namespace MusicShop.Controllers;

/// <summary>
/// 專輯控制器 - 展示層
/// 負責處理使用者請求與回應
/// </summary>
public class AlbumController : Controller
{
    private readonly IAlbumService _albumService;
    private readonly IArtistService _artistService;
    private readonly IArtistCategoryService _artistCategoryService;
    private readonly IProductTypeService _productTypeService;
    private readonly IWishlistService _wishlistService;
    private readonly UserManager<AppUser> _userManager;

    public AlbumController(
        IAlbumService albumService,
        IArtistService artistService,
        IArtistCategoryService artistCategoryService,
        IProductTypeService productTypeService,
        IWishlistService wishlistService,
        UserManager<AppUser> userManager)
    {
        _albumService = albumService;
        _artistService = artistService;
        _artistCategoryService = artistCategoryService;
        _productTypeService = productTypeService;
        _wishlistService = wishlistService;
        _userManager = userManager;
    }

    // GET: /Album
    public async Task<IActionResult> Index(
        string? search,
        int? artistCategoryId,
        int? artistId,
        int? productTypeId,
        int? parentProductTypeId,
        string? sortBy)
    {
        // 從服務層取得專輯 ViewModel（支援多重篩選與排序）
        var albums = await _albumService.GetAlbumCardViewModelsAsync(search, artistCategoryId, artistId, productTypeId, parentProductTypeId, sortBy);

        // 從服務層取得分類清單 ViewModel（不包含 Entity）
        var artistCategories = await _artistCategoryService.GetArtistCategorySelectItemsAsync();
        var childCategories = await _productTypeService.GetChildCategorySelectItemsAsync();

        // 如果有父分類 ID，取得父分類資訊以供顯示
        if (parentProductTypeId.HasValue)
        {
            var parentCategory = await _productTypeService.GetProductTypeByIdAsync(parentProductTypeId.Value);
            ViewBag.ParentCategoryName = parentCategory?.Name;
        }

        // 如果有藝人 ID，取得藝人名稱以供顯示標題
        if (artistId.HasValue)
        {
            var artist = await _artistService.GetArtistByIdAsync(artistId.Value);
            ViewBag.SelectedArtistName = artist?.Name;
        }

        // 傳遞資料給 View
        var userId = _userManager.GetUserId(User);
        ViewBag.WishlistIds = await _wishlistService.GetWishlistAlbumIdsAsync(userId ?? string.Empty);

        ViewBag.Search = search;
        ViewBag.ArtistCategoryId = artistCategoryId;
        ViewBag.ArtistId = artistId;
        ViewBag.ProductTypeId = productTypeId;
        ViewBag.ParentProductTypeId = parentProductTypeId;
        ViewBag.SortBy = sortBy;
        ViewBag.ArtistCategories = artistCategories;
        ViewBag.ChildCategories = childCategories;

        return View(albums);
    }

    // GET: /Album/Detail/5
    public async Task<IActionResult> Detail(int id)
    {
        // 從服務層取得專輯詳情 ViewModel
        var viewModel = await _albumService.GetAlbumDetailViewModelAsync(id);

        if (viewModel == null)
            return NotFound();

        // 傳遞收藏狀態（已登入才需查詢）
        var userId = _userManager.GetUserId(User);
        var wishlistIds = await _wishlistService.GetWishlistAlbumIdsAsync(userId ?? string.Empty);
        ViewBag.IsInWishlist = wishlistIds.Contains(id);

        return View(viewModel);
    }
}
