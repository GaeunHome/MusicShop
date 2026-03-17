// ─────────────────────────────────────────────────────────────
// AdminController.Album.cs - Album management partial class
// Responsibility: Album CRUD actions and PopulateAlbumViewBags helper
// ─────────────────────────────────────────────────────────────

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using MusicShop.Service.ViewModels.Admin;
using MusicShop.Web.Infrastructure;

namespace MusicShop.Controllers
{
    public partial class AdminController
    {
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
                // 圖片處理採「先建後傳」兩段式流程：
                // 1. 先建立商品取得 DB 自動產生的 ID
                // 2. 以 ID 組出分類/藝人/ID 的子目錄後儲存圖片，再回寫 URL
                // 原因：圖片儲存路徑依賴商品 ID，而 ID 在 INSERT 後才確定
                await _albumService.CreateAlbumAsync(vm);

                if (coverImageFile?.Length > 0 || descriptionImageFile?.Length > 0)
                {
                    var subFolder = await _albumImageService.BuildSubFolderAsync(vm.ProductTypeId, vm.ArtistId, vm.Id);
                    vm.CoverImageUrl = await _albumImageService.SaveImageAsync(coverImageFile, subFolder, "cover");
                    vm.DescriptionImageUrl = await _albumImageService.SaveImageAsync(descriptionImageFile, subFolder, "description");
                    await _albumService.UpdateAlbumAsync(vm);
                }

                TempData[TempDataKeys.Success] = "商品新增成功！";
                return RedirectToAction("Albums");
            }
            catch (Exception ex)
            {
                TempData[TempDataKeys.Error] = ex.Message;
                await PopulateAlbumViewBags();
                return View("Album/Create", vm);
            }
        }

        public async Task<IActionResult> AlbumEdit(int id)
        {
            var vm = await _albumService.GetAlbumFormByIdAsync(id);
            if (vm == null) return NotFound();

            // 使用 ViewModel 方法取得父分類 ID，避免直接接觸 Entity
            int? selectedParentId = null;
            if (vm.ProductTypeId.HasValue)
            {
                selectedParentId = await _productTypeService.GetParentIdByProductTypeIdAsync(vm.ProductTypeId.Value);
            }

            await PopulateAlbumViewBags(vm.ArtistId);
            ViewBag.ParentCategories = new SelectList(
                await _productTypeService.GetParentCategorySelectItemsAsync(), "Id", "Name", selectedParentId);
            ViewBag.SelectedProductTypeId = vm.ProductTypeId;

            // 使用 ViewModel 方法取得藝人分類 ID，供 JS 初始化級聯下拉選單使用
            if (vm.ArtistId.HasValue)
            {
                ViewBag.SelectedArtistCategoryId = await _artistService.GetArtistCategoryIdByArtistIdAsync(vm.ArtistId.Value);
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
                // 編輯時傳入 existingUrl：若使用者未上傳新圖片，SaveImageAsync 會保留原有 URL
                var subFolder = await _albumImageService.BuildSubFolderAsync(vm.ProductTypeId, vm.ArtistId, vm.Id);
                vm.CoverImageUrl = await _albumImageService.SaveImageAsync(coverImageFile, subFolder, "cover", vm.CoverImageUrl);
                vm.DescriptionImageUrl = await _albumImageService.SaveImageAsync(descriptionImageFile, subFolder, "description", vm.DescriptionImageUrl);
                await _albumService.UpdateAlbumAsync(vm);
                TempData[TempDataKeys.Success] = "商品更新成功！";
                return RedirectToAction("Albums");
            }
            catch (Exception ex)
            {
                TempData[TempDataKeys.Error] = ex.Message;
                await PopulateAlbumViewBags(vm.ArtistId);
                return View("Album/Edit", vm);
            }
        }

        /// <summary>
        /// 填充商品表單所需的下拉選單資料至 ViewBag。
        /// 包含藝人分類、藝人清單、商品父分類與子分類，供前端級聯下拉選單使用。
        /// 在 Create/Edit 的 GET 與驗證失敗回傳時都需呼叫，故抽為共用方法。
        /// 所有資料均使用 ViewModel（SelectItemViewModel），不接觸資料層 Entity。
        /// </summary>
        private async Task PopulateAlbumViewBags(int? selectedArtistId = null)
        {
            var artistCategories = await _artistCategoryService.GetArtistCategorySelectItemsAsync();
            var artists = await _artistService.GetArtistSelectItemsAsync();
            var parentCategories = await _productTypeService.GetParentCategorySelectItemsAsync();
            var childCategories = await _productTypeService.GetChildCategorySelectItemsAsync();

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
                TempData[TempDataKeys.Success] = "商品刪除成功！";
            }
            catch (Exception ex)
            {
                TempData[TempDataKeys.Error] = ex.Message;
            }

            return RedirectToAction("Albums");
        }
    }
}
