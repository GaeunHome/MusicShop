using Microsoft.AspNetCore.Mvc;
using MusicShop.Service.Services.Interfaces;
using System.Security.Claims;

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

    /// <summary>
    /// 每頁顯示的商品數量
    /// </summary>
    private const int PageSize = 12;

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
        // 確保頁碼至少為 1
        if (page < 1) page = 1;

        // 從服務層取得分頁專輯 ViewModel（支援多重篩選與排序）
        var pagedResult = await _albumService.GetAlbumCardViewModelsPagedAsync(
            page, PageSize, search, artistCategoryId, artistId, productTypeId, parentProductTypeId, sortBy);

        // 從服務層取得分類清單 ViewModel（不包含 Entity）
        var artistCategories = await _artistCategoryService.GetArtistCategorySelectItemsAsync();
        var childCategories = await _productTypeService.GetChildCategorySelectItemsAsync();

        // 使用 ViewModel 方法取得名稱，避免直接接觸 Entity
        if (parentProductTypeId.HasValue)
        {
            ViewBag.ParentCategoryName = await _productTypeService.GetProductTypeNameByIdAsync(parentProductTypeId.Value);
        }

        // 使用 ViewModel 方法取得藝人名稱，避免直接接觸 Entity
        if (artistId.HasValue)
        {
            ViewBag.SelectedArtistName = await _artistService.GetArtistNameByIdAsync(artistId.Value);
        }

        // 傳遞資料給 View
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        ViewBag.WishlistIds = await _wishlistService.GetWishlistAlbumIdsAsync(userId ?? string.Empty);

        ViewBag.Search = search;
        ViewBag.ArtistCategoryId = artistCategoryId;
        ViewBag.ArtistId = artistId;
        ViewBag.ProductTypeId = productTypeId;
        ViewBag.ParentProductTypeId = parentProductTypeId;
        ViewBag.SortBy = sortBy;
        ViewBag.ArtistCategories = artistCategories;
        ViewBag.ChildCategories = childCategories;

        // 分頁資訊
        ViewBag.CurrentPage = pagedResult.CurrentPage;
        ViewBag.TotalPages = pagedResult.TotalPages;
        ViewBag.HasPreviousPage = pagedResult.HasPreviousPage;
        ViewBag.HasNextPage = pagedResult.HasNextPage;
        ViewBag.TotalCount = pagedResult.TotalCount;
        ViewBag.PageSize = PageSize;

        return View(pagedResult.Items);
    }

    // GET: /Album/Detail/5
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
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var wishlistIds = await _wishlistService.GetWishlistAlbumIdsAsync(userId ?? string.Empty);
        ViewBag.IsInWishlist = wishlistIds.Contains(id);

        return View(viewModel);
    }
}
