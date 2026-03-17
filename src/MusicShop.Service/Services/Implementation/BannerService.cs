using AutoMapper;
using MusicShop.Data.Entities;
using MusicShop.Data.UnitOfWork;
using MusicShop.Library.Helpers;
using MusicShop.Service.Constants;
using MusicShop.Service.Services.Interfaces;
using MusicShop.Service.ViewModels.Admin;
using MusicShop.Service.ViewModels.Home;

namespace MusicShop.Service.Services.Implementation
{
    /// <summary>
    /// 首頁幻燈片商業邏輯實作
    /// </summary>
    public class BannerService : IBannerService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ICacheService _cacheService;

        public BannerService(IUnitOfWork unitOfWork, IMapper mapper, ICacheService cacheService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _cacheService = cacheService;
        }

        public async Task<IEnumerable<Banner>> GetActiveBannersAsync()
            => await _cacheService.GetOrCreateAsync(
                CacheKeys.Banners,
                () => _unitOfWork.Banners.GetActiveBannersAsync());

        public async Task<IEnumerable<BannerDisplayViewModel>> GetActiveBannerDisplaysAsync()
        {
            var banners = await GetActiveBannersAsync();
            return _mapper.Map<IEnumerable<BannerDisplayViewModel>>(banners);
        }

        public async Task<IEnumerable<BannerListItemViewModel>> GetBannerListItemsAsync()
        {
            var banners = await _unitOfWork.Banners.GetAllOrderedAsync();
            return _mapper.Map<IEnumerable<BannerListItemViewModel>>(banners);
        }

        public async Task<BannerFormViewModel?> GetBannerFormByIdAsync(int id)
        {
            var banner = await _unitOfWork.Banners.GetByIdAsync(id);
            if (banner == null) return null;

            return _mapper.Map<BannerFormViewModel>(banner);
        }

        public async Task<BannerFormViewModel> CreateBannerAsync(BannerFormViewModel vm)
        {
            var banner = _mapper.Map<Banner>(vm);
            banner.ImageUrl ??= string.Empty;

            await _unitOfWork.Banners.AddAsync(banner);
            await _unitOfWork.SaveChangesAsync();

            vm.Id = banner.Id;
            vm.ImageUrl = banner.ImageUrl;

            _cacheService.RemoveByPrefix(CacheKeys.BannersPrefix);
            return vm;
        }

        public async Task UpdateBannerAsync(BannerFormViewModel vm)
        {
            var existing = await _unitOfWork.Banners.GetByIdAsync(vm.Id);
            ValidationHelper.ValidateEntityExists(existing, "幻燈片", vm.Id);

            existing!.AlbumId = vm.AlbumId;
            existing.DisplayOrder = vm.DisplayOrder;
            existing.IsActive = vm.IsActive;
            existing.ImageUrl = vm.ImageUrl ?? existing.ImageUrl;

            await _unitOfWork.Banners.UpdateAsync(existing);
            await _unitOfWork.SaveChangesAsync();

            _cacheService.RemoveByPrefix(CacheKeys.BannersPrefix);
        }

        public async Task UpdateBannerImageUrlAsync(int id, string imageUrl)
        {
            var banner = await _unitOfWork.Banners.GetByIdAsync(id);
            ValidationHelper.ValidateEntityExists(banner, "幻燈片", id);

            banner!.ImageUrl = imageUrl;
            await _unitOfWork.Banners.UpdateAsync(banner);
            await _unitOfWork.SaveChangesAsync();

            _cacheService.RemoveByPrefix(CacheKeys.BannersPrefix);
        }

        public async Task DeleteBannerAsync(int id)
        {
            var banner = await _unitOfWork.Banners.GetByIdAsync(id);
            ValidationHelper.ValidateEntityExists(banner, "幻燈片", id);

            await _unitOfWork.Banners.DeleteAsync(banner!);
            await _unitOfWork.SaveChangesAsync();

            _cacheService.RemoveByPrefix(CacheKeys.BannersPrefix);
        }
    }
}
