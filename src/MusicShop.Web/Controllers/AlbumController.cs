using Microsoft.AspNetCore.Mvc;
using MusicShop.Library.Helpers;
using MusicShop.Service.Services.Interfaces;
using MusicShop.Service.ViewModels.Album;

namespace MusicShop.Controllers;

/// <summary>
/// 專輯控制器 - 展示層
/// 負責處理使用者請求與回應
/// </summary>
public class AlbumController : BaseController
{
    private readonly IAlbumService _albumService;
    private readonly IArtistService _artistService;
    private readonly IArtistCategoryService _artistCategoryService;
    private readonly IProductTypeService _productTypeService;
    private readonly IWishlistService _wishlistService;

    public AlbumController(
        IAlbumService albumService,
        IArtistService artistService,
        IArtistCategoryService artistCategoryService,
        IProductTypeService productTypeService,
        IWishlistService wishlistService)
    {
        _albumService = albumService;
        _artistService = artistService;
        _artistCategoryService = artistCategoryService;
        _productTypeService = productTypeService;
        _wishlistService = wishlistService;
    }

    private const int PageSize = DisplayConstants.AlbumPageSize;

    // GET: /Album
    public async Task<IActionResult> Index(
        string? search,
        int? artistCategoryId,
        int? artistId,
        int? productTypeId,
        int? parentProductTypeId,
        string? sortBy,
        int page = 1)
    {
        if (page < 1) page = 1;

        var pagedResult = await _albumService.GetAlbumCardViewModelsPagedAsync(
            page, PageSize, search, artistCategoryId, artistId, productTypeId, parentProductTypeId, sortBy);

        var userId = GetCurrentUserId();

        var viewModel = new AlbumIndexViewModel
        {
            PagedResult = pagedResult,
            Search = search,
            ArtistCategoryId = artistCategoryId,
            ArtistId = artistId,
            ProductTypeId = productTypeId,
            ParentProductTypeId = parentProductTypeId,
            SortBy = sortBy,
            ArtistCategories = await _artistCategoryService.GetArtistCategorySelectItemsAsync(),
            ChildCategories = await _productTypeService.GetChildCategorySelectItemsAsync(),
            WishlistIds = await _wishlistService.GetWishlistAlbumIdsAsync(userId ?? string.Empty),
            ParentCategoryName = parentProductTypeId.HasValue
                ? await _productTypeService.GetProductTypeNameByIdAsync(parentProductTypeId.Value)
                : null,
            SelectedArtistName = artistId.HasValue
                ? await _artistService.GetArtistNameByIdAsync(artistId.Value)
                : null
        };

        // 供共用 Partial View 使用的 ViewBag（_AlbumCard 讀取 WishlistIds、_Pagination 讀取分頁資訊）
        ViewBag.WishlistIds = viewModel.WishlistIds;
        SetPaginationViewBag(pagedResult);

        return View(viewModel);
    }

    /// <summary>
    /// 設定分頁 ViewBag（供共用 _Pagination Partial View 使用）
    /// </summary>
    private void SetPaginationViewBag<T>(PagedResult<T> pagedResult)
    {
        ViewBag.CurrentPage = pagedResult.CurrentPage;
        ViewBag.TotalPages = pagedResult.TotalPages;
        ViewBag.HasPreviousPage = pagedResult.HasPreviousPage;
        ViewBag.HasNextPage = pagedResult.HasNextPage;
    }

    // GET: /Album/Detail/5
    [ResponseCache(Duration = 60, VaryByQueryKeys = ["id"])]
    public async Task<IActionResult> Detail(int id)
    {
        // 從服務層取得專輯詳情 ViewModel
        var viewModel = await _albumService.GetAlbumDetailViewModelAsync(id);

        if (viewModel == null)
            return NotFound();

        // 設定 SEO Meta 標籤與 Open Graph 資訊
        ViewBag.MetaDescription = $"{viewModel.Title} - {viewModel.ArtistName} | MusicShop";
        ViewBag.OgTitle = viewModel.Title;
        ViewBag.OgType = "product";
        ViewBag.OgImage = viewModel.FirstImageUrl;

        // 傳遞收藏狀態（已登入才需查詢）
        var userId = GetCurrentUserId();
        var wishlistIds = await _wishlistService.GetWishlistAlbumIdsAsync(userId ?? string.Empty);
        ViewBag.IsInWishlist = wishlistIds.Contains(id);

        return View(viewModel);
    }
}
