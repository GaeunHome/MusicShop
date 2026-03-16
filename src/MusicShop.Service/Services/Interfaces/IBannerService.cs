using MusicShop.Data.Entities;
using MusicShop.Service.ViewModels.Admin;
using MusicShop.Service.ViewModels.Home;

namespace MusicShop.Service.Services.Interfaces
{
    /// <summary>
    /// 首頁幻燈片商業邏輯介面
    /// </summary>
    public interface IBannerService
    {
        /// <summary>
        /// 取得首頁顯示的幻燈片（IsActive=true，依 DisplayOrder 排序）
        /// 【內部使用，返回 Entity】
        /// </summary>
        Task<IEnumerable<Banner>> GetActiveBannersAsync();

        /// <summary>
        /// 取得首頁顯示的幻燈片 ViewModel（供展示層使用）
        /// </summary>
        Task<IEnumerable<BannerDisplayViewModel>> GetActiveBannerDisplaysAsync();

        /// <summary>
        /// 取得後台幻燈片列表（ViewModel）
        /// 【架構說明】服務層負責 Entity → ViewModel 轉換，Controller 與 View 只使用 ViewModel
        /// </summary>
        Task<IEnumerable<BannerListItemViewModel>> GetBannerListItemsAsync();

        /// <summary>
        /// 根據 ID 取得後台幻燈片表單 ViewModel（用於編輯頁面預填資料）
        /// 【架構說明】服務層負責 Entity → ViewModel 轉換，Controller 與 View 只使用 ViewModel
        /// </summary>
        Task<BannerFormViewModel?> GetBannerFormByIdAsync(int id);

        /// <summary>
        /// 新增幻燈片（後台管理用）
        /// 【架構說明】服務層負責 ViewModel → Entity 轉換，Controller 只傳遞 ViewModel
        /// </summary>
        Task<BannerFormViewModel> CreateBannerAsync(BannerFormViewModel vm);

        /// <summary>
        /// 更新幻燈片（後台管理用）
        /// 【架構說明】服務層負責 ViewModel → Entity 轉換，Controller 只傳遞 ViewModel
        /// </summary>
        Task UpdateBannerAsync(BannerFormViewModel vm);

        /// <summary>
        /// 更新幻燈片圖片 URL（圖片上傳後單獨更新）
        /// </summary>
        Task UpdateBannerImageUrlAsync(int id, string imageUrl);

        /// <summary>
        /// 刪除幻燈片
        /// </summary>
        Task DeleteBannerAsync(int id);
    }
}
