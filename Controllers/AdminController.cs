using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using MusicShop.Models;
using MusicShop.Services.Interface;
using MusicShop.ViewModels;

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

        public AdminController(
            IAlbumService albumService,
            IArtistCategoryService artistCategoryService,
            IArtistService artistService,
            IProductTypeService productTypeService,
            IOrderService orderService,
            IStatisticsService statisticsService,
            IUserService userService,
            UserManager<AppUser> userManager)
        {
            _albumService = albumService;
            _artistCategoryService = artistCategoryService;
            _artistService = artistService;
            _productTypeService = productTypeService;
            _orderService = orderService;
            _statisticsService = statisticsService;
            _userService = userService;
            _userManager = userManager;
        }

        // ─── 後台首頁 ───────────────────────────────────────
        public async Task<IActionResult> Index()
        {
            ViewBag.AlbumCount = await _statisticsService.GetAlbumCountAsync();
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
            return View(albums);
        }

        public async Task<IActionResult> AlbumCreate()
        {
            var artistCategories = await _artistCategoryService.GetAllArtistCategoriesAsync();
            var artists = await _artistService.GetAllArtistsAsync();
            var parentCategories = await _productTypeService.GetParentCategoriesAsync();
            var childCategories = await _productTypeService.GetAllChildCategoriesAsync();

            ViewBag.ArtistCategories = new SelectList(artistCategories, "Id", "Name");
            ViewBag.Artists = new SelectList(artists, "Id", "Name");
            ViewBag.ParentCategories = new SelectList(parentCategories, "Id", "Name");
            ViewBag.ChildCategories = childCategories;
            return View();
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> AlbumCreate(Album album)
        {
            if (!ModelState.IsValid)
            {
                var artistCategories = await _artistCategoryService.GetAllArtistCategoriesAsync();
                var artists = await _artistService.GetAllArtistsAsync();
                var parentCategories = await _productTypeService.GetParentCategoriesAsync();
                var childCategories = await _productTypeService.GetAllChildCategoriesAsync();

                ViewBag.ArtistCategories = new SelectList(artistCategories, "Id", "Name");
                ViewBag.Artists = new SelectList(artists, "Id", "Name");
                ViewBag.ParentCategories = new SelectList(parentCategories, "Id", "Name");
                ViewBag.ChildCategories = childCategories;
                return View(album);
            }

            try
            {
                album.CreatedAt = DateTime.UtcNow;
                await _albumService.CreateAlbumAsync(album);
                TempData["Success"] = "商品新增成功！";
                return RedirectToAction("Albums");
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                var artistCategories = await _artistCategoryService.GetAllArtistCategoriesAsync();
                var artists = await _artistService.GetAllArtistsAsync();
                var parentCategories = await _productTypeService.GetParentCategoriesAsync();
                var childCategories = await _productTypeService.GetAllChildCategoriesAsync();

                ViewBag.ArtistCategories = new SelectList(artistCategories, "Id", "Name");
                ViewBag.Artists = new SelectList(artists, "Id", "Name");
                ViewBag.ParentCategories = new SelectList(parentCategories, "Id", "Name");
                ViewBag.ChildCategories = childCategories;
                return View(album);
            }
        }

        public async Task<IActionResult> AlbumEdit(int id)
        {
            var album = await _albumService.GetAlbumDetailAsync(id);
            if (album == null) return NotFound();

            var artistCategories = await _artistCategoryService.GetAllArtistCategoriesAsync();
            var artists = await _artistService.GetAllArtistsAsync();
            var parentCategories = await _productTypeService.GetParentCategoriesAsync();
            var childCategories = await _productTypeService.GetAllChildCategoriesAsync();

            // 如果商品有選擇商品類型，找出它的父分類
            int? selectedParentId = null;
            if (album.ProductTypeId.HasValue && album.ProductType != null)
            {
                selectedParentId = album.ProductType.ParentId;
            }

            ViewBag.ArtistCategories = new SelectList(artistCategories, "Id", "Name", album.ArtistCategoryId);
            ViewBag.Artists = new SelectList(artists, "Id", "Name", album.ArtistId);
            ViewBag.ParentCategories = new SelectList(parentCategories, "Id", "Name", selectedParentId);
            ViewBag.ChildCategories = childCategories;
            ViewBag.SelectedProductTypeId = album.ProductTypeId;
            return View(album);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> AlbumEdit(Album album)
        {
            if (!ModelState.IsValid)
            {
                var artistCategories = await _artistCategoryService.GetAllArtistCategoriesAsync();
                var artists = await _artistService.GetAllArtistsAsync();
                var parentCategories = await _productTypeService.GetParentCategoriesAsync();
                var childCategories = await _productTypeService.GetAllChildCategoriesAsync();

                ViewBag.ArtistCategories = new SelectList(artistCategories, "Id", "Name", album.ArtistCategoryId);
                ViewBag.Artists = new SelectList(artists, "Id", "Name", album.ArtistId);
                ViewBag.ParentCategories = new SelectList(parentCategories, "Id", "Name");
                ViewBag.ChildCategories = childCategories;
                return View(album);
            }

            try
            {
                await _albumService.UpdateAlbumAsync(album);
                TempData["Success"] = "商品更新成功！";
                return RedirectToAction("Albums");
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                var artistCategories = await _artistCategoryService.GetAllArtistCategoriesAsync();
                var artists = await _artistService.GetAllArtistsAsync();
                var parentCategories = await _productTypeService.GetParentCategoriesAsync();
                var childCategories = await _productTypeService.GetAllChildCategoriesAsync();

                ViewBag.ArtistCategories = new SelectList(artistCategories, "Id", "Name", album.ArtistCategoryId);
                ViewBag.Artists = new SelectList(artists, "Id", "Name", album.ArtistId);
                ViewBag.ParentCategories = new SelectList(parentCategories, "Id", "Name");
                ViewBag.ChildCategories = childCategories;
                return View(album);
            }
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

            return View();
        }

        // 藝人分類 CRUD
        public IActionResult ArtistCategoryCreate() => View();

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> ArtistCategoryCreate(ArtistCategory artistCategory)
        {
            if (!ModelState.IsValid)
                return View(artistCategory);

            try
            {
                await _artistCategoryService.CreateArtistCategoryAsync(artistCategory);
                TempData["Success"] = "藝人分類新增成功！";
                return RedirectToAction("Categories");
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                return View(artistCategory);
            }
        }

        public async Task<IActionResult> ArtistCategoryEdit(int id)
        {
            var artistCategory = await _artistCategoryService.GetArtistCategoryByIdAsync(id);
            if (artistCategory == null) return NotFound();
            return View(artistCategory);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> ArtistCategoryEdit(ArtistCategory artistCategory)
        {
            if (!ModelState.IsValid)
                return View(artistCategory);

            try
            {
                await _artistCategoryService.UpdateArtistCategoryAsync(artistCategory);
                TempData["Success"] = "藝人分類更新成功！";
                return RedirectToAction("Categories");
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                return View(artistCategory);
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
            return View();
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> ProductTypeCreate(ProductType productType)
        {
            if (!ModelState.IsValid)
                return View(productType);

            try
            {
                await _productTypeService.CreateProductTypeAsync(productType);
                TempData["Success"] = "商品類型新增成功！";
                return RedirectToAction("Categories");
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                return View(productType);
            }
        }

        public async Task<IActionResult> ProductTypeEdit(int id)
        {
            var productType = await _productTypeService.GetProductTypeByIdAsync(id);
            if (productType == null) return NotFound();

            var parentCategories = await _productTypeService.GetParentCategoriesAsync();
            ViewBag.ParentCategories = new SelectList(parentCategories, "Id", "Name");

            return View(productType);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> ProductTypeEdit(ProductType productType)
        {
            if (!ModelState.IsValid)
                return View(productType);

            try
            {
                await _productTypeService.UpdateProductTypeAsync(productType);
                TempData["Success"] = "商品類型更新成功！";
                return RedirectToAction("Categories");
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                return View(productType);
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

        // ─── 訂單管理 ───────────────────────────────────────
        public async Task<IActionResult> Orders()
        {
            var orders = await _orderService.GetAllOrdersAsync();
            return View(orders);
        }

        public async Task<IActionResult> OrderDetail(int id)
        {
            try
            {
                var order = await _orderService.GetOrderByIdAsync(id);

                if (order == null) return NotFound();
                return View(order);
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

            return View(userViewModels);
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
    }
}
