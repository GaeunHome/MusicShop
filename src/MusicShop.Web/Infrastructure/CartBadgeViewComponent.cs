using Microsoft.AspNetCore.Mvc;
using MusicShop.Service.Services.Interfaces;
using System.Security.Claims;

namespace MusicShop.Web.Infrastructure;

/// <summary>
/// 購物車徽章 ViewComponent
/// 動態顯示導覽列購物車商品數量，透過 Service 層查詢資料
/// </summary>
public class CartBadgeViewComponent : ViewComponent
{
    private readonly ICartService _cartService;

    public CartBadgeViewComponent(ICartService cartService)
    {
        _cartService = cartService;
    }

    public async Task<IViewComponentResult> InvokeAsync()
    {
        var cartItemCount = 0;

        // 雙重驗證：先檢查 IsAuthenticated 再檢查 userId 是否有值。
        // Identity?.IsAuthenticated 確認認證 Cookie 有效，但在極端情況下
        //（如使用者帳號已刪除但 Cookie 尚未過期）NameIdentifier Claim 可能為空，
        // 因此需額外檢查 userId 避免傳入空值導致 Service 層查詢錯誤。
        if (HttpContext.User.Identity?.IsAuthenticated == true)
        {
            var userId = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!string.IsNullOrEmpty(userId))
            {
                cartItemCount = await _cartService.GetCartItemCountAsync(userId);
            }
        }

        return View(cartItemCount);
    }
}
