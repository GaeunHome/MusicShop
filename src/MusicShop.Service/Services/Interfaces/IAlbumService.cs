using MusicShop.Data.Entities;
using MusicShop.Service.ViewModels.Admin;
using MusicShop.Service.ViewModels.Album;

namespace MusicShop.Service.Services.Interfaces
{
    /// <summary>
    /// 專輯商業邏輯介面
    /// </summary>
    public interface IAlbumService
    {
        /// <summary>
        /// 取得專輯列表（支援搜尋與多重篩選）
        /// 【內部使用，返回 Entity，供 Service 層使用】
        /// </summary>
        Task<IEnumerable<Album>> GetAlbumsAsync(
            string? searchTerm = null,
            int? artistCategoryId = null,
            int? artistId = null,
            int? productTypeId = null,
            int? parentProductTypeId = null,
            string? sortBy = null);

        /// <summary>
        /// 取得專輯列表 ViewModel（支援搜尋與多重篩選）
        /// 【公開前台使用，返回 ViewModel，供 AlbumController 使用】
        /// </summary>
        Task<IEnumerable<AlbumCardViewModel>> GetAlbumCardViewModelsAsync(
            string? searchTerm = null,
            int? artistCategoryId = null,
            int? artistId = null,
            int? productTypeId = null,
            int? parentProductTypeId = null,
            string? sortBy = null);

        /// <summary>
        /// 取得專輯詳細資訊（Entity，供 Service 層內部使用）
        /// </summary>
        Task<Album?> GetAlbumDetailAsync(int id);

        /// <summary>
        /// 取得專輯詳情 ViewModel（供展示層使用）
        /// 【公開前台使用，返回 ViewModel，供 AlbumController 使用】
        /// </summary>
        Task<AlbumDetailViewModel?> GetAlbumDetailViewModelAsync(int id);

        /// <summary>
        /// 取得最新上架的專輯 ViewModel
        /// 【公開前台使用，返回 ViewModel，供 HomeController 使用】
        /// </summary>
        Task<IEnumerable<AlbumCardViewModel>> GetLatestAlbumCardsAsync(int count);

        /// <summary>
        /// 取得最新上架的專輯（Entity，內部使用）
        /// </summary>
        Task<IEnumerable<Album>> GetLatestAlbumsAsync(int count);

        /// <summary>
        /// 檢查專輯庫存是否充足
        /// </summary>
        Task<bool> IsStockAvailableAsync(int albumId, int quantity = 1);

        /// <summary>
        /// 取得後台商品列表（ViewModel）
        /// 【架構說明】服務層負責 Entity → ViewModel 轉換，Controller 與 View 只使用 ViewModel
        /// </summary>
        Task<IEnumerable<AlbumListItemViewModel>> GetAlbumListItemsAsync();

        /// <summary>
        /// 根據 ID 取得後台商品表單 ViewModel（用於編輯頁面預填資料）
        /// 【架構說明】服務層負責 Entity → ViewModel 轉換，Controller 與 View 只使用 ViewModel
        /// </summary>
        Task<AlbumFormViewModel?> GetAlbumFormByIdAsync(int id);

        /// <summary>
        /// 新增專輯（後台管理用）
        /// 【架構說明】服務層負責 ViewModel → Entity 轉換，Controller 只傳遞 ViewModel
        /// </summary>
        Task<AlbumFormViewModel> CreateAlbumAsync(AlbumFormViewModel vm);

        /// <summary>
        /// 更新專輯（後台管理用）
        /// 【架構說明】服務層負責 ViewModel → Entity 轉換，Controller 只傳遞 ViewModel
        /// </summary>
        Task UpdateAlbumAsync(AlbumFormViewModel vm);

        /// <summary>
        /// 刪除專輯（後台管理用）
        /// </summary>
        Task DeleteAlbumAsync(int id);
    }
}
