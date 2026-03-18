using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using MusicShop.Service.Services.Interfaces;
using MusicShop.Service.ViewModels.Admin;
using MusicShop.Web.Infrastructure;

namespace MusicShop.Web.Areas.Admin.Controllers;

/// <summary>
/// 後台精選藝人管理控制器
/// </summary>
[Area("Admin")]
[Authorize(Roles = "Admin")]
public class FeaturedArtistController : Controller
{
    private readonly IFeaturedArtistService _featuredArtistService;
    private readonly IArtistService _artistService;

    public FeaturedArtistController(
        IFeaturedArtistService featuredArtistService,
        IArtistService artistService)
    {
        _featuredArtistService = featuredArtistService;
        _artistService = artistService;
    }

    // ─── 精選藝人列表 ───────────────────────────────────────
    public async Task<IActionResult> Index()
    {
        var items = await _featuredArtistService.GetFeaturedArtistListItemsAsync();
        return View(items);
    }

    // ─── 新增精選藝人 ───────────────────────────────────────
    public async Task<IActionResult> Create()
    {
        await PopulateArtistViewBags();
        return View(new FeaturedArtistFormViewModel());
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(FeaturedArtistFormViewModel vm)
    {
        if (!ModelState.IsValid)
        {
            await PopulateArtistViewBags();
            return View(vm);
        }

        try
        {
            await _featuredArtistService.CreateFeaturedArtistAsync(vm);
            TempData[TempDataKeys.Success] = "精選藝人新增成功！";
            return RedirectToAction(nameof(Index));
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            await PopulateArtistViewBags();
            return View(vm);
        }
    }

    // ─── 編輯精選藝人 ───────────────────────────────────────
    public async Task<IActionResult> Edit(int id)
    {
        var vm = await _featuredArtistService.GetFeaturedArtistFormByIdAsync(id);
        if (vm == null) return NotFound();

        await PopulateArtistViewBags();
        return View(vm);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(FeaturedArtistFormViewModel vm)
    {
        if (!ModelState.IsValid)
        {
            await PopulateArtistViewBags();
            return View(vm);
        }

        try
        {
            await _featuredArtistService.UpdateFeaturedArtistAsync(vm);
            TempData[TempDataKeys.Success] = "精選藝人更新成功！";
            return RedirectToAction(nameof(Index));
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            await PopulateArtistViewBags();
            return View(vm);
        }
    }

    // ─── 刪除精選藝人 ───────────────────────────────────────
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        await _featuredArtistService.DeleteFeaturedArtistAsync(id);
        TempData[TempDataKeys.Success] = "精選藝人已移除。";
        return RedirectToAction(nameof(Index));
    }

    /// <summary>
    /// 填充藝人下拉選單至 ViewBag
    /// </summary>
    private async Task PopulateArtistViewBags()
    {
        var artists = await _artistService.GetArtistSelectItemsAsync();
        ViewBag.Artists = new SelectList(artists, "Id", "Name");
    }
}
