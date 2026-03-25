using MusicShop.Data.Entities;

namespace MusicShop.Data.Repositories.Interfaces
{
    /// <summary>
    /// 系統參數資料存取介面
    /// </summary>
    public interface ISystemSettingRepository : IGenericRepository<SystemSetting>
    {
        /// <summary>
        /// 依 Key 取得系統參數
        /// </summary>
        Task<SystemSetting?> GetByKeyAsync(string key);

        /// <summary>
        /// 依分組取得系統參數
        /// </summary>
        Task<IEnumerable<SystemSetting>> GetByGroupAsync(string group);

        /// <summary>
        /// 取得所有系統參數（依 Group、Key 排序）
        /// </summary>
        Task<IEnumerable<SystemSetting>> GetAllOrderedAsync();
    }
}
