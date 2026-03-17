// ─────────────────────────────────────────────────────────────
// AdminController.cs - Base partial class
// Responsibility: Constructor, shared dependencies, Dashboard action
// ─────────────────────────────────────────────────────────────

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MusicShop.Service.Services.Interfaces;
using MusicShop.Web.Infrastructure;

namespace MusicShop.Controllers
{
    /// <summary>
    /// 後台管理控制器 - 展示層
    /// 使用三層式架構：Controller → Service → Repository
    /// Controller 只使用 ViewModel，不直接接觸 Data 層實體
    ///
    /// 此控制器使用 partial class 拆分為多個檔案：
    /// - AdminController.cs          : 建構函式、共用依賴、後台首頁
    /// - AdminController.Album.cs    : 商品管理 CRUD
    /// - AdminController.Category.cs : 分類與藝人管理
    /// - AdminController.Order.cs    : 訂單管理
    /// - AdminController.User.cs     : 使用者管理
    /// - AdminController.Banner.cs   : 幻燈片管理
    /// </summary>
    [Authorize(Roles = "Admin")]
    public partial class AdminController : Controller
    {
        private readonly IAlbumService _albumService;
        private readonly IArtistCategoryService _artistCategoryService;
        private readonly IArtistService _artistService;
        private readonly IProductTypeService _productTypeService;
        private readonly IOrderService _orderService;
        private readonly IStatisticsService _statisticsService;
        private readonly IUserService _userService;
        private readonly IAlbumImageService _albumImageService;
        private readonly IBannerService _bannerService;
        private readonly IBannerImageService _bannerImageService;

        public AdminController(
            IAlbumService albumService,
            IArtistCategoryService artistCategoryService,
            IArtistService artistService,
            IProductTypeService productTypeService,
            IOrderService orderService,
            IStatisticsService statisticsService,
            IUserService userService,
            IAlbumImageService albumImageService,
            IBannerService bannerService,
            IBannerImageService bannerImageService)
        {
            _albumService = albumService;
            _artistCategoryService = artistCategoryService;
            _artistService = artistService;
            _productTypeService = productTypeService;
            _orderService = orderService;
            _statisticsService = statisticsService;
            _userService = userService;
            _albumImageService = albumImageService;
            _bannerService = bannerService;
            _bannerImageService = bannerImageService;
        }

        // ─── 後台首頁 ───────────────────────────────────────
        public async Task<IActionResult> Index()
        {
            ViewBag.AlbumCount = await _statisticsService.GetAlbumCountAsync();
            ViewBag.ArtistCount = await _statisticsService.GetArtistCountAsync();
            ViewBag.CategoryCount = await _statisticsService.GetCategoryCountAsync();
            ViewBag.OrderCount = await _statisticsService.GetOrderCountAsync();
            ViewBag.UserCount = await _statisticsService.GetUserCountAsync();
            ViewBag.TotalSales = await _statisticsService.GetTotalSalesAsync();
            ViewBag.PendingOrderCount = await _statisticsService.GetPendingOrderCountAsync();
            return View();
        }
    }
}
