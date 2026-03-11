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
        // 從服務層取得專輯資料（支援多重篩選與排序）
        var albums = await _albumService.GetAlbumsAsync(search, artistCategoryId, artistId, productTypeId, parentProductTypeId, sortBy);

        // 從服務層取得分類清單
        var artistCategories = await _artistCategoryService.GetAllArtistCategoriesAsync();
        var artistsGrouped = await _artistService.GetArtistsGroupedByCategoryAsync();
        var childCategories = await _productTypeService.GetAllChildCategoriesAsync();

        // 如果有父分類 ID，取得父分類資訊以供顯示
        if (parentProductTypeId.HasValue)
        {
            var parentCategory = await _productTypeService.GetProductTypeByIdAsync(parentProductTypeId.Value);
            ViewBag.ParentCategoryName = parentCategory?.Name;
        }

        // 如果有藝人 ID，取得藝人資訊以供顯示標題
        if (artistId.HasValue)
        {
            var artist = await _artistService.GetArtistByIdAsync(artistId.Value);
            ViewBag.SelectedArtist = artist;
        }

        // 傳遞資料給 View
        // 已登入使用者：取得收藏清單 ID，供 View 顯示愛心狀態
        var userId = _userManager.GetUserId(User);
        ViewBag.WishlistIds = await _wishlistService.GetWishlistAlbumIdsAsync(userId ?? string.Empty);

        ViewBag.Search = search;
        ViewBag.ArtistCategoryId = artistCategoryId;
        ViewBag.ArtistId = artistId;
        ViewBag.ProductTypeId = productTypeId;
        ViewBag.ParentProductTypeId = parentProductTypeId;
        ViewBag.SortBy = sortBy;
        ViewBag.ArtistCategories = artistCategories;
        ViewBag.ArtistsGrouped = artistsGrouped;
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

        // 傳遞收藏狀態（已登入才需查詢）
        var userId = _userManager.GetUserId(User);
        var wishlistIds = await _wishlistService.GetWishlistAlbumIdsAsync(userId ?? string.Empty);
        ViewBag.IsInWishlist = wishlistIds.Contains(id);

        // 取得相關商品（同藝人分類或同商品類型，排除當前商品，最多 8 個）
        var relatedAlbums = await _albumService.GetAlbumsAsync(
            searchTerm: null,
            artistCategoryId: album.Artist?.ArtistCategoryId,
            artistId: album.ArtistId,
            productTypeId: album.ProductTypeId,
            parentProductTypeId: null);

        // 建立 ViewModel
        var viewModel = new AlbumDetailViewModel
        {
            Album = album,
            ImageUrls = !string.IsNullOrEmpty(album.CoverImageUrl)
                ? album.CoverImageUrl.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList()
                : new List<string>(),
            RelatedAlbums = relatedAlbums
                .Where(a => a.Id != id)
                .Take(8)
                .Select(a => new AlbumCardViewModel { Album = a })
                .ToList()
        };

        return View(viewModel);
    }
}
