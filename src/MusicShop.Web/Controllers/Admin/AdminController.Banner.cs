// ─────────────────────────────────────────────────────────────
// AdminController.Banner.cs - Banner management partial class
// Responsibility: Banner CRUD, toggle, and PopulateBannerViewBags helper
// ─────────────────────────────────────────────────────────────

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using MusicShop.Service.ViewModels.Admin;
using MusicShop.Web.Infrastructure;

namespace MusicShop.Controllers
{
    public partial class AdminController
    {
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

                TempData[TempDataKeys.Success] = "幻燈片新增成功！";
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

                TempData[TempDataKeys.Success] = "幻燈片更新成功！";
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
            TempData[TempDataKeys.Success] = "幻燈片已刪除。";
            return RedirectToAction(nameof(Banners));
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> BannerToggle(int id)
        {
            var vm = await _bannerService.GetBannerFormByIdAsync(id);
            if (vm == null) return NotFound();

            vm.IsActive = !vm.IsActive;
            await _bannerService.UpdateBannerAsync(vm);

            TempData[TempDataKeys.Success] = vm.IsActive ? "幻燈片已啟用。" : "幻燈片已停用。";
            return RedirectToAction(nameof(Banners));
        }

        /// <summary>
        /// 填充幻燈片表單所需的商品下拉選單至 ViewBag，供幻燈片聯動商品選擇使用。
        /// 使用 ViewModel（SelectItemViewModel），不接觸資料層 Entity。
        /// </summary>
        private async Task PopulateBannerViewBags()
        {
            var albums = await _albumService.GetAlbumSelectItemsAsync();
            ViewBag.Albums = new SelectList(albums, "Id", "Name");
        }
    }
}
