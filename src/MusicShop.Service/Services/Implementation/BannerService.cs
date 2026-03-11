using MusicShop.Data.Entities;
using MusicShop.Data.UnitOfWork;
using MusicShop.Service.Services.Interfaces;

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

        public async Task<IEnumerable<Banner>> GetAllBannersAsync()
            => await _unitOfWork.Banners.GetAllOrderedAsync();

        public async Task<Banner?> GetBannerByIdAsync(int id)
            => await _unitOfWork.Banners.GetByIdAsync(id);

        public async Task<Banner> CreateBannerAsync(Banner banner)
        {
            await _unitOfWork.Banners.AddAsync(banner);
            await _unitOfWork.SaveChangesAsync();
            return banner;
        }

        public async Task UpdateBannerAsync(Banner banner)
        {
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
