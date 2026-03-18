// ─────────────────────────────────────────────────────────────
// BannerController.cs - 後台幻燈片管理
// Area: Admin
// ─────────────────────────────────────────────────────────────

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using MusicShop.Service.Services.Interfaces;
using MusicShop.Service.ViewModels.Admin;
using MusicShop.Web.Infrastructure;

namespace MusicShop.Web.Areas.Admin.Controllers;

/// <summary>
/// 後台幻燈片管理控制器，負責幻燈片 CRUD 與啟用/停用操作
/// </summary>
public class BannerController : AdminBaseController
{
    private readonly IBannerService _bannerService;
    private readonly IAlbumService _albumService;
    private readonly IBannerImageService _bannerImageService;

    public BannerController(
        IBannerService bannerService,
        IAlbumService albumService,
        IBannerImageService bannerImageService)
    {
        _bannerService = bannerService;
        _albumService = albumService;
        _bannerImageService = bannerImageService;
    }

    // ─── 幻燈片列表 ───────────────────────────────────────
    public async Task<IActionResult> Index()
    {
        var banners = await _bannerService.GetBannerListItemsAsync();
        return View(banners);
    }

    // ─── 新增幻燈片 ───────────────────────────────────────
    public async Task<IActionResult> Create()
    {
        await PopulateBannerViewBags();
        return View(new BannerFormViewModel());
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(BannerFormViewModel vm, IFormFile? bannerImageFile)
    {
        if (!ModelState.IsValid)
        {
            await PopulateBannerViewBags();
            return View(vm);
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
            return RedirectToAction(nameof(Index));
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            await PopulateBannerViewBags();
            return View(vm);
        }
    }

    // ─── 編輯幻燈片 ───────────────────────────────────────
    public async Task<IActionResult> Edit(int id)
    {
        var vm = await _bannerService.GetBannerFormByIdAsync(id);
        if (vm == null) return NotFound();

        await PopulateBannerViewBags();
        return View(vm);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(BannerFormViewModel vm, IFormFile? bannerImageFile)
    {
        if (!ModelState.IsValid)
        {
            await PopulateBannerViewBags();
            return View(vm);
        }

        try
        {
            var imageUrl = await _bannerImageService.SaveBannerImageAsync(bannerImageFile, vm.Id, vm.ImageUrl);
            vm.ImageUrl = imageUrl ?? vm.ImageUrl;

            await _bannerService.UpdateBannerAsync(vm);

            TempData[TempDataKeys.Success] = "幻燈片更新成功！";
            return RedirectToAction(nameof(Index));
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            await PopulateBannerViewBags();
            return View(vm);
        }
    }

    // ─── 刪除幻燈片 ───────────────────────────────────────
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var vm = await _bannerService.GetBannerFormByIdAsync(id);
        if (vm != null)
        {
            _bannerImageService.DeleteBannerImage(vm.ImageUrl);
            await _bannerService.DeleteBannerAsync(id);
        }
        TempData[TempDataKeys.Success] = "幻燈片已刪除。";
        return RedirectToAction(nameof(Index));
    }

    // ─── 切換啟用/停用 ───────────────────────────────────
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Toggle(int id)
    {
        var vm = await _bannerService.GetBannerFormByIdAsync(id);
        if (vm == null) return NotFound();

        vm.IsActive = !vm.IsActive;
        await _bannerService.UpdateBannerAsync(vm);

        TempData[TempDataKeys.Success] = vm.IsActive ? "幻燈片已啟用。" : "幻燈片已停用。";
        return RedirectToAction(nameof(Index));
    }

    /// <summary>
    /// 填充幻燈片表單所需的商品下拉選單至 ViewBag
    /// </summary>
    private async Task PopulateBannerViewBags()
    {
        var albums = await _albumService.GetAlbumSelectItemsAsync();
        ViewBag.Albums = new SelectList(albums, "Id", "Name");
    }
}
