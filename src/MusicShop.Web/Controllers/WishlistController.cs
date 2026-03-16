using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MusicShop.Data.Entities;
using MusicShop.Service.Services.Interfaces;

namespace MusicShop.Web.Controllers
{
    /// <summary>
    /// 收藏清單控制器
    /// </summary>
    [Authorize]
    public class WishlistController : Controller
    {
        private readonly IWishlistService _wishlistService;
        private readonly UserManager<AppUser> _userManager;

        public WishlistController(IWishlistService wishlistService, UserManager<AppUser> userManager)
        {
            _wishlistService = wishlistService;
            _userManager = userManager;
        }

        // GET: /Wishlist
        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User)!;
            var items = await _wishlistService.GetWishlistItemViewModelsAsync(userId);
            return View(items);
        }

        // POST: /Wishlist/Toggle
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Toggle(int albumId)
        {
            var userId = _userManager.GetUserId(User)!;
            var added = await _wishlistService.ToggleWishlistAsync(userId, albumId);
            return Json(new { added, message = added ? "已加入收藏" : "已取消收藏" });
        }
    }
}
