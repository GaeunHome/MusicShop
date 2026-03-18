using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MusicShop.Service.Services.Interfaces;

namespace MusicShop.Web.Controllers.Api;

/// <summary>
/// 購物車 API 控制器
/// 提供 RESTful JSON 端點，供前端 AJAX 呼叫
/// </summary>
[ApiController]
[Route("api/cart")]
[Authorize]
public class CartApiController : BaseApiController
{
    private readonly ICartService _cartService;

    public CartApiController(ICartService cartService)
    {
        _cartService = cartService;
    }

    /// <summary>
    /// 取得購物車商品數量（供導覽列 Badge 即時更新）
    /// GET /api/cart/count
    /// </summary>
    [HttpGet("count")]
    public async Task<IActionResult> GetCount()
    {
        var userId = GetCurrentUserId();
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var count = await _cartService.GetCartItemCountAsync(userId);
        return Ok(new { count });
    }

    /// <summary>
    /// AJAX 加入購物車（不整頁跳轉，回傳 JSON）
    /// POST /api/cart/add
    /// </summary>
    [HttpPost("add")]
    public async Task<IActionResult> Add([FromBody] AddToCartRequest request)
    {
        var userId = GetCurrentUserId();
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        try
        {
            await _cartService.AddToCartAsync(userId, request.AlbumId, request.Quantity);
            var count = await _cartService.GetCartItemCountAsync(userId);
            return Ok(new { success = true, message = "已加入購物車！", cartCount = count });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
    }
}

/// <summary>
/// 加入購物車的請求模型
/// </summary>
public class AddToCartRequest
{
    public int AlbumId { get; set; }
    public int Quantity { get; set; } = 1;
}
