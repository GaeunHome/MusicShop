using MusicShop.Data.Entities;
using MusicShop.Service.Services.Interfaces;
using MusicShop.Data.UnitOfWork;
using MusicShop.Library.Helpers;
using MusicShop.Service.ViewModels.Admin;

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

    public async Task<IEnumerable<ArtistListItemViewModel>> GetArtistListItemsAsync()
    {
        var artists = await _unitOfWork.Artists.GetAllArtistsAsync();
        return artists.Select(a => new ArtistListItemViewModel
        {
            Id = a.Id,
            Name = a.Name,
            ArtistCategoryName = a.ArtistCategory?.Name,
            DisplayOrder = a.DisplayOrder
        });
    }

    public async Task<ArtistFormViewModel?> GetArtistFormByIdAsync(int id)
    {
        var artist = await _unitOfWork.Artists.GetArtistByIdAsync(id);
        if (artist == null) return null;

        return new ArtistFormViewModel
        {
            Id = artist.Id,
            Name = artist.Name,
            Description = artist.Description,
            ProfileImageUrl = artist.ProfileImageUrl,
            ArtistCategoryId = artist.ArtistCategoryId,
            DisplayOrder = artist.DisplayOrder
        };
    }

    public async Task<ArtistFormViewModel> CreateArtistAsync(ArtistFormViewModel vm)
    {
        // 商業邏輯驗證
        ValidationHelper.ValidateNotEmpty(vm.Name, "藝人名稱", nameof(vm.Name));
        ValidationHelper.ValidateId(vm.ArtistCategoryId, "藝人分類", nameof(vm.ArtistCategoryId));

        var artist = new Artist
        {
            Name = vm.Name,
            Description = vm.Description,
            ProfileImageUrl = vm.ProfileImageUrl,
            ArtistCategoryId = vm.ArtistCategoryId,
            DisplayOrder = vm.DisplayOrder
        };

        var created = await _unitOfWork.Artists.AddArtistAsync(artist);
        vm.Id = created.Id;
        return vm;
    }

    public async Task UpdateArtistAsync(ArtistFormViewModel vm)
    {
        // 商業邏輯驗證
        ValidationHelper.ValidateNotEmpty(vm.Name, "藝人名稱", nameof(vm.Name));
        ValidationHelper.ValidateId(vm.ArtistCategoryId, "藝人分類", nameof(vm.ArtistCategoryId));

        var exists = await _unitOfWork.Artists.ArtistExistsAsync(vm.Id);
        ValidationHelper.ValidateCondition(exists, $"找不到 ID 為 {vm.Id} 的藝人");

        var artist = await _unitOfWork.Artists.GetArtistByIdAsync(vm.Id);
        artist!.Name = vm.Name;
        artist.Description = vm.Description;
        artist.ProfileImageUrl = vm.ProfileImageUrl;
        artist.ArtistCategoryId = vm.ArtistCategoryId;
        artist.DisplayOrder = vm.DisplayOrder;

        await _unitOfWork.Artists.UpdateArtistAsync(artist);
    }

    public async Task DeleteArtistAsync(int id)
    {
        var exists = await _unitOfWork.Artists.ArtistExistsAsync(id);
        ValidationHelper.ValidateCondition(exists, $"找不到 ID 為 {id} 的藝人");

        await _unitOfWork.Artists.DeleteArtistAsync(id);
    }
}
