using MusicShop.Models;

namespace MusicShop.Repositories.Interface;

/// <summary>
/// 藝人資料存取介面
/// </summary>
public interface IArtistRepository
{
    /// <summary>
    /// 取得所有藝人
    /// </summary>
    Task<IEnumerable<Artist>> GetAllArtistsAsync();

    /// <summary>
    /// 根據藝人分類 ID 取得藝人列表
    /// </summary>
    Task<IEnumerable<Artist>> GetArtistsByCategoryIdAsync(int artistCategoryId);

    /// <summary>
    /// 根據 ID 取得單一藝人（包含關聯資料）
    /// </summary>
    Task<Artist?> GetArtistByIdAsync(int id);

    /// <summary>
    /// 取得藝人分組資料（依藝人分類分組）
    /// </summary>
    Task<Dictionary<ArtistCategory, IEnumerable<Artist>>> GetArtistsGroupedByCategoryAsync();

    /// <summary>
    /// 新增藝人
    /// </summary>
    Task<Artist> AddArtistAsync(Artist artist);

    /// <summary>
    /// 更新藝人
    /// </summary>
    Task UpdateArtistAsync(Artist artist);

    /// <summary>
    /// 刪除藝人
    /// </summary>
    Task DeleteArtistAsync(int id);

    /// <summary>
    /// 檢查藝人是否存在
    /// </summary>
    Task<bool> ArtistExistsAsync(int id);
}
