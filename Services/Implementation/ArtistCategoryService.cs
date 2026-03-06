using MusicShop.Models;
using MusicShop.Repositories.Interface;
using MusicShop.Services.Interface;
using MusicShop.Helpers;

namespace MusicShop.Services.Implementation;

/// <summary>
/// 藝人分類商業邏輯實作
/// </summary>
public class ArtistCategoryService : IArtistCategoryService
{
    private readonly IArtistCategoryRepository _artistCategoryRepository;
    private readonly IAlbumRepository _albumRepository;

    public ArtistCategoryService(
        IArtistCategoryRepository artistCategoryRepository,
        IAlbumRepository albumRepository)
    {
        _artistCategoryRepository = artistCategoryRepository;
        _albumRepository = albumRepository;
    }

    public async Task<IEnumerable<ArtistCategory>> GetAllArtistCategoriesAsync()
    {
        return await _artistCategoryRepository.GetAllAsync();
    }

    public async Task<ArtistCategory?> GetArtistCategoryByIdAsync(int id)
    {
        return await _artistCategoryRepository.GetByIdAsync(id);
    }

    public async Task<ArtistCategory> CreateArtistCategoryAsync(ArtistCategory artistCategory)
    {
        // 商業邏輯驗證
        ValidationHelper.ValidateString(artistCategory.Name, "藝人分類名稱", 50, nameof(artistCategory.Name));

        return await _artistCategoryRepository.CreateAsync(artistCategory);
    }

    public async Task UpdateArtistCategoryAsync(ArtistCategory artistCategory)
    {
        // 商業邏輯驗證
        ValidationHelper.ValidateString(artistCategory.Name, "藝人分類名稱", 50, nameof(artistCategory.Name));

        var exists = await _artistCategoryRepository.GetByIdAsync(artistCategory.Id);
        ValidationHelper.ValidateEntityExists(exists, "藝人分類", artistCategory.Id);

        await _artistCategoryRepository.UpdateAsync(artistCategory);
    }

    public async Task DeleteArtistCategoryAsync(int id)
    {
        var exists = await _artistCategoryRepository.GetByIdAsync(id);
        ValidationHelper.ValidateEntityExists(exists, "藝人分類", id);

        // 檢查是否有商品使用此分類
        var albums = await _albumRepository.GetAlbumsAsync(null, id, null);
        ValidationHelper.ValidateCondition(
            !albums.Any(),
            $"無法刪除「{exists!.Name}」，因為還有 {albums.Count()} 個商品使用此分類"
        );

        await _artistCategoryRepository.DeleteAsync(id);
    }
}
