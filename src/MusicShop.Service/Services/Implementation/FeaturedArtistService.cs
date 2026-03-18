using AutoMapper;
using MusicShop.Data.Entities;
using MusicShop.Data.UnitOfWork;
using MusicShop.Library.Helpers;
using MusicShop.Service.Services.Interfaces;
using MusicShop.Service.ViewModels.Admin;
using MusicShop.Service.ViewModels.Album;
using MusicShop.Service.ViewModels.Home;

namespace MusicShop.Service.Services.Implementation;

/// <summary>
/// 精選藝人商業邏輯實作
/// </summary>
public class FeaturedArtistService : IFeaturedArtistService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public FeaturedArtistService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<IEnumerable<FeaturedArtistDisplayViewModel>> GetActiveFeaturedArtistDisplaysAsync()
    {
        var featuredArtists = await _unitOfWork.FeaturedArtists.GetActiveFeaturedArtistsAsync();

        return featuredArtists.Select(fa => new FeaturedArtistDisplayViewModel
        {
            ArtistId = fa.ArtistId,
            ArtistName = fa.Artist.Name,
            ProfileImageUrl = fa.Artist.ProfileImageUrl,
            Albums = _mapper.Map<List<AlbumCardViewModel>>(
                fa.Artist.Albums.OrderByDescending(a => a.CreatedAt).Take(DisplayConstants.FeaturedArtistAlbumsCount).ToList())
        });
    }

    public async Task<IEnumerable<FeaturedArtistListItemViewModel>> GetFeaturedArtistListItemsAsync()
    {
        var featuredArtists = await _unitOfWork.FeaturedArtists.GetAllOrderedAsync();
        return _mapper.Map<IEnumerable<FeaturedArtistListItemViewModel>>(featuredArtists);
    }

    public async Task<FeaturedArtistFormViewModel?> GetFeaturedArtistFormByIdAsync(int id)
    {
        var entity = await _unitOfWork.FeaturedArtists.GetByIdAsync(id);
        if (entity == null) return null;
        return _mapper.Map<FeaturedArtistFormViewModel>(entity);
    }

    public async Task CreateFeaturedArtistAsync(FeaturedArtistFormViewModel vm)
    {
        ValidationHelper.ValidateId(vm.ArtistId, "藝人", nameof(vm.ArtistId));

        var entity = new FeaturedArtist
        {
            ArtistId = vm.ArtistId,
            DisplayOrder = vm.DisplayOrder,
            IsActive = vm.IsActive
        };

        await _unitOfWork.FeaturedArtists.AddAsync(entity);
        await _unitOfWork.SaveChangesAsync();

        vm.Id = entity.Id;
    }

    public async Task UpdateFeaturedArtistAsync(FeaturedArtistFormViewModel vm)
    {
        var existing = await _unitOfWork.FeaturedArtists.GetByIdAsync(vm.Id);
        ValidationHelper.ValidateEntityExists(existing, "精選藝人", vm.Id);

        existing!.ArtistId = vm.ArtistId;
        existing.DisplayOrder = vm.DisplayOrder;
        existing.IsActive = vm.IsActive;

        await _unitOfWork.FeaturedArtists.UpdateAsync(existing);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task DeleteFeaturedArtistAsync(int id)
    {
        var entity = await _unitOfWork.FeaturedArtists.GetByIdAsync(id);
        ValidationHelper.ValidateEntityExists(entity, "精選藝人", id);

        await _unitOfWork.FeaturedArtists.DeleteAsync(entity!);
        await _unitOfWork.SaveChangesAsync();
    }
}
