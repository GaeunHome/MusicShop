// ─────────────────────────────────────────────────────────────
// ArtistController.cs - 後台藝人管理
// Area: Admin
// ─────────────────────────────────────────────────────────────

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using MusicShop.Service.Services.Interfaces;
using MusicShop.Service.ViewModels.Admin;
using MusicShop.Web.Infrastructure;

namespace MusicShop.Web.Areas.Admin.Controllers;

/// <summary>
/// 後台藝人管理控制器，負責藝人 CRUD 與上下架操作
/// </summary>
[Area("Admin")]
[Authorize(Roles = "Admin")]
public class ArtistController : Controller
{
    private readonly IArtistService _artistService;
    private readonly IArtistCategoryService _artistCategoryService;

    private const int ArtistPageSize = 10;

    public ArtistController(
        IArtistService artistService,
        IArtistCategoryService artistCategoryService)
    {
        _artistService = artistService;
        _artistCategoryService = artistCategoryService;
    }

    // ─── 藝人列表（支援分頁與篩選）─────────────────────
    public async Task<IActionResult> Index(int page = 1, int? categoryId = null, bool? isActive = null)
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

        return View(pagedResult.Items);
    }

    // ─── 新增藝人 ─────────────────────────────────────────
    public async Task<IActionResult> Create()
    {
        var artistCategories = await _artistCategoryService.GetArtistCategorySelectItemsAsync();
        ViewBag.ArtistCategories = new SelectList(artistCategories, "Id", "Name");

        // 提供目前最大排序值，方便使用者決定新藝人的排序位置
        ViewBag.MaxDisplayOrder = await _artistService.GetMaxDisplayOrderAsync();

        return View(new ArtistFormViewModel());
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ArtistFormViewModel vm)
    {
        if (!ModelState.IsValid)
        {
            var artistCategories = await _artistCategoryService.GetArtistCategorySelectItemsAsync();
            ViewBag.ArtistCategories = new SelectList(artistCategories, "Id", "Name");
            return View(vm);
        }

        try
        {
            await _artistService.CreateArtistAsync(vm);
            TempData[TempDataKeys.Success] = "藝人新增成功！";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            TempData[TempDataKeys.Error] = ex.Message;
            var artistCategories = await _artistCategoryService.GetArtistCategorySelectItemsAsync();
            ViewBag.ArtistCategories = new SelectList(artistCategories, "Id", "Name");
            return View(vm);
        }
    }

    // ─── 編輯藝人 ─────────────────────────────────────────
    public async Task<IActionResult> Edit(int id)
    {
        var vm = await _artistService.GetArtistFormByIdAsync(id);
        if (vm == null) return NotFound();

        var artistCategories = await _artistCategoryService.GetArtistCategorySelectItemsAsync();
        ViewBag.ArtistCategories = new SelectList(artistCategories, "Id", "Name", vm.ArtistCategoryId);
        return View(vm);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(ArtistFormViewModel vm)
    {
        if (!ModelState.IsValid)
        {
            var artistCategories = await _artistCategoryService.GetArtistCategorySelectItemsAsync();
            ViewBag.ArtistCategories = new SelectList(artistCategories, "Id", "Name", vm.ArtistCategoryId);
            return View(vm);
        }

        try
        {
            await _artistService.UpdateArtistAsync(vm);
            TempData[TempDataKeys.Success] = "藝人更新成功！";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            TempData[TempDataKeys.Error] = ex.Message;
            var artistCategories = await _artistCategoryService.GetArtistCategorySelectItemsAsync();
            ViewBag.ArtistCategories = new SelectList(artistCategories, "Id", "Name", vm.ArtistCategoryId);
            return View(vm);
        }
    }

    // ─── 刪除藝人 ─────────────────────────────────────────
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
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

        return RedirectToAction(nameof(Index));
    }

    // ─── 切換上下架狀態 ───────────────────────────────────
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleActive(int id)
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

        return RedirectToAction(nameof(Index));
    }

    // ─── API：根據藝人分類取得藝人列表（級聯下拉選單用）─
    [HttpGet]
    public async Task<IActionResult> GetByCategory(int categoryId)
    {
        var artists = await _artistService.GetArtistSelectItemsByCategoryIdAsync(categoryId);
        var result = artists.Select(a => new { id = a.Id, name = a.Name });
        return Json(result);
    }
}
