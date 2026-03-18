using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MusicShop.Service.Services.Interfaces;

namespace MusicShop.Web.Controllers.Api;

/// <summary>
/// 優惠券 API 控制器（結帳頁面 AJAX 用）
/// </summary>
[ApiController]
[Route("api/coupon")]
[Authorize]
public class CouponApiController : BaseApiController
{
    private readonly ICouponService _couponService;

    public CouponApiController(ICouponService couponService)
    {
        _couponService = couponService;
    }

    /// <summary>
    /// 驗證並計算優惠券折扣（結帳預覽用）
    /// </summary>
    [HttpPost("validate")]
    public async Task<IActionResult> Validate([FromBody] CouponValidateRequest request)
    {
        var userId = GetCurrentUserId();
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var result = await _couponService.ValidateAndCalculateDiscountAsync(
            request.UserCouponId, userId, request.TotalAmount);

        return Ok(new
        {
            result.Success,
            result.Message,
            result.DiscountAmount,
            result.FinalAmount
        });
    }

    /// <summary>
    /// 兌換優惠券
    /// </summary>
    [HttpPost("redeem")]
    public async Task<IActionResult> Redeem([FromBody] CouponRedeemRequest request)
    {
        var userId = GetCurrentUserId();
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var (success, message) = await _couponService.RedeemCouponByCodeAsync(userId, request.Code);

        return Ok(new { success, message });
    }
}

public class CouponValidateRequest
{
    public int UserCouponId { get; set; }
    public decimal TotalAmount { get; set; }
}

public class CouponRedeemRequest
{
    public string Code { get; set; } = string.Empty;
}
