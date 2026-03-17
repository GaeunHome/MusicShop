// ─────────────────────────────────────────────────────────────
// AdminController.Category.cs - Category & Artist management partial class
// Responsibility: ArtistCategory CRUD, ProductType CRUD, Artist CRUD,
//                 and cascade dropdown API endpoints
// ─────────────────────────────────────────────────────────────

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using MusicShop.Service.ViewModels.Admin;
using MusicShop.Web.Infrastructure;

namespace MusicShop.Controllers
{
    public partial class AdminController
    {
        // ─── 分類管理 ───────────────────────────────────────
        public async Task<IActionResult> Categories()
        {
            var vm = new CategoryManagementViewModel
            {
                ArtistCategories = await _artistCategoryService.GetArtistCategoryListItemsAsync(),
                CategoryTree = await _productTypeService.GetCategoryTreeViewModelsAsync()
            };

            return View("Category/Index", vm);
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
                TempData[TempDataKeys.Success] = "藝人分類新增成功！";
                return RedirectToAction("Categories");
            }
            catch (Exception ex)
            {
                TempData[TempDataKeys.Error] = ex.Message;
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
                TempData[TempDataKeys.Success] = "藝人分類更新成功！";
                return RedirectToAction("Categories");
            }
            catch (Exception ex)
            {
                TempData[TempDataKeys.Error] = ex.Message;
                return View("Category/ArtistCategory/Edit", vm);
            }
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> ArtistCategoryDelete(int id)
        {
            try
            {
                await _artistCategoryService.DeleteArtistCategoryAsync(id);
                TempData[TempDataKeys.Success] = "藝人分類刪除成功！";
            }
            catch (Exception ex)
            {
                TempData[TempDataKeys.Error] = ex.Message;
            }

            return RedirectToAction("Categories");
        }

        // 商品類型 CRUD
        public async Task<IActionResult> ProductTypeCreate()
        {
            var parentCategories = await _productTypeService.GetParentCategorySelectItemsAsync();
            ViewBag.ParentCategories = new SelectList(parentCategories, "Id", "Name");
            return View("Category/ProductType/Create", new ProductTypeFormViewModel());
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> ProductTypeCreate(ProductTypeFormViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                var parentCategories = await _productTypeService.GetParentCategorySelectItemsAsync();
                ViewBag.ParentCategories = new SelectList(parentCategories, "Id", "Name");
                return View("Category/ProductType/Create", vm);
            }

            try
            {
                await _productTypeService.CreateProductTypeAsync(vm);
                TempData[TempDataKeys.Success] = "商品類型新增成功！";
                return RedirectToAction("Categories");
            }
            catch (Exception ex)
            {
                TempData[TempDataKeys.Error] = ex.Message;
                var parentCategories = await _productTypeService.GetParentCategorySelectItemsAsync();
                ViewBag.ParentCategories = new SelectList(parentCategories, "Id", "Name");
                return View("Category/ProductType/Create", vm);
            }
        }

        public async Task<IActionResult> ProductTypeEdit(int id)
        {
            var vm = await _productTypeService.GetProductTypeFormByIdAsync(id);
            if (vm == null) return NotFound();

            var parentCategories = await _productTypeService.GetParentCategorySelectItemsAsync();
            ViewBag.ParentCategories = new SelectList(parentCategories, "Id", "Name");

            return View("Category/ProductType/Edit", vm);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> ProductTypeEdit(ProductTypeFormViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                var parentCategories = await _productTypeService.GetParentCategorySelectItemsAsync();
                ViewBag.ParentCategories = new SelectList(parentCategories, "Id", "Name");
                return View("Category/ProductType/Edit", vm);
            }

            try
            {
                await _productTypeService.UpdateProductTypeAsync(vm);
                TempData[TempDataKeys.Success] = "商品類型更新成功！";
                return RedirectToAction("Categories");
            }
            catch (Exception ex)
            {
                TempData[TempDataKeys.Error] = ex.Message;
                var parentCategories = await _productTypeService.GetParentCategorySelectItemsAsync();
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
                TempData[TempDataKeys.Success] = "商品類型刪除成功！";
            }
            catch (Exception ex)
            {
                TempData[TempDataKeys.Error] = ex.Message;
            }

            return RedirectToAction("Categories");
        }

        // API: 根據父分類 ID 取得子分類（用於級聯下拉選單）
        // 使用 ViewModel 方法，避免在 Controller 中直接操作 Entity
        [HttpGet]
        public async Task<IActionResult> GetChildCategories(int parentId)
        {
            var children = await _productTypeService.GetChildCategorySelectItemsByParentIdAsync(parentId);
            var result = children.Select(c => new { id = c.Id, name = c.Name });
            return Json(result);
        }

