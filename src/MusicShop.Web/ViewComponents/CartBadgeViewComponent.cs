using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MusicShop.Data.Entities;
using MusicShop.Service.Services.Interfaces;

namespace MusicShop.ViewComponents;

/// <summary>
/// 購物車徽章 ViewComponent
/// 符合三層式架構：ViewComponent (展示層) → Service (商業邏輯層) → Repository (資料存取層)
/// </summary>
public class CartBadgeViewComponent : ViewComponent
{
    private readonly ICartService _cartService;
    private readonly UserManager<AppUser> _userManager;

    public CartBadgeViewComponent(
        ICartService cartService,
        UserManager<AppUser> userManager)
    {
        _cartService = cartService;
        _userManager = userManager;
    }

    public async Task<IViewComponentResult> InvokeAsync()
    {
        // 預設數量為 0
        var cartItemCount = 0;

        // 如果使用者已登入，查詢購物車數量
        if (HttpContext.User.Identity?.IsAuthenticated == true)
        {
            var userId = _userManager.GetUserId(HttpContext.User);
            if (!string.IsNullOrEmpty(userId))
            {
                cartItemCount = await _cartService.GetCartItemCountAsync(userId);
            }
        }

        // 傳遞數量到 View
        return View(cartItemCount);
    }
}
