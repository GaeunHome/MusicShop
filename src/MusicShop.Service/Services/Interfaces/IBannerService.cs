using MusicShop.Data.Entities;

namespace MusicShop.Service.Services.Interfaces
{
    /// <summary>
    /// 首頁幻燈片商業邏輯介面
    /// </summary>
    public interface IBannerService
    {
        /// <summary>
        /// 取得首頁顯示的幻燈片（IsActive=true，依 DisplayOrder 排序）
        /// </summary>
        Task<IEnumerable<Banner>> GetActiveBannersAsync();

        /// <summary>
        /// 取得所有幻燈片（後台管理用）
        /// </summary>
        Task<IEnumerable<Banner>> GetAllBannersAsync();

        /// <summary>
        /// 根據 ID 取得單一幻燈片
        /// </summary>
        Task<Banner?> GetBannerByIdAsync(int id);

        /// <summary>
        /// 新增幻燈片
        /// </summary>
        Task<Banner> CreateBannerAsync(Banner banner);

        /// <summary>
        /// 更新幻燈片
        /// </summary>
        Task UpdateBannerAsync(Banner banner);

        /// <summary>
        /// 刪除幻燈片
        /// </summary>
        Task DeleteBannerAsync(int id);
    }
}
