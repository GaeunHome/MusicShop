using MusicShop.Data.Entities;
using MusicShop.Data.UnitOfWork;
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

        public BannerService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IEnumerable<Banner>> GetActiveBannersAsync()
            => await _unitOfWork.Banners.GetActiveBannersAsync();

        public async Task<IEnumerable<BannerDisplayViewModel>> GetActiveBannerDisplaysAsync()
        {
            var banners = await _unitOfWork.Banners.GetActiveBannersAsync();
            return banners.Select(b => new BannerDisplayViewModel
            {
                Id = b.Id,
                ImageUrl = b.ImageUrl,
                AlbumId = b.AlbumId
            });
        }

        public async Task<IEnumerable<BannerListItemViewModel>> GetBannerListItemsAsync()
        {
            var banners = await _unitOfWork.Banners.GetAllOrderedAsync();
            return banners.Select(b => new BannerListItemViewModel
            {
                Id = b.Id,
                ImageUrl = b.ImageUrl,
                AlbumId = b.AlbumId,
                AlbumTitle = b.Album?.Title,
                DisplayOrder = b.DisplayOrder,
                IsActive = b.IsActive
            });
        }

        public async Task<BannerFormViewModel?> GetBannerFormByIdAsync(int id)
        {
            var banner = await _unitOfWork.Banners.GetByIdAsync(id);
            if (banner == null) return null;

            return new BannerFormViewModel
            {
                Id = banner.Id,
                AlbumId = banner.AlbumId,
                DisplayOrder = banner.DisplayOrder,
                IsActive = banner.IsActive,
                ImageUrl = banner.ImageUrl
            };
        }

        public async Task<BannerFormViewModel> CreateBannerAsync(BannerFormViewModel vm)
        {
            var banner = new Banner
            {
                AlbumId = vm.AlbumId,
                DisplayOrder = vm.DisplayOrder,
                IsActive = vm.IsActive,
                ImageUrl = vm.ImageUrl ?? string.Empty
            };

            await _unitOfWork.Banners.AddAsync(banner);
            await _unitOfWork.SaveChangesAsync();

            vm.Id = banner.Id;
            vm.ImageUrl = banner.ImageUrl;
            return vm;
        }

        public async Task UpdateBannerAsync(BannerFormViewModel vm)
        {
            var existing = await _unitOfWork.Banners.GetByIdAsync(vm.Id);
            if (existing == null) return;

            existing.AlbumId = vm.AlbumId;
            existing.DisplayOrder = vm.DisplayOrder;
            existing.IsActive = vm.IsActive;
            existing.ImageUrl = vm.ImageUrl ?? existing.ImageUrl;

            await _unitOfWork.Banners.UpdateAsync(existing);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task UpdateBannerImageUrlAsync(int id, string imageUrl)
        {
            var banner = await _unitOfWork.Banners.GetByIdAsync(id);
            if (banner == null) return;

            banner.ImageUrl = imageUrl;
            await _unitOfWork.Banners.UpdateAsync(banner);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task DeleteBannerAsync(int id)
        {
            var banner = await _unitOfWork.Banners.GetByIdAsync(id);
            if (banner != null)
            {
                await _unitOfWork.Banners.DeleteAsync(banner);
                await _unitOfWork.SaveChangesAsync();
            }
        }
    }
}
