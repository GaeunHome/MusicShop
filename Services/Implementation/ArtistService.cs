using MusicShop.Models;
using MusicShop.Services.Interface;
using MusicShop.Repositories.Interface;
using MusicShop.Helpers;

namespace MusicShop.Services.Implementation;

/// <summary>
/// 藝人商業邏輯實作
/// </summary>
public class ArtistService : IArtistService
{
    private readonly IArtistRepository _artistRepository;

    public ArtistService(IArtistRepository artistRepository)
    {
        _artistRepository = artistRepository;
    }

    public async Task<IEnumerable<Artist>> GetAllArtistsAsync()
    {
        return await _artistRepository.GetAllArtistsAsync();
    }

    public async Task<Dictionary<ArtistCategory, IEnumerable<Artist>>> GetArtistsGroupedByCategoryAsync()
    {
        return await _artistRepository.GetArtistsGroupedByCategoryAsync();
    }

    public async Task<Artist?> GetArtistByIdAsync(int id)
    {
        return await _artistRepository.GetArtistByIdAsync(id);
    }

    public async Task<Artist> CreateArtistAsync(Artist artist)
    {
        // 商業邏輯驗證
        ValidationHelper.ValidateNotEmpty(artist.Name, "藝人名稱", nameof(artist.Name));
        ValidationHelper.ValidateId(artist.ArtistCategoryId, "藝人分類", nameof(artist.ArtistCategoryId));

        return await _artistRepository.AddArtistAsync(artist);
    }

    public async Task UpdateArtistAsync(Artist artist)
    {
        // 商業邏輯驗證
        ValidationHelper.ValidateNotEmpty(artist.Name, "藝人名稱", nameof(artist.Name));
        ValidationHelper.ValidateId(artist.ArtistCategoryId, "藝人分類", nameof(artist.ArtistCategoryId));

        var exists = await _artistRepository.ArtistExistsAsync(artist.Id);
        ValidationHelper.ValidateCondition(exists, $"找不到 ID 為 {artist.Id} 的藝人");

        await _artistRepository.UpdateArtistAsync(artist);
    }

    public async Task DeleteArtistAsync(int id)
    {
        var exists = await _artistRepository.ArtistExistsAsync(id);
        ValidationHelper.ValidateCondition(exists, $"找不到 ID 為 {id} 的藝人");

        await _artistRepository.DeleteArtistAsync(id);
    }
}
