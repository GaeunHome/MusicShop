using Microsoft.AspNetCore.Mvc;
using MusicShop.Service.Services.Interfaces;

namespace MusicShop.Web.Controllers.Api;

/// <summary>
/// 專輯 API 控制器
/// 提供搜尋建議等 RESTful JSON 端點，供前端 AJAX 呼叫
/// </summary>
[ApiController]
[Route("api/album")]
public class AlbumApiController : BaseApiController
{
    private readonly IAlbumService _albumService;

    /// <summary>
    /// 搜尋建議最大回傳筆數
    /// </summary>
    private const int MaxSuggestions = 6;

    public AlbumApiController(IAlbumService albumService)
    {
        _albumService = albumService;
    }

    /// <summary>
    /// 搜尋即時建議（使用者輸入時觸發，回傳前幾筆符合的商品）
    /// GET /api/album/suggestions?q=BTS
    /// </summary>
    [HttpGet("suggestions")]
    public async Task<IActionResult> GetSuggestions([FromQuery] string q)
    {
        // 至少輸入 1 個字才觸發搜尋
        if (string.IsNullOrWhiteSpace(q) || q.Trim().Length < 1)
            return Ok(Array.Empty<object>());

        var albums = await _albumService.GetAlbumCardViewModelsAsync(searchTerm: q.Trim());

        // 取前 N 筆，只回傳前端需要的欄位（減少傳輸量）
        var suggestions = albums.Take(MaxSuggestions).Select(a => new
        {
            id = a.Id,
            title = a.Title,
            artistName = a.ArtistName ?? "",
            price = a.FormattedPrice,
            coverImageUrl = a.CoverImageUrl ?? ""
        });

        return Ok(suggestions);
    }
}
