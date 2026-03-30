// ─────────────────────────────────────────────────────────────
// AlbumController.cs - 後台商品管理
// Area: Admin
// ─────────────────────────────────────────────────────────────

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using MusicShop.Service.Services.Interfaces;
using MusicShop.Service.ViewModels.Admin;
using MusicShop.Web.Infrastructure;

namespace MusicShop.Web.Areas.Admin.Controllers;

/// <summary>
/// 後台商品管理控制器，負責商品 CRUD 操作
/// </summary>
public class AlbumController : AdminBaseController
{
    private readonly IAlbumService _albumService;
    private readonly IArtistCategoryService _artistCategoryService;
    private readonly IArtistService _artistService;
    private readonly IProductTypeService _productTypeService;
    private readonly IAlbumImageService _albumImageService;

    public AlbumController(
        IAlbumService albumService,
        IArtistCategoryService artistCategoryService,
        IArtistService artistService,
        IProductTypeService productTypeService,
        IAlbumImageService albumImageService)
    {
        _albumService = albumService;
        _artistCategoryService = artistCategoryService;
        _artistService = artistService;
        _productTypeService = productTypeService;
        _albumImageService = albumImageService;
    }

    // ─── 商品列表 ─────────────────────────────────────────
    public async Task<IActionResult> Index(int page = 1, string? keyword = null)
    {
        var pagedAlbums = await _albumService.GetAdminAlbumListPagedAsync(page, keyword);
        return View(pagedAlbums);
    }

    // ─── 新增商品 ─────────────────────────────────────────
    public async Task<IActionResult> Create()
    {
        await PopulateAlbumViewBags();
        return View(new AlbumFormViewModel());
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(AlbumFormViewModel vm, IFormFile? coverImageFile, IFormFile? descriptionImageFile)
    {
        if (!ModelState.IsValid)
        {
            await PopulateAlbumViewBags();
            return View(vm);
        }

        try
        {
            // 圖片處理採「先建後傳」兩段式流程：
            // 1. 先建立商品取得 DB 自動產生的 ID
            // 2. 以 ID 組出分類/藝人/ID 的子目錄後儲存圖片，再回寫 URL
            await _albumService.CreateAlbumAsync(vm);

            if (coverImageFile?.Length > 0 || descriptionImageFile?.Length > 0)
            {
                var subFolder = await _albumImageService.BuildSubFolderAsync(vm.ProductTypeId, vm.ArtistId, vm.Id);
                vm.CoverImageUrl = await _albumImageService.SaveImageAsync(coverImageFile, subFolder, "cover");
                vm.DescriptionImageUrl = await _albumImageService.SaveImageAsync(descriptionImageFile, subFolder, "description");
                await _albumService.UpdateAlbumAsync(vm);
            }

            TempData[TempDataKeys.Success] = "商品新增成功！";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            TempData[TempDataKeys.Error] = ex.Message;
            await PopulateAlbumViewBags();
            return View(vm);
        }
    }

    // ─── 編輯商品 ─────────────────────────────────────────
    public async Task<IActionResult> Edit(int id)
    {
        var vm = await _albumService.GetAlbumFormByIdAsync(id);
        if (vm == null) return NotFound();

        // 取得父分類 ID，供前端初始化級聯下拉選單使用
        int? selectedParentId = null;
        if (vm.ProductTypeId.HasValue)
        {
            selectedParentId = await _productTypeService.GetParentIdByProductTypeIdAsync(vm.ProductTypeId.Value);
        }

        await PopulateAlbumViewBags(vm.ArtistId);
        ViewBag.ParentCategories = new SelectList(
            await _productTypeService.GetParentCategorySelectItemsAsync(), "Id", "Name", selectedParentId);
        ViewBag.SelectedProductTypeId = vm.ProductTypeId;

        // 取得藝人分類 ID，供 JS 初始化級聯下拉選單使用
        if (vm.ArtistId.HasValue)
        {
            ViewBag.SelectedArtistCategoryId = await _artistService.GetArtistCategoryIdByArtistIdAsync(vm.ArtistId.Value);
        }

        return View(vm);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(AlbumFormViewModel vm, IFormFile? coverImageFile, IFormFile? descriptionImageFile)
    {
        if (!ModelState.IsValid)
        {
            await PopulateAlbumViewBags(vm.ArtistId);
            return View(vm);
        }

        try
        {
            // 編輯時傳入 existingUrl：若使用者未上傳新圖片，SaveImageAsync 會保留原有 URL
            var subFolder = await _albumImageService.BuildSubFolderAsync(vm.ProductTypeId, vm.ArtistId, vm.Id);
            vm.CoverImageUrl = await _albumImageService.SaveImageAsync(coverImageFile, subFolder, "cover", vm.CoverImageUrl);
            vm.DescriptionImageUrl = await _albumImageService.SaveImageAsync(descriptionImageFile, subFolder, "description", vm.DescriptionImageUrl);
            await _albumService.UpdateAlbumAsync(vm);
            TempData[TempDataKeys.Success] = "商品更新成功！";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            TempData[TempDataKeys.Error] = ex.Message;
            await PopulateAlbumViewBags(vm.ArtistId);
            return View(vm);
        }
    }

    // ─── 刪除商品 ─────────────────────────────────────────
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
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

        return RedirectToAction(nameof(Index));
    }

    /// <summary>
    /// 填充商品表單所需的下拉選單資料至 ViewBag。
    /// 包含藝人分類、藝人清單、商品父分類與子分類，供前端級聯下拉選單使用。
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
}
