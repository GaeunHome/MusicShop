// ─────────────────────────────────────────────────────────────
// SystemSettingController.cs - 後台系統參數管理
// Area: Admin（僅 SuperAdmin 可存取）
// ─────────────────────────────────────────────────────────────

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MusicShop.Controllers;
using MusicShop.Service.Services.Interfaces;
using MusicShop.Service.ViewModels.Admin;
using MusicShop.Web.Infrastructure;

namespace MusicShop.Web.Areas.Admin.Controllers;

/// <summary>
/// 後台系統參數管理控制器，僅 SuperAdmin 可存取
/// 不繼承 AdminBaseController（授權層級不同），直接標記 Area 與角色
/// </summary>
[Area("Admin")]
[Authorize(Roles = "SuperAdmin")]
public class SystemSettingController : BaseController
{
    private readonly ISystemSettingService _systemSettingService;

    public SystemSettingController(ISystemSettingService systemSettingService)
    {
        _systemSettingService = systemSettingService;
    }

    // ─── 系統參數列表 ───────────────────────────────────────
    public async Task<IActionResult> Index()
    {
        var settings = await _systemSettingService.GetAllSettingsAsync();
        return View(settings);
    }

    // ─── 新增系統參數 ───────────────────────────────────────
    public IActionResult Create()
    {
        return View(new SystemSettingFormViewModel());
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(SystemSettingFormViewModel vm)
    {
        if (!ModelState.IsValid)
        {
            return View(vm);
        }

        try
        {
            var userId = GetAuthorizedUserId();
            await _systemSettingService.CreateSettingAsync(vm, userId);

            TempData[TempDataKeys.Success] = $"系統參數 '{vm.Key}' 新增成功！";
            return RedirectToAction(nameof(Index));
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(vm);
        }
    }

    // ─── 編輯系統參數 ───────────────────────────────────────
    public async Task<IActionResult> Edit(int id)
    {
        var vm = await _systemSettingService.GetSettingFormByIdAsync(id);
        if (vm == null) return NotFound();

        return View(vm);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(SystemSettingFormViewModel vm)
    {
        if (!ModelState.IsValid)
        {
            return View(vm);
        }

        try
        {
            var userId = GetAuthorizedUserId();
            await _systemSettingService.UpdateSettingAsync(vm, userId);

            TempData[TempDataKeys.Success] = $"系統參數 '{vm.Key}' 更新成功！";
            return RedirectToAction(nameof(Index));
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(vm);
        }
    }

    // ─── 刪除系統參數 ───────────────────────────────────────
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            await _systemSettingService.DeleteSettingAsync(id);
            TempData[TempDataKeys.Success] = "系統參數已刪除。";
        }
        catch (KeyNotFoundException)
        {
            TempData[TempDataKeys.Error] = "找不到指定的系統參數。";
        }

        return RedirectToAction(nameof(Index));
    }
}
