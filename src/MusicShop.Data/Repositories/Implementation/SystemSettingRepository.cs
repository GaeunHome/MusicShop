using Microsoft.EntityFrameworkCore;
using MusicShop.Data.Entities;
using MusicShop.Data.Repositories.Interfaces;

namespace MusicShop.Data.Repositories.Implementation
{
    /// <summary>
    /// 系統參數資料存取實作
    /// </summary>
    public class SystemSettingRepository : GenericRepository<SystemSetting>, ISystemSettingRepository
    {
        public SystemSettingRepository(ApplicationDbContext context) : base(context) { }

        public async Task<SystemSetting?> GetByKeyAsync(string key)
        {
            return await _context.SystemSettings
                .FirstOrDefaultAsync(s => s.Key == key);
        }

        public async Task<IEnumerable<SystemSetting>> GetByGroupAsync(string group)
        {
            return await _context.SystemSettings
                .AsNoTracking()
                .Where(s => s.Group == group)
                .OrderBy(s => s.Key)
                .ToListAsync();
        }

        public async Task<IEnumerable<SystemSetting>> GetAllOrderedAsync()
        {
            return await _context.SystemSettings
                .AsNoTracking()
                .OrderBy(s => s.Group)
                .ThenBy(s => s.Key)
                .ToListAsync();
        }
    }
}
