using MusicShop.Data.Entities;

namespace MusicShop.Service.Services.Interfaces;

/// <summary>
/// 藝人商業邏輯介面
/// </summary>
public interface IArtistService
{
    /// <summary>
    /// 取得所有藝人
    /// </summary>
    Task<IEnumerable<Artist>> GetAllArtistsAsync();

    /// <summary>
    /// 根據藝人分類 ID 取得藝人列表（用於級聯下拉選單）
    /// </summary>
    Task<IEnumerable<Artist>> GetArtistsByCategoryIdAsync(int artistCategoryId);

    /// <summary>
    /// 取得藝人分組資料（依藝人分類分組，用於 Mega Menu 和側邊欄）
    /// </summary>
    Task<Dictionary<ArtistCategory, IEnumerable<Artist>>> GetArtistsGroupedByCategoryAsync();

    /// <summary>
    /// 根據 ID 取得單一藝人
    /// </summary>
    Task<Artist?> GetArtistByIdAsync(int id);

    /// <summary>
    /// 建立藝人
    /// </summary>
    Task<Artist> CreateArtistAsync(Artist artist);

    /// <summary>
    /// 更新藝人
    /// </summary>
    Task UpdateArtistAsync(Artist artist);

    /// <summary>
    /// 刪除藝人
    /// </summary>
    Task DeleteArtistAsync(int id);
}
