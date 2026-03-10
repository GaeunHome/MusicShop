using MusicShop.Data.Entities;
using MusicShop.Service.Services.Interfaces;
using MusicShop.Data.UnitOfWork;
using MusicShop.Library.Helpers;

namespace MusicShop.Service.Services.Implementation;

/// <summary>
/// 藝人商業邏輯實作
/// </summary>
public class ArtistService : IArtistService
{
    private readonly IUnitOfWork _unitOfWork;

    public ArtistService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IEnumerable<Artist>> GetAllArtistsAsync()
    {
        return await _unitOfWork.Artists.GetAllArtistsAsync();
    }

    public async Task<IEnumerable<Artist>> GetArtistsByCategoryIdAsync(int artistCategoryId)
    {
        ValidationHelper.ValidateId(artistCategoryId, "藝人分類 ID", nameof(artistCategoryId));
        return await _unitOfWork.Artists.GetArtistsByCategoryIdAsync(artistCategoryId);
    }

    public async Task<Dictionary<ArtistCategory, IEnumerable<Artist>>> GetArtistsGroupedByCategoryAsync()
    {
        return await _unitOfWork.Artists.GetArtistsGroupedByCategoryAsync();
    }

    public async Task<Artist?> GetArtistByIdAsync(int id)
    {
        return await _unitOfWork.Artists.GetArtistByIdAsync(id);
    }

    public async Task<Artist> CreateArtistAsync(Artist artist)
    {
        // 商業邏輯驗證
        ValidationHelper.ValidateNotEmpty(artist.Name, "藝人名稱", nameof(artist.Name));
        ValidationHelper.ValidateId(artist.ArtistCategoryId, "藝人分類", nameof(artist.ArtistCategoryId));

        return await _unitOfWork.Artists.AddArtistAsync(artist);
    }

    public async Task UpdateArtistAsync(Artist artist)
    {
        // 商業邏輯驗證
        ValidationHelper.ValidateNotEmpty(artist.Name, "藝人名稱", nameof(artist.Name));
        ValidationHelper.ValidateId(artist.ArtistCategoryId, "藝人分類", nameof(artist.ArtistCategoryId));

        var exists = await _unitOfWork.Artists.ArtistExistsAsync(artist.Id);
        ValidationHelper.ValidateCondition(exists, $"找不到 ID 為 {artist.Id} 的藝人");

        await _unitOfWork.Artists.UpdateArtistAsync(artist);
    }

    public async Task DeleteArtistAsync(int id)
    {
        var exists = await _unitOfWork.Artists.ArtistExistsAsync(id);
        ValidationHelper.ValidateCondition(exists, $"找不到 ID 為 {id} 的藝人");

        await _unitOfWork.Artists.DeleteArtistAsync(id);
    }
}
