using MusicShop.Data.Entities;

namespace MusicShop.Data.Repositories.Interfaces;

/// <summary>
/// 藝人分類資料存取介面
/// </summary>
public interface IArtistCategoryRepository
{
    /// <summary>
    /// 取得所有藝人分類（依排序順序）
    /// </summary>
    Task<IEnumerable<ArtistCategory>> GetAllAsync();

    /// <summary>
    /// 根據 ID 取得藝人分類
    /// </summary>
    Task<ArtistCategory?> GetByIdAsync(int id);

    /// <summary>
    /// 新增藝人分類
    /// </summary>
    Task<ArtistCategory> CreateAsync(ArtistCategory artistCategory);

    /// <summary>
    /// 更新藝人分類
    /// </summary>
    Task UpdateAsync(ArtistCategory artistCategory);

    /// <summary>
    /// 刪除藝人分類
    /// </summary>
    Task DeleteAsync(int id);
}
