// ─────────────────────────────────────────────────────────────
// UserController.cs - 後台使用者管理
// Area: Admin
// ─────────────────────────────────────────────────────────────

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MusicShop.Service.Services.Interfaces;
using MusicShop.Web.Infrastructure;
using System.Security.Claims;

namespace MusicShop.Web.Areas.Admin.Controllers;

/// <summary>
/// 後台使用者管理控制器，負責使用者列表與角色管理
/// </summary>
public class UserController : AdminBaseController
{
    private readonly IUserService _userService;

    public UserController(IUserService userService)
    {
        _userService = userService;
    }

    // ─── 使用者列表 ───────────────────────────────────────
    public async Task<IActionResult> Index()
    {
        var userViewModels = await _userService.GetAllUsersWithRolesAsync();
        return View(userViewModels);
    }

    // ─── 切換管理員角色 ───────────────────────────────────
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleAdminRole(string userId)
    {
        var currentAdminId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(currentAdminId))
        {
            TempData[TempDataKeys.Error] = "無法取得當前使用者資訊";
            return RedirectToAction(nameof(Index));
        }

        var (success, message) = await _userService.ToggleAdminRoleAsync(userId, currentAdminId);

        if (success)
        {
            TempData[TempDataKeys.Success] = message;
        }
        else
        {
            TempData[TempDataKeys.Error] = message;
        }

        return RedirectToAction(nameof(Index));
    }

    // ─── 切換超級管理員角色（僅 SuperAdmin 可操作）─────────
    [HttpPost, ValidateAntiForgeryToken]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> ToggleSuperAdminRole(string userId)
    {
        var currentAdminId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(currentAdminId))
        {
            TempData[TempDataKeys.Error] = "無法取得當前使用者資訊";
            return RedirectToAction(nameof(Index));
        }

        var (success, message) = await _userService.ToggleSuperAdminRoleAsync(userId, currentAdminId);

        if (success)
        {
            TempData[TempDataKeys.Success] = message;
        }
        else
        {
            TempData[TempDataKeys.Error] = message;
        }

        return RedirectToAction(nameof(Index));
    }

    // ─── 手動確認使用者 Email ─────────────────────────────
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> ConfirmEmail(string userId)
    {
        var (success, message) = await _userService.AdminConfirmEmailAsync(userId);

        if (success)
            TempData[TempDataKeys.Success] = message;
        else
            TempData[TempDataKeys.Error] = message;

        return RedirectToAction(nameof(Index));
    }
}
