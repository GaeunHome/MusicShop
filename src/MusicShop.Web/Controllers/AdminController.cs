using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using MusicShop.Data.Entities;
using MusicShop.Service.Services.Interfaces;
using MusicShop.Service.ViewModels.Admin;
using MusicShop.Web.Infrastructure.Interfaces;

namespace MusicShop.Controllers
{
    /// <summary>
    /// 後台管理控制器 - 展示層
    /// 使用三層式架構：Controller → Service → Repository
    /// Controller 只使用 ViewModel，不直接接觸 Data 層實體
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
            var albums = await _albumService.GetAlbumListItemsAsync();
            return View("Album/Index", albums);
        }

        public async Task<IActionResult> AlbumCreate()
        {
            await PopulateAlbumViewBags();
            return View("Album/Create", new AlbumFormViewModel());
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> AlbumCreate(AlbumFormViewModel vm, IFormFile? coverImageFile, IFormFile? descriptionImageFile)
        {
            if (!ModelState.IsValid)
            {
                await PopulateAlbumViewBags();
                return View("Album/Create", vm);
            }

            try
            {
                // 先建立商品以取得 DB 產生的 ID
                await _albumService.CreateAlbumAsync(vm);

                // 再以 ID 組出目錄並儲存圖片
                if (coverImageFile?.Length > 0 || descriptionImageFile?.Length > 0)
                {
                    var subFolder = await _albumImageService.BuildSubFolderAsync(vm.ProductTypeId, vm.ArtistId, vm.Id);
                    vm.CoverImageUrl = await _albumImageService.SaveImageAsync(coverImageFile, subFolder, "cover");
                    vm.DescriptionImageUrl = await _albumImageService.SaveImageAsync(descriptionImageFile, subFolder, "description");
                    await _albumService.UpdateAlbumAsync(vm);
                }

                TempData["Success"] = "商品新增成功！";
                return RedirectToAction("Albums");
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                await PopulateAlbumViewBags();
                return View("Album/Create", vm);
            }
        }

        public async Task<IActionResult> AlbumEdit(int id)
        {
            var vm = await _albumService.GetAlbumFormByIdAsync(id);
            if (vm == null) return NotFound();

            // 取得商品類型父分類 ID（用於 JS 初始化級聯下拉選單）
            int? selectedParentId = null;
            if (vm.ProductTypeId.HasValue)
            {
                var productType = await _productTypeService.GetProductTypeWithChildrenAsync(vm.ProductTypeId.Value);
                selectedParentId = productType?.ParentId;
            }

            await PopulateAlbumViewBags(vm.ArtistId);
            ViewBag.ParentCategories = new SelectList(
                await _productTypeService.GetParentCategoriesAsync(), "Id", "Name", selectedParentId);
            ViewBag.SelectedProductTypeId = vm.ProductTypeId;

            // 取得藝人的分類 ID，供 JS 初始化使用
            if (vm.ArtistId.HasValue)
            {
                var artist = await _artistService.GetArtistByIdAsync(vm.ArtistId.Value);
                ViewBag.SelectedArtistCategoryId = artist?.ArtistCategoryId;
            }

            return View("Album/Edit", vm);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> AlbumEdit(AlbumFormViewModel vm, IFormFile? coverImageFile, IFormFile? descriptionImageFile)
        {
            if (!ModelState.IsValid)
            {
                await PopulateAlbumViewBags(vm.ArtistId);
                return View("Album/Edit", vm);
            }

            try
            {
                var subFolder = await _albumImageService.BuildSubFolderAsync(vm.ProductTypeId, vm.ArtistId, vm.Id);
                vm.CoverImageUrl = await _albumImageService.SaveImageAsync(coverImageFile, subFolder, "cover", vm.CoverImageUrl);
                vm.DescriptionImageUrl = await _albumImageService.SaveImageAsync(descriptionImageFile, subFolder, "description", vm.DescriptionImageUrl);
                await _albumService.UpdateAlbumAsync(vm);
                TempData["Success"] = "商品更新成功！";
                return RedirectToAction("Albums");
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                await PopulateAlbumViewBags(vm.ArtistId);
                return View("Album/Edit", vm);
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
        public IActionResult ArtistCategoryCreate() => View("Category/ArtistCategory/Create", new ArtistCategoryFormViewModel());

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> ArtistCategoryCreate(ArtistCategoryFormViewModel vm)
        {
            if (!ModelState.IsValid)
                return View("Category/ArtistCategory/Create", vm);

            try
            {
                await _artistCategoryService.CreateArtistCategoryAsync(vm);
                TempData["Success"] = "藝人分類新增成功！";
                return RedirectToAction("Categories");
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                return View("Category/ArtistCategory/Create", vm);
            }
        }

        public async Task<IActionResult> ArtistCategoryEdit(int id)
        {
            var vm = await _artistCategoryService.GetArtistCategoryFormByIdAsync(id);
            if (vm == null) return NotFound();
            return View("Category/ArtistCategory/Edit", vm);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> ArtistCategoryEdit(ArtistCategoryFormViewModel vm)
        {
            if (!ModelState.IsValid)
                return View("Category/ArtistCategory/Edit", vm);

            try
            {
                await _artistCategoryService.UpdateArtistCategoryAsync(vm);
                TempData["Success"] = "藝人分類更新成功！";
                return RedirectToAction("Categories");
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                return View("Category/ArtistCategory/Edit", vm);
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
            return View("Category/ProductType/Create", new ProductTypeFormViewModel());
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> ProductTypeCreate(ProductTypeFormViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                var parentCategories = await _productTypeService.GetParentCategoriesAsync();
                ViewBag.ParentCategories = new SelectList(parentCategories, "Id", "Name");
                return View("Category/ProductType/Create", vm);
            }

            try
            {
                await _productTypeService.CreateProductTypeAsync(vm);
                TempData["Success"] = "商品類型新增成功！";
                return RedirectToAction("Categories");
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                var parentCategories = await _productTypeService.GetParentCategoriesAsync();
                ViewBag.ParentCategories = new SelectList(parentCategories, "Id", "Name");
                return View("Category/ProductType/Create", vm);
            }
        }

        public async Task<IActionResult> ProductTypeEdit(int id)
        {
            var vm = await _productTypeService.GetProductTypeFormByIdAsync(id);
            if (vm == null) return NotFound();

            var parentCategories = await _productTypeService.GetParentCategoriesAsync();
            ViewBag.ParentCategories = new SelectList(parentCategories, "Id", "Name");

            return View("Category/ProductType/Edit", vm);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> ProductTypeEdit(ProductTypeFormViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                var parentCategories = await _productTypeService.GetParentCategoriesAsync();
                ViewBag.ParentCategories = new SelectList(parentCategories, "Id", "Name");
                return View("Category/ProductType/Edit", vm);
            }

            try
            {
                await _productTypeService.UpdateProductTypeAsync(vm);
                TempData["Success"] = "商品類型更新成功！";
                return RedirectToAction("Categories");
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                var parentCategories = await _productTypeService.GetParentCategoriesAsync();
                ViewBag.ParentCategories = new SelectList(parentCategories, "Id", "Name");
                return View("Category/ProductType/Edit", vm);
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
            var artists = await _artistService.GetArtistListItemsAsync();
            return View("Artist/Index", artists);
        }

        public async Task<IActionResult> ArtistCreate()
        {
            var artistCategories = await _artistCategoryService.GetAllArtistCategoriesAsync();
            ViewBag.ArtistCategories = new SelectList(artistCategories, "Id", "Name");
            return View("Artist/Create", new ArtistFormViewModel());
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> ArtistCreate(ArtistFormViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                var artistCategories = await _artistCategoryService.GetAllArtistCategoriesAsync();
                ViewBag.ArtistCategories = new SelectList(artistCategories, "Id", "Name");
                return View("Artist/Create", vm);
            }

            try
            {
                await _artistService.CreateArtistAsync(vm);
                TempData["Success"] = "藝人新增成功！";
                return RedirectToAction("Artists");
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                var artistCategories = await _artistCategoryService.GetAllArtistCategoriesAsync();
                ViewBag.ArtistCategories = new SelectList(artistCategories, "Id", "Name");
                return View("Artist/Create", vm);
            }
        }

        public async Task<IActionResult> ArtistEdit(int id)
        {
            var vm = await _artistService.GetArtistFormByIdAsync(id);
            if (vm == null) return NotFound();

            var artistCategories = await _artistCategoryService.GetAllArtistCategoriesAsync();
            ViewBag.ArtistCategories = new SelectList(artistCategories, "Id", "Name", vm.ArtistCategoryId);
            return View("Artist/Edit", vm);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> ArtistEdit(ArtistFormViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                var artistCategories = await _artistCategoryService.GetAllArtistCategoriesAsync();
                ViewBag.ArtistCategories = new SelectList(artistCategories, "Id", "Name", vm.ArtistCategoryId);
                return View("Artist/Edit", vm);
            }

            try
            {
                await _artistService.UpdateArtistAsync(vm);
                TempData["Success"] = "藝人更新成功！";
                return RedirectToAction("Artists");
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                var artistCategories = await _artistCategoryService.GetAllArtistCategoriesAsync();
                ViewBag.ArtistCategories = new SelectList(artistCategories, "Id", "Name", vm.ArtistCategoryId);
                return View("Artist/Edit", vm);
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
            var banners = await _bannerService.GetBannerListItemsAsync();
            return View("Banner/Index", banners);
        }

        public async Task<IActionResult> BannerCreate()
        {
            await PopulateBannerViewBags();
            return View("Banner/Create", new BannerFormViewModel());
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> BannerCreate(BannerFormViewModel vm, IFormFile? bannerImageFile)
        {
            if (!ModelState.IsValid)
            {
                await PopulateBannerViewBags();
                return View("Banner/Create", vm);
            }

            try
            {
                // 先建立 Banner 取得 Id，再儲存圖片
                vm.ImageUrl = string.Empty;
                await _bannerService.CreateBannerAsync(vm);

                var imageUrl = await _bannerImageService.SaveBannerImageAsync(bannerImageFile, vm.Id);
                if (!string.IsNullOrEmpty(imageUrl))
                {
                    await _bannerService.UpdateBannerImageUrlAsync(vm.Id, imageUrl);
                }

                TempData["Success"] = "幻燈片新增成功！";
                return RedirectToAction(nameof(Banners));
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                await PopulateBannerViewBags();
                return View("Banner/Create", vm);
            }
        }

        public async Task<IActionResult> BannerEdit(int id)
        {
            var vm = await _bannerService.GetBannerFormByIdAsync(id);
            if (vm == null) return NotFound();

            await PopulateBannerViewBags();
            return View("Banner/Edit", vm);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> BannerEdit(BannerFormViewModel vm, IFormFile? bannerImageFile)
        {
            if (!ModelState.IsValid)
            {
                await PopulateBannerViewBags();
                return View("Banner/Edit", vm);
            }

            try
            {
                var imageUrl = await _bannerImageService.SaveBannerImageAsync(bannerImageFile, vm.Id, vm.ImageUrl);
                vm.ImageUrl = imageUrl ?? vm.ImageUrl;

                await _bannerService.UpdateBannerAsync(vm);

                TempData["Success"] = "幻燈片更新成功！";
                return RedirectToAction(nameof(Banners));
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                await PopulateBannerViewBags();
                return View("Banner/Edit", vm);
            }
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> BannerDelete(int id)
        {
            var vm = await _bannerService.GetBannerFormByIdAsync(id);
            if (vm != null)
            {
                _bannerImageService.DeleteBannerImage(vm.ImageUrl);
                await _bannerService.DeleteBannerAsync(id);
            }
            TempData["Success"] = "幻燈片已刪除。";
            return RedirectToAction(nameof(Banners));
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> BannerToggle(int id)
        {
            var vm = await _bannerService.GetBannerFormByIdAsync(id);
            if (vm == null) return NotFound();

            vm.IsActive = !vm.IsActive;
            await _bannerService.UpdateBannerAsync(vm);

            TempData["Success"] = vm.IsActive ? "幻燈片已啟用。" : "幻燈片已停用。";
            return RedirectToAction(nameof(Banners));
        }

        private async Task PopulateBannerViewBags()
        {
            var albums = await _albumService.GetAlbumsAsync();
            ViewBag.Albums = new SelectList(albums, "Id", "Title");
        }
    }
}
