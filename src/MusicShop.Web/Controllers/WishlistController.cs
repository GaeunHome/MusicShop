using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MusicShop.Controllers;
using MusicShop.Service.Services.Interfaces;

namespace MusicShop.Web.Controllers
{
    /// <summary>
    /// 收藏清單控制器
    /// </summary>
    [Authorize]
    public class WishlistController : BaseController
    {
        private readonly IWishlistService _wishlistService;

        public WishlistController(IWishlistService wishlistService)
        {
            _wishlistService = wishlistService;
        }

        // GET: /Wishlist
        public async Task<IActionResult> Index()
        {
            var userId = GetAuthorizedUserId();
            var items = await _wishlistService.GetWishlistItemViewModelsAsync(userId);
            return View(items);
        }

        // POST: /Wishlist/Toggle
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Toggle(int albumId)
        {
            var userId = GetAuthorizedUserId();
            var added = await _wishlistService.ToggleWishlistAsync(userId, albumId);
            return Json(new { added, message = added ? "已加入收藏" : "已取消收藏" });
        }
    }
}
