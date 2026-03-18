using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MusicShop.Service.Services.Interfaces;

namespace MusicShop.Web.Controllers.Api;

/// <summary>
/// 收藏清單 API 控制器
/// 提供 RESTful JSON 端點，供前端 AJAX 呼叫（取代原本 WishlistController.Toggle 的表單提交）
/// </summary>
[ApiController]
[Route("api/wishlist")]
[Authorize]
public class WishlistApiController : BaseApiController
{
    private readonly IWishlistService _wishlistService;

    public WishlistApiController(IWishlistService wishlistService)
    {
        _wishlistService = wishlistService;
    }

    /// <summary>
    /// 切換收藏狀態（加入 or 移除）
    /// POST /api/wishlist/toggle
    /// </summary>
    [HttpPost("toggle")]
    public async Task<IActionResult> Toggle([FromBody] WishlistToggleRequest request)
    {
        var userId = GetCurrentUserId();
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        try
        {
            var added = await _wishlistService.ToggleWishlistAsync(userId, request.AlbumId);
            return Ok(new
            {
                success = true,
                added,
                message = added ? "已加入收藏" : "已取消收藏"
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
    }

    /// <summary>
    /// 檢查某商品是否已收藏
    /// GET /api/wishlist/check/5
    /// </summary>
    [HttpGet("check/{albumId}")]
    public async Task<IActionResult> Check(int albumId)
    {
        var userId = GetCurrentUserId();
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var wishlistIds = await _wishlistService.GetWishlistAlbumIdsAsync(userId);
        var isWishlisted = wishlistIds.Contains(albumId);
        return Ok(new { albumId, isWishlisted });
    }

    /// <summary>
    /// 取得收藏數量
    /// GET /api/wishlist/count
    /// </summary>
    [HttpGet("count")]
    public async Task<IActionResult> GetCount()
    {
        var userId = GetCurrentUserId();
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var wishlistIds = await _wishlistService.GetWishlistAlbumIdsAsync(userId);
        return Ok(new { count = wishlistIds.Count });
    }
}

/// <summary>
/// 收藏切換的請求模型
/// </summary>
public class WishlistToggleRequest
{
    public int AlbumId { get; set; }
}
