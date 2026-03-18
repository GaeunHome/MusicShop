// ─────────────────────────────────────────────────────────────
// CategoryController.cs - 後台分類管理
// Area: Admin
// ─────────────────────────────────────────────────────────────

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using MusicShop.Service.Services.Interfaces;
using MusicShop.Service.ViewModels.Admin;
using MusicShop.Web.Infrastructure;

namespace MusicShop.Web.Areas.Admin.Controllers;

/// <summary>
/// 後台分類管理控制器，負責藝人分類與商品類型的 CRUD 操作
/// </summary>
public class CategoryController : AdminBaseController
{
    private readonly IArtistCategoryService _artistCategoryService;
    private readonly IProductTypeService _productTypeService;

    public CategoryController(
        IArtistCategoryService artistCategoryService,
        IProductTypeService productTypeService)
    {
        _artistCategoryService = artistCategoryService;
        _productTypeService = productTypeService;
    }

    // ─── 分類管理首頁 ─────────────────────────────────────
    public async Task<IActionResult> Index()
    {
        var vm = new CategoryManagementViewModel
        {
            ArtistCategories = await _artistCategoryService.GetArtistCategoryListItemsAsync(),
            CategoryTree = await _productTypeService.GetCategoryTreeViewModelsAsync()
        };

        return View(vm);
    }

    // ═══════════════════════════════════════════════════════
    // 藝人分類 CRUD
    // ═══════════════════════════════════════════════════════

    public IActionResult ArtistCategoryCreate() => View(new ArtistCategoryFormViewModel());

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> ArtistCategoryCreate(ArtistCategoryFormViewModel vm)
    {
        if (!ModelState.IsValid)
            return View(vm);

        try
        {
            await _artistCategoryService.CreateArtistCategoryAsync(vm);
            TempData[TempDataKeys.Success] = "藝人分類新增成功！";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            TempData[TempDataKeys.Error] = ex.Message;
            return View(vm);
        }
    }

    public async Task<IActionResult> ArtistCategoryEdit(int id)
    {
        var vm = await _artistCategoryService.GetArtistCategoryFormByIdAsync(id);
        if (vm == null) return NotFound();
        return View(vm);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> ArtistCategoryEdit(ArtistCategoryFormViewModel vm)
    {
        if (!ModelState.IsValid)
            return View(vm);

        try
        {
            await _artistCategoryService.UpdateArtistCategoryAsync(vm);
            TempData[TempDataKeys.Success] = "藝人分類更新成功！";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            TempData[TempDataKeys.Error] = ex.Message;
            return View(vm);
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

        return RedirectToAction(nameof(Index));
    }

    // ═══════════════════════════════════════════════════════
    // 商品類型 CRUD
    // ═══════════════════════════════════════════════════════

    public async Task<IActionResult> ProductTypeCreate()
    {
        var parentCategories = await _productTypeService.GetParentCategorySelectItemsAsync();
        ViewBag.ParentCategories = new SelectList(parentCategories, "Id", "Name");
        return View(new ProductTypeFormViewModel());
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> ProductTypeCreate(ProductTypeFormViewModel vm)
    {
        if (!ModelState.IsValid)
        {
            var parentCategories = await _productTypeService.GetParentCategorySelectItemsAsync();
            ViewBag.ParentCategories = new SelectList(parentCategories, "Id", "Name");
            return View(vm);
        }

        try
        {
            await _productTypeService.CreateProductTypeAsync(vm);
            TempData[TempDataKeys.Success] = "商品類型新增成功！";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            TempData[TempDataKeys.Error] = ex.Message;
            var parentCategories = await _productTypeService.GetParentCategorySelectItemsAsync();
            ViewBag.ParentCategories = new SelectList(parentCategories, "Id", "Name");
            return View(vm);
        }
    }

    public async Task<IActionResult> ProductTypeEdit(int id)
    {
        var vm = await _productTypeService.GetProductTypeFormByIdAsync(id);
        if (vm == null) return NotFound();

        var parentCategories = await _productTypeService.GetParentCategorySelectItemsAsync();
        ViewBag.ParentCategories = new SelectList(parentCategories, "Id", "Name");

        return View(vm);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> ProductTypeEdit(ProductTypeFormViewModel vm)
    {
        if (!ModelState.IsValid)
        {
            var parentCategories = await _productTypeService.GetParentCategorySelectItemsAsync();
            ViewBag.ParentCategories = new SelectList(parentCategories, "Id", "Name");
            return View(vm);
        }

        try
        {
            await _productTypeService.UpdateProductTypeAsync(vm);
            TempData[TempDataKeys.Success] = "商品類型更新成功！";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            TempData[TempDataKeys.Error] = ex.Message;
            var parentCategories = await _productTypeService.GetParentCategorySelectItemsAsync();
            ViewBag.ParentCategories = new SelectList(parentCategories, "Id", "Name");
            return View(vm);
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

        return RedirectToAction(nameof(Index));
    }

    // ─── API：根據父分類 ID 取得子分類（級聯下拉選單用）─
    [HttpGet]
    public async Task<IActionResult> GetChildCategories(int parentId)
    {
        var children = await _productTypeService.GetChildCategorySelectItemsByParentIdAsync(parentId);
        var result = children.Select(c => new { id = c.Id, name = c.Name });
        return Json(result);
    }
}