        // ─── 藝人管理 ───────────────────────────────────────
        private const int ArtistPageSize = 10;

        public async Task<IActionResult> Artists(int page = 1, int? categoryId = null, bool? isActive = null)
        {
            if (page < 1) page = 1;

            var pagedResult = await _artistService.GetArtistListItemsPagedAsync(
                page, ArtistPageSize, categoryId, isActive);

            // 分頁資訊
            ViewBag.CurrentPage = pagedResult.CurrentPage;
            ViewBag.TotalPages = pagedResult.TotalPages;
            ViewBag.HasPreviousPage = pagedResult.HasPreviousPage;
            ViewBag.HasNextPage = pagedResult.HasNextPage;
            ViewBag.TotalCount = pagedResult.TotalCount;

            // 篩選條件（用於 UI 回顯）
            ViewBag.SelectedCategoryId = categoryId;
            ViewBag.SelectedIsActive = isActive;

            // 分類下拉選單
            var artistCategories = await _artistCategoryService.GetArtistCategorySelectItemsAsync();
            ViewBag.ArtistCategories = new SelectList(artistCategories, "Id", "Name", categoryId);

            return View("Artist/Index", pagedResult.Items);
        }

        public async Task<IActionResult> ArtistCreate()
        {
            // 使用 ViewModel 取代 Entity，確保 Controller 不接觸資料層實體
            var artistCategories = await _artistCategoryService.GetArtistCategorySelectItemsAsync();
            ViewBag.ArtistCategories = new SelectList(artistCategories, "Id", "Name");

            // 提供目前最大排序值，方便使用者決定新藝人的排序位置
            ViewBag.MaxDisplayOrder = await _artistService.GetMaxDisplayOrderAsync();

            return View("Artist/Create", new ArtistFormViewModel());
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> ArtistCreate(ArtistFormViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                var artistCategories = await _artistCategoryService.GetArtistCategorySelectItemsAsync();
                ViewBag.ArtistCategories = new SelectList(artistCategories, "Id", "Name");
                return View("Artist/Create", vm);
            }

            try
            {
                await _artistService.CreateArtistAsync(vm);
                TempData[TempDataKeys.Success] = "藝人新增成功！";
                return RedirectToAction("Artists");
            }
            catch (Exception ex)
            {
                TempData[TempDataKeys.Error] = ex.Message;
                var artistCategories = await _artistCategoryService.GetArtistCategorySelectItemsAsync();
                ViewBag.ArtistCategories = new SelectList(artistCategories, "Id", "Name");
                return View("Artist/Create", vm);
            }
        }

        public async Task<IActionResult> ArtistEdit(int id)
        {
            var vm = await _artistService.GetArtistFormByIdAsync(id);
            if (vm == null) return NotFound();

            var artistCategories = await _artistCategoryService.GetArtistCategorySelectItemsAsync();
            ViewBag.ArtistCategories = new SelectList(artistCategories, "Id", "Name", vm.ArtistCategoryId);
            return View("Artist/Edit", vm);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> ArtistEdit(ArtistFormViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                var artistCategories = await _artistCategoryService.GetArtistCategorySelectItemsAsync();
                ViewBag.ArtistCategories = new SelectList(artistCategories, "Id", "Name", vm.ArtistCategoryId);
                return View("Artist/Edit", vm);
            }

            try
            {
                await _artistService.UpdateArtistAsync(vm);
                TempData[TempDataKeys.Success] = "藝人更新成功！";
                return RedirectToAction("Artists");
            }
            catch (Exception ex)
            {
                TempData[TempDataKeys.Error] = ex.Message;
                var artistCategories = await _artistCategoryService.GetArtistCategorySelectItemsAsync();
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
                TempData[TempDataKeys.Success] = "藝人刪除成功！";
            }
            catch (Exception ex)
            {
                TempData[TempDataKeys.Error] = ex.Message;
            }

            return RedirectToAction("Artists");
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> ArtistToggleActive(int id)
        {
            try
            {
                await _artistService.ToggleArtistActiveAsync(id);
                TempData[TempDataKeys.Success] = "藝人狀態已更新！";
            }
            catch (Exception ex)
            {
                TempData[TempDataKeys.Error] = ex.Message;
            }

            return RedirectToAction("Artists");
        }

        // API: 根據藝人分類 ID 取得藝人列表（用於級聯下拉選單）
        // 使用 ViewModel 方法，避免在 Controller 中直接操作 Entity
        [HttpGet]
        public async Task<IActionResult> GetArtistsByCategory(int categoryId)
        {
            var artists = await _artistService.GetArtistSelectItemsByCategoryIdAsync(categoryId);
            var result = artists.Select(a => new { id = a.Id, name = a.Name });
            return Json(result);
        }
    }
}
