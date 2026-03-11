using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using MusicShop.Data.Entities;
using MusicShop.Service.Services.Interfaces;
using MusicShop.Service.ViewModels.Admin;
using MusicShop.Web.Services.Interfaces;

namespace MusicShop.Controllers
{
    /// <summary>
    /// 後台管理控制器 - 展示層
    /// 使用三層式架構：Controller → Service → Repository
    /// </summary>
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly IAlbumService _albumService;
        private readonly IArtistCategoryService _artistCategoryService;
        private readonly IArtistService _artistService;
        private readonly IProductTypeService _productTypeService;
        private readonly IOrderService _orderService;
        private readonly IStatisticsService _statisticsService;
        private readonly IUserService _userService;
        private readonly UserManager<AppUser> _userManager;
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
            UserManager<AppUser> userManager,
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
            _userManager = userManager;
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

        // ─── 商品管理 ───────────────────────────────────────
        public async Task<IActionResult> Albums()
        {
            var albums = await _albumService.GetAlbumsAsync();
            return View("Album/Index", albums);
        }

        public async Task<IActionResult> AlbumCreate()
        {
            await PopulateAlbumViewBags();
            return View("Album/Create");
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> AlbumCreate(Album album, IFormFile? coverImageFile, IFormFile? descriptionImageFile)
        {
            ModelState.Remove("Artist");
            ModelState.Remove("ProductType");
            ModelState.Remove("CoverImageUrl");
            ModelState.Remove("DescriptionImageUrl");

            if (!ModelState.IsValid)
            {
                await PopulateAlbumViewBags();
                return View("Album/Create", album);
            }

            try
            {
                // 先建立商品以取得 DB 產生的 ID
                await _albumService.CreateAlbumAsync(album);

                // 再以 ID 組出目錄並儲存圖片
                if (coverImageFile?.Length > 0 || descriptionImageFile?.Length > 0)
                {
                    var subFolder = await _albumImageService.BuildSubFolderAsync(album.ProductTypeId, album.ArtistId, album.Id);
                    album.CoverImageUrl = await _albumImageService.SaveImageAsync(coverImageFile, subFolder, "cover");
                    album.DescriptionImageUrl = await _albumImageService.SaveImageAsync(descriptionImageFile, subFolder, "description");
                    await _albumService.UpdateAlbumAsync(album);
                }

                TempData["Success"] = "商品新增成功！";
                return RedirectToAction("Albums");
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                await PopulateAlbumViewBags();
                return View("Album/Create", album);
            }
        }

        public async Task<IActionResult> AlbumEdit(int id)
        {
            var album = await _albumService.GetAlbumDetailAsync(id);
            if (album == null) return NotFound();

            int? selectedParentId = album.ProductType?.ParentId;

            await PopulateAlbumViewBags(album.ArtistId);
            ViewBag.ParentCategories = new SelectList(
                await _productTypeService.GetParentCategoriesAsync(), "Id", "Name", selectedParentId);
            ViewBag.SelectedProductTypeId = album.ProductTypeId;
            return View("Album/Edit", album);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> AlbumEdit(Album album, IFormFile? coverImageFile, IFormFile? descriptionImageFile)
        {
            ModelState.Remove("Artist");
            ModelState.Remove("ProductType");
            ModelState.Remove("CoverImageUrl");
            ModelState.Remove("DescriptionImageUrl");

            if (!ModelState.IsValid)
            {
                await PopulateAlbumViewBags(album.ArtistId);
                return View("Album/Edit", album);
            }

            try
            {
                var subFolder = await _albumImageService.BuildSubFolderAsync(album.ProductTypeId, album.ArtistId, album.Id);
                album.CoverImageUrl = await _albumImageService.SaveImageAsync(coverImageFile, subFolder, "cover", album.CoverImageUrl);
                album.DescriptionImageUrl = await _albumImageService.SaveImageAsync(descriptionImageFile, subFolder, "description", album.DescriptionImageUrl);
                await _albumService.UpdateAlbumAsync(album);
                TempData["Success"] = "商品更新成功！";
                return RedirectToAction("Albums");
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                await PopulateAlbumViewBags(album.ArtistId);
                return View("Album/Edit", album);
            }
        }

        private async Task PopulateAlbumViewBags(int? selectedArtistId = null)
        {
            var artistCategories = await _artistCategoryService.GetAllArtistCategoriesAsync();
            var artists = await _artistService.GetAllArtistsAsync();
            var parentCategories = await _productTypeService.GetParentCategoriesAsync();
            var childCategories = await _productTypeService.GetAllChildCategoriesAsync();

            ViewBag.ArtistCategories = new SelectList(artistCategories, "Id", "Name");
            ViewBag.Artists = new SelectList(artists, "Id", "Name", selectedArtistId);
            ViewBag.ParentCategories = new SelectList(parentCategories, "Id", "Name");
            ViewBag.ChildCategories = childCategories;
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> AlbumDelete(int id)
        {
            try
            {
                await _albumService.DeleteAlbumAsync(id);
                TempData["Success"] = "商品刪除成功！";
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
            }

            return RedirectToAction("Albums");
        }

        // ─── 分類管理 ───────────────────────────────────────
        public async Task<IActionResult> Categories()
        {
            var artistCategories = await _artistCategoryService.GetAllArtistCategoriesAsync();
            var parentCategories = await _productTypeService.GetParentCategoriesAsync();

            ViewBag.ArtistCategories = artistCategories;
            ViewBag.ParentCategories = parentCategories;

            // 為每個父分類載入子分類
            var categoryTree = new Dictionary<ProductType, IEnumerable<ProductType>>();
            foreach (var parent in parentCategories)
            {
                var children = await _productTypeService.GetChildrenByParentIdAsync(parent.Id);
                categoryTree[parent] = children;
            }
            ViewBag.CategoryTree = categoryTree;

            return View("Category/Index");
        }

        // 藝人分類 CRUD
        public IActionResult ArtistCategoryCreate() => View("Category/ArtistCategory/Create");

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> ArtistCategoryCreate(ArtistCategory artistCategory)
        {
            if (!ModelState.IsValid)
                return View("Category/ArtistCategory/Create", artistCategory);

            try
            {
                await _artistCategoryService.CreateArtistCategoryAsync(artistCategory);
                TempData["Success"] = "藝人分類新增成功！";
                return RedirectToAction("Categories");
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                return View("Category/ArtistCategory/Create", artistCategory);
            }
        }

        public async Task<IActionResult> ArtistCategoryEdit(int id)
        {
            var artistCategory = await _artistCategoryService.GetArtistCategoryByIdAsync(id);
            if (artistCategory == null) return NotFound();
            return View("Category/ArtistCategory/Edit", artistCategory);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> ArtistCategoryEdit(ArtistCategory artistCategory)
        {
            if (!ModelState.IsValid)
                return View("Category/ArtistCategory/Edit", artistCategory);

            try
            {
                await _artistCategoryService.UpdateArtistCategoryAsync(artistCategory);
                TempData["Success"] = "藝人分類更新成功！";
                return RedirectToAction("Categories");
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                return View("Category/ArtistCategory/Edit", artistCategory);
            }
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> ArtistCategoryDelete(int id)
        {
            try
            {
                await _artistCategoryService.DeleteArtistCategoryAsync(id);
                TempData["Success"] = "藝人分類刪除成功！";
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
            }

            return RedirectToAction("Categories");
        }

        // 商品類型 CRUD
        public async Task<IActionResult> ProductTypeCreate()
        {
            var parentCategories = await _productTypeService.GetParentCategoriesAsync();
            ViewBag.ParentCategories = new SelectList(parentCategories, "Id", "Name");
            return View("Category/ProductType/Create");
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> ProductTypeCreate(ProductType productType)
        {
            if (!ModelState.IsValid)
                return View("Category/ProductType/Create", productType);

            try
            {
                await _productTypeService.CreateProductTypeAsync(productType);
                TempData["Success"] = "商品類型新增成功！";
                return RedirectToAction("Categories");
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                return View("Category/ProductType/Create", productType);
            }
        }

        public async Task<IActionResult> ProductTypeEdit(int id)
        {
            var productType = await _productTypeService.GetProductTypeByIdAsync(id);
            if (productType == null) return NotFound();

            var parentCategories = await _productTypeService.GetParentCategoriesAsync();
            ViewBag.ParentCategories = new SelectList(parentCategories, "Id", "Name");

            return View("Category/ProductType/Edit", productType);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> ProductTypeEdit(ProductType productType)
        {
            if (!ModelState.IsValid)
                return View("Category/ProductType/Edit", productType);

            try
            {
                await _productTypeService.UpdateProductTypeAsync(productType);
                TempData["Success"] = "商品類型更新成功！";
                return RedirectToAction("Categories");
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                return View("Category/ProductType/Edit", productType);
            }
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> ProductTypeDelete(int id)
        {
            try
            {
                await _productTypeService.DeleteProductTypeAsync(id);
                TempData["Success"] = "商品類型刪除成功！";
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
            }

            return RedirectToAction("Categories");
        }

        // API: 根據父分類 ID 取得子分類（用於級聯下拉選單）
        [HttpGet]
        public async Task<IActionResult> GetChildCategories(int parentId)
        {
            var children = await _productTypeService.GetChildrenByParentIdAsync(parentId);
            var result = children.Select(c => new { id = c.Id, name = c.Name });
            return Json(result);
        }

        // ─── 藝人管理 ───────────────────────────────────────
        public async Task<IActionResult> Artists()
        {
            var artists = await _artistService.GetAllArtistsAsync();
            return View("Artist/Index", artists);
        }

        public async Task<IActionResult> ArtistCreate()
        {
            var artistCategories = await _artistCategoryService.GetAllArtistCategoriesAsync();
            ViewBag.ArtistCategories = new SelectList(artistCategories, "Id", "Name");
            return View("Artist/Create");
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> ArtistCreate(Artist artist)
        {
            // 移除導航屬性的驗證錯誤（因為表單只提交 ID，不提交整個物件）
            ModelState.Remove("ArtistCategory");
            ModelState.Remove("Albums");

            if (!ModelState.IsValid)
            {
                var artistCategories = await _artistCategoryService.GetAllArtistCategoriesAsync();
                ViewBag.ArtistCategories = new SelectList(artistCategories, "Id", "Name");
                return View("Artist/Create", artist);
            }

            try
            {
                await _artistService.CreateArtistAsync(artist);
                TempData["Success"] = "藝人新增成功！";
                return RedirectToAction("Artists");
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                var artistCategories = await _artistCategoryService.GetAllArtistCategoriesAsync();
                ViewBag.ArtistCategories = new SelectList(artistCategories, "Id", "Name");
                return View("Artist/Create", artist);
            }
        }

        public async Task<IActionResult> ArtistEdit(int id)
        {
            var artist = await _artistService.GetArtistByIdAsync(id);
            if (artist == null) return NotFound();

            var artistCategories = await _artistCategoryService.GetAllArtistCategoriesAsync();
            ViewBag.ArtistCategories = new SelectList(artistCategories, "Id", "Name", artist.ArtistCategoryId);
            return View("Artist/Edit", artist);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> ArtistEdit(Artist artist)
        {
            // 移除導航屬性的驗證錯誤（因為表單只提交 ID，不提交整個物件）
            ModelState.Remove("ArtistCategory");
            ModelState.Remove("Albums");

            if (!ModelState.IsValid)
            {
                var artistCategories = await _artistCategoryService.GetAllArtistCategoriesAsync();
                ViewBag.ArtistCategories = new SelectList(artistCategories, "Id", "Name", artist.ArtistCategoryId);
                return View("Artist/Edit", artist);
            }

            try
            {
                await _artistService.UpdateArtistAsync(artist);
                TempData["Success"] = "藝人更新成功！";
                return RedirectToAction("Artists");
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                var artistCategories = await _artistCategoryService.GetAllArtistCategoriesAsync();
                ViewBag.ArtistCategories = new SelectList(artistCategories, "Id", "Name", artist.ArtistCategoryId);
                return View("Artist/Edit", artist);
            }
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> ArtistDelete(int id)
        {
            try
            {
                await _artistService.DeleteArtistAsync(id);
                TempData["Success"] = "藝人刪除成功！";
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
            }

            return RedirectToAction("Artists");
        }

        // API: 根據藝人分類 ID 取得藝人列表（用於級聯下拉選單）
        [HttpGet]
        public async Task<IActionResult> GetArtistsByCategory(int categoryId)
        {
            var artists = await _artistService.GetArtistsByCategoryIdAsync(categoryId);
            var result = artists.Select(a => new { id = a.Id, name = a.Name });
            return Json(result);
        }

        // ─── 訂單管理 ───────────────────────────────────────
        public async Task<IActionResult> Orders()
        {
            var orders = await _orderService.GetAllOrdersAsync();
            return View("Order/Index", orders);
        }

        public async Task<IActionResult> OrderDetail(int id)
        {
            try
            {
                var order = await _orderService.GetOrderByIdAsync(id);

                if (order == null) return NotFound();
                return View("Order/Detail", order);
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction("Orders");
            }
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateOrderStatus(int orderId, OrderStatus status)
        {
            try
            {
                await _orderService.UpdateOrderStatusAsync(orderId, status);
                TempData["Success"] = "訂單狀態更新成功！";
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
            }

            return RedirectToAction("OrderDetail", new { id = orderId });
        }

        // ─── 使用者管理 ───────────────────────────────────────

        /// <summary>
        /// 使用者管理頁面 - 顯示所有使用者和其角色
        /// </summary>
        public async Task<IActionResult> Users()
        {
            // 透過 Service 層取得所有使用者及其角色資訊
            var userViewModels = await _userService.GetAllUsersWithRolesAsync();

            return View("User/Index", userViewModels);
        }

        /// <summary>
        /// 切換使用者的管理員角色（指派或移除）
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleAdminRole(string userId)
        {
            // 取得當前管理員的 ID
            var currentAdminId = _userManager.GetUserId(User);

            if (string.IsNullOrEmpty(currentAdminId))
            {
                TempData["Error"] = "無法取得當前使用者資訊";
                return RedirectToAction(nameof(Users));
            }

            // 透過 Service 層執行角色切換
            var (success, message) = await _userService.ToggleAdminRoleAsync(userId, currentAdminId);

            if (success)
            {
                TempData["Success"] = message;
            }
            else
            {
                TempData["Error"] = message;
            }

            return RedirectToAction(nameof(Users));
        }

        // ─── 幻燈片管理 ─────────────────────────────────────
        public async Task<IActionResult> Banners()
        {
            var banners = await _bannerService.GetAllBannersAsync();
            return View("Banner/Index", banners);
        }

        public async Task<IActionResult> BannerCreate()
        {
            await PopulateBannerViewBags();
            return View("Banner/Create");
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> BannerCreate(Banner banner, IFormFile? bannerImageFile)
        {
            ModelState.Remove("Album");
            ModelState.Remove("ImageUrl");

            if (!ModelState.IsValid)
            {
                await PopulateBannerViewBags();
                return View("Banner/Create", banner);
            }

            try
            {
                // 先建立 Banner 取得 Id，再儲存圖片
                banner.ImageUrl = string.Empty;
                var created = await _bannerService.CreateBannerAsync(banner);

                var imageUrl = await _bannerImageService.SaveBannerImageAsync(bannerImageFile, created.Id);
                if (!string.IsNullOrEmpty(imageUrl))
                {
                    created.ImageUrl = imageUrl;
                    await _bannerService.UpdateBannerAsync(created);
                }

                TempData["Success"] = "幻燈片新增成功！";
                return RedirectToAction(nameof(Banners));
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                await PopulateBannerViewBags();
                return View("Banner/Create", banner);
            }
        }

        public async Task<IActionResult> BannerEdit(int id)
        {
            var banner = await _bannerService.GetBannerByIdAsync(id);
            if (banner == null) return NotFound();

            await PopulateBannerViewBags();
            return View("Banner/Edit", banner);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> BannerEdit(Banner banner, IFormFile? bannerImageFile)
        {
            ModelState.Remove("Album");
            ModelState.Remove("ImageUrl");

            if (!ModelState.IsValid)
            {
                await PopulateBannerViewBags();
                return View("Banner/Edit", banner);
            }

            try
            {
                var existing = await _bannerService.GetBannerByIdAsync(banner.Id);
                if (existing == null) return NotFound();

                existing.AlbumId = banner.AlbumId;
                existing.DisplayOrder = banner.DisplayOrder;
                existing.IsActive = banner.IsActive;

                var imageUrl = await _bannerImageService.SaveBannerImageAsync(bannerImageFile, existing.Id, existing.ImageUrl);
                existing.ImageUrl = imageUrl ?? existing.ImageUrl;

                await _bannerService.UpdateBannerAsync(existing);

                TempData["Success"] = "幻燈片更新成功！";
                return RedirectToAction(nameof(Banners));
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                await PopulateBannerViewBags();
                return View("Banner/Edit", banner);
            }
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> BannerDelete(int id)
        {
            var banner = await _bannerService.GetBannerByIdAsync(id);
            if (banner != null)
            {
                _bannerImageService.DeleteBannerImage(banner.ImageUrl);
                await _bannerService.DeleteBannerAsync(id);
            }
            TempData["Success"] = "幻燈片已刪除。";
            return RedirectToAction(nameof(Banners));
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> BannerToggle(int id)
        {
            var banner = await _bannerService.GetBannerByIdAsync(id);
            if (banner == null) return NotFound();

            banner.IsActive = !banner.IsActive;
            await _bannerService.UpdateBannerAsync(banner);

            TempData["Success"] = banner.IsActive ? "幻燈片已啟用。" : "幻燈片已停用。";
            return RedirectToAction(nameof(Banners));
        }

        private async Task PopulateBannerViewBags()
        {
            var albums = await _albumService.GetAlbumsAsync();
            ViewBag.Albums = new SelectList(albums, "Id", "Title");
        }
    }
}
