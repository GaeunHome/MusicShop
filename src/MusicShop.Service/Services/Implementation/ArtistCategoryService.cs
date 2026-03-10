using MusicShop.Data.Entities;
using MusicShop.Data.UnitOfWork;
using MusicShop.Service.Services.Interfaces;
using MusicShop.Library.Helpers;

namespace MusicShop.Service.Services.Implementation;

/// <summary>
/// 藝人分類商業邏輯實作
/// </summary>
public class ArtistCategoryService : IArtistCategoryService
{
    private readonly IUnitOfWork _unitOfWork;

    public ArtistCategoryService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IEnumerable<ArtistCategory>> GetAllArtistCategoriesAsync()
    {
        return await _unitOfWork.ArtistCategories.GetAllAsync();
    }

    public async Task<ArtistCategory?> GetArtistCategoryByIdAsync(int id)
    {
        return await _unitOfWork.ArtistCategories.GetByIdAsync(id);
    }

    public async Task<ArtistCategory> CreateArtistCategoryAsync(ArtistCategory artistCategory)
    {
        // 商業邏輯驗證
        ValidationHelper.ValidateString(artistCategory.Name, "藝人分類名稱", 50, nameof(artistCategory.Name));

        return await _unitOfWork.ArtistCategories.CreateAsync(artistCategory);
    }

    public async Task UpdateArtistCategoryAsync(ArtistCategory artistCategory)
    {
        // 商業邏輯驗證
        ValidationHelper.ValidateString(artistCategory.Name, "藝人分類名稱", 50, nameof(artistCategory.Name));

        var exists = await _unitOfWork.ArtistCategories.GetByIdAsync(artistCategory.Id);
        ValidationHelper.ValidateEntityExists(exists, "藝人分類", artistCategory.Id);

        await _unitOfWork.ArtistCategories.UpdateAsync(artistCategory);
    }

    public async Task DeleteArtistCategoryAsync(int id)
    {
        var exists = await _unitOfWork.ArtistCategories.GetByIdAsync(id);
        ValidationHelper.ValidateEntityExists(exists, "藝人分類", id);

        // 檢查是否有商品使用此分類
        var albums = await _unitOfWork.Albums.GetAlbumsAsync(null, id, null);
        ValidationHelper.ValidateCondition(
            !albums.Any(),
            $"無法刪除「{exists!.Name}」，因為還有 {albums.Count()} 個商品使用此分類"
        );

        await _unitOfWork.ArtistCategories.DeleteAsync(id);
    }
}
