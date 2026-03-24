using Microsoft.EntityFrameworkCore;
using MusicShop.Data.Entities;
using MusicShop.Data.Repositories.Interfaces;

namespace MusicShop.Data.Repositories.Implementation
{
    /// <summary>
    /// 幻燈片橫幅資料存取實作
    /// </summary>
    public class BannerRepository : GenericRepository<Banner>, IBannerRepository
    {
        public BannerRepository(ApplicationDbContext context) : base(context) { }

        public async Task<IEnumerable<Banner>> GetActiveBannersAsync()
        {
            return await _context.Banners
                .AsNoTracking()
                .Where(b => b.IsActive)
                .Include(b => b.Album)
                .OrderBy(b => b.DisplayOrder)
                .ToListAsync();
        }

        public async Task<IEnumerable<Banner>> GetAllOrderedAsync()
        {
            return await _context.Banners
                .AsNoTracking()
                .Include(b => b.Album)
                .OrderBy(b => b.DisplayOrder)
                .ToListAsync();
        }
    }
}
