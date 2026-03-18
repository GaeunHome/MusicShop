using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MusicShop.Service.Services.Interfaces;
using MusicShop.Web.Infrastructure;

namespace MusicShop.Controllers;

/// <summary>
/// 使用者優惠券控制器（我的優惠券 + 兌換）
/// </summary>
[Authorize]
public class CouponController : BaseController
{
    private readonly ICouponService _couponService;

    public CouponController(ICouponService couponService)
    {
        _couponService = couponService;
    }

    // GET: /Coupon
    public async Task<IActionResult> Index()
    {
        var userId = GetAuthorizedUserId();
        var coupons = await _couponService.GetUserCouponsAsync(userId);
        return View(coupons);
    }

    // POST: /Coupon/Redeem
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Redeem(string code)
    {
        var userId = GetAuthorizedUserId();

        if (string.IsNullOrWhiteSpace(code))
        {
            TempData[TempDataKeys.Error] = "請輸入兌換碼";
            return RedirectToAction(nameof(Index));
        }

        var (success, message) = await _couponService.RedeemCouponByCodeAsync(userId, code);

        if (success)
            TempData[TempDataKeys.Success] = message;
        else
            TempData[TempDataKeys.Error] = message;

        return RedirectToAction(nameof(Index));
    }
}
