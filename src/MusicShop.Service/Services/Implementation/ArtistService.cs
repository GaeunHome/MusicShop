using AutoMapper;
using MusicShop.Data.Entities;
using MusicShop.Service.Services.Interfaces;
using MusicShop.Data.UnitOfWork;
using MusicShop.Library.Helpers;
using MusicShop.Service.ViewModels;
using MusicShop.Service.ViewModels.Admin;
using MusicShop.Service.ViewModels.Shared;

namespace MusicShop.Service.Services.Implementation;

/// <summary>
/// 藝人商業邏輯實作
/// </summary>
public class ArtistService : IArtistService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public ArtistService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
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

    public async Task<IEnumerable<SelectItemViewModel>> GetArtistSelectItemsAsync()
    {
        var artists = await _unitOfWork.Artists.GetAllArtistsAsync();
        return artists.Select(a => new SelectItemViewModel
        {
            Id = a.Id,
            Name = a.Name
        });
    }

    public async Task<IEnumerable<SelectItemViewModel>> GetArtistSelectItemsByCategoryIdAsync(int artistCategoryId)
    {
        ValidationHelper.ValidateId(artistCategoryId, "藝人分類 ID", nameof(artistCategoryId));

        var artists = await _unitOfWork.Artists.GetArtistsByCategoryIdAsync(artistCategoryId);
        return artists.Select(a => new SelectItemViewModel
        {
            Id = a.Id,
            Name = a.Name
        });
    }

    public async Task<string?> GetArtistNameByIdAsync(int id)
    {
        var artist = await _unitOfWork.Artists.GetArtistByIdAsync(id);
        return artist?.Name;
    }

    public async Task<int?> GetArtistCategoryIdByArtistIdAsync(int artistId)
    {
        var artist = await _unitOfWork.Artists.GetArtistByIdAsync(artistId);
        return artist?.ArtistCategoryId;
    }

    public async Task<IEnumerable<ArtistListItemViewModel>> GetArtistListItemsAsync()
    {
        var artists = await _unitOfWork.Artists.GetAllArtistsAsync();
        return _mapper.Map<IEnumerable<ArtistListItemViewModel>>(artists);
    }

    public async Task<PagedResult<ArtistListItemViewModel>> GetArtistListItemsPagedAsync(
        int page, int pageSize, int? artistCategoryId = null, bool? isActive = null)
    {
        ValidationHelper.ValidatePositive(page, "頁碼", nameof(page));
        ValidationHelper.ValidatePositive(pageSize, "每頁筆數", nameof(pageSize));

        var (artists, totalCount) = await _unitOfWork.Artists.GetArtistsPagedAsync(
            page, pageSize, artistCategoryId, isActive);

        var viewModels = _mapper.Map<IEnumerable<ArtistListItemViewModel>>(artists);
        return new PagedResult<ArtistListItemViewModel>(viewModels, totalCount, page, pageSize);
    }

    public async Task<ArtistFormViewModel?> GetArtistFormByIdAsync(int id)
    {
        var artist = await _unitOfWork.Artists.GetArtistByIdAsync(id);
        if (artist == null) return null;

        return _mapper.Map<ArtistFormViewModel>(artist);
    }

    public async Task<ArtistFormViewModel> CreateArtistAsync(ArtistFormViewModel vm)
    {
        ValidationHelper.ValidateNotEmpty(vm.Name, "藝人名稱", nameof(vm.Name));
        ValidationHelper.ValidateId(vm.ArtistCategoryId, "藝人分類", nameof(vm.ArtistCategoryId));

        var newArtist = _mapper.Map<Artist>(vm);

        var savedArtist = await _unitOfWork.Artists.AddArtistAsync(newArtist);
        await _unitOfWork.SaveChangesAsync();
        vm.Id = savedArtist.Id;
        return vm;
    }

    public async Task UpdateArtistAsync(ArtistFormViewModel vm)
    {
        ValidationHelper.ValidateNotEmpty(vm.Name, "藝人名稱", nameof(vm.Name));
        ValidationHelper.ValidateId(vm.ArtistCategoryId, "藝人分類", nameof(vm.ArtistCategoryId));

        var artistExists = await _unitOfWork.Artists.ArtistExistsAsync(vm.Id);
        ValidationHelper.ValidateCondition(artistExists, $"找不到 ID 為 {vm.Id} 的藝人");

        var existingArtist = await _unitOfWork.Artists.GetArtistByIdAsync(vm.Id);
        existingArtist!.Name = vm.Name;
        existingArtist.Description = vm.Description;
        existingArtist.ProfileImageUrl = vm.ProfileImageUrl;
        existingArtist.ArtistCategoryId = vm.ArtistCategoryId;
        existingArtist.DisplayOrder = vm.DisplayOrder;
        existingArtist.IsActive = vm.IsActive;

        await _unitOfWork.Artists.UpdateArtistAsync(existingArtist);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task ToggleArtistActiveAsync(int id)
    {
        var artist = await _unitOfWork.Artists.GetArtistByIdAsync(id);
        ValidationHelper.ValidateEntityExists(artist, "藝人", id);

        artist!.IsActive = !artist.IsActive;
        await _unitOfWork.Artists.UpdateArtistAsync(artist);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task<int> GetMaxDisplayOrderAsync()
    {
        return await _unitOfWork.Artists.GetMaxDisplayOrderAsync();
    }

    public async Task DeleteArtistAsync(int id)
    {
        var exists = await _unitOfWork.Artists.ArtistExistsAsync(id);
        ValidationHelper.ValidateCondition(exists, $"找不到 ID 為 {id} 的藝人");

        await _unitOfWork.Artists.DeleteArtistAsync(id);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task<List<NavArtistGroupViewModel>> GetNavArtistGroupsAsync()
    {
        var grouped = await _unitOfWork.Artists.GetArtistsGroupedByCategoryAsync();

        return grouped.Select(g => new NavArtistGroupViewModel
        {
            CategoryId = g.Key.Id,
            CategoryName = g.Key.Name,
            DisplayOrder = g.Key.DisplayOrder,
            Artists = _mapper.Map<List<NavArtistItemViewModel>>(g.Value)
        }).OrderBy(g => g.DisplayOrder).ToList();
    }
}
