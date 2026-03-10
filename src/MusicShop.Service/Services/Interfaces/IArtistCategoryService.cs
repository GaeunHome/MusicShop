using MusicShop.Data.Entities;

namespace MusicShop.Service.Services.Interfaces;

/// <summary>
/// 藝人分類商業邏輯介面
/// </summary>
public interface IArtistCategoryService
{
    /// <summary>
    /// 取得所有藝人分類
    /// </summary>
    Task<IEnumerable<ArtistCategory>> GetAllArtistCategoriesAsync();

    /// <summary>
    /// 根據 ID 取得藝人分類
    /// </summary>
    Task<ArtistCategory?> GetArtistCategoryByIdAsync(int id);

    /// <summary>
    /// 新增藝人分類
    /// </summary>
    Task<ArtistCategory> CreateArtistCategoryAsync(ArtistCategory artistCategory);

    /// <summary>
    /// 更新藝人分類
    /// </summary>
    Task UpdateArtistCategoryAsync(ArtistCategory artistCategory);

    /// <summary>
    /// 刪除藝人分類
    /// </summary>
    Task DeleteArtistCategoryAsync(int id);
}
