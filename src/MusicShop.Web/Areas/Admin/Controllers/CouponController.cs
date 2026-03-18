using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MusicShop.Service.Services.Interfaces;
using MusicShop.Service.ViewModels.Admin;
using MusicShop.Web.Infrastructure;

namespace MusicShop.Web.Areas.Admin.Controllers;

/// <summary>
/// 後台優惠券管理控制器
/// </summary>
[Area("Admin")]
[Authorize(Roles = "Admin")]
public class CouponController : Controller
{
    private readonly ICouponService _couponService;

    public CouponController(ICouponService couponService)
    {
        _couponService = couponService;
    }

    // ─── 優惠券列表 ───────────────────────────────────────
    public async Task<IActionResult> Index()
    {
        var items = await _couponService.GetCouponListItemsAsync();
        return View(items);
    }

    // ─── 新增優惠券 ───────────────────────────────────────
    public IActionResult Create()
    {
        return View(new CouponFormViewModel());
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CouponFormViewModel vm)
    {
        if (!ModelState.IsValid)
            return View(vm);

        try
        {
            await _couponService.CreateCouponAsync(vm);
            TempData[TempDataKeys.Success] = "優惠券新增成功！";
            return RedirectToAction(nameof(Index));
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(vm);
        }
    }

    // ─── 編輯優惠券 ───────────────────────────────────────
    public async Task<IActionResult> Edit(int id)
    {
        var vm = await _couponService.GetCouponFormByIdAsync(id);
        if (vm == null) return NotFound();
        return View(vm);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(CouponFormViewModel vm)
    {
        if (!ModelState.IsValid)
            return View(vm);

        try
        {
            await _couponService.UpdateCouponAsync(vm);
            TempData[TempDataKeys.Success] = "優惠券更新成功！";
            return RedirectToAction(nameof(Index));
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(vm);
        }
    }

    // ─── 刪除優惠券 ───────────────────────────────────────
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        await _couponService.DeleteCouponAsync(id);
        TempData[TempDataKeys.Success] = "優惠券已刪除。";
        return RedirectToAction(nameof(Index));
    }

    // ─── 發放優惠券給使用者 ───────────────────────────────
    public async Task<IActionResult> Issue(int id)
    {
        var coupon = await _couponService.GetCouponFormByIdAsync(id);
        if (coupon == null) return NotFound();

        return View(new CouponIssueViewModel
        {
            CouponId = id,
            CouponName = coupon.Name
        });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Issue(CouponIssueViewModel vm)
    {
        if (!ModelState.IsValid)
            return View(vm);

        var (success, message) = await _couponService.IssueCouponToUserAsync(vm.CouponId, vm.UserEmail);

        if (success)
        {
            TempData[TempDataKeys.Success] = message;
            return RedirectToAction(nameof(Index));
        }

        ModelState.AddModelError(string.Empty, message);
        return View(vm);
    }

    // ─── 統一發放優惠券給所有使用者 ──────────────────────────
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> IssueAll(int couponId)
    {
        try
        {
            var count = await _couponService.IssueCouponToAllUsersAsync(couponId);
            TempData[TempDataKeys.Success] = $"已統一發放優惠券給 {count} 位使用者！";
        }
        catch (InvalidOperationException ex)
        {
            TempData[TempDataKeys.Error] = ex.Message;
        }

        return RedirectToAction(nameof(Index));
    }

    // ─── 發放生日優惠券 ───────────────────────────────────
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> IssueBirthday(int couponId)
    {
        try
        {
            var count = await _couponService.IssueBirthdayCouponsAsync(couponId);
            TempData[TempDataKeys.Success] = $"已發放生日優惠券給 {count} 位當月壽星！";
        }
        catch (InvalidOperationException ex)
        {
            TempData[TempDataKeys.Error] = ex.Message;
        }

        return RedirectToAction(nameof(Index));
    }
}
