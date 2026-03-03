using MusicShop.Models;

namespace MusicShop.Services.Interface
{
    /// <summary>
    /// 專輯商業邏輯介面
    /// </summary>
    public interface IAlbumService
    {
        /// <summary>
        /// 取得專輯列表（支援搜尋與分類篩選）
        /// </summary>
        Task<IEnumerable<Album>> GetAlbumsAsync(string? searchTerm = null, int? categoryId = null);

        /// <summary>
        /// 取得專輯詳細資訊
        /// </summary>
        Task<Album?> GetAlbumDetailAsync(int id);

        /// <summary>
        /// 取得最新上架的專輯
        /// </summary>
        Task<IEnumerable<Album>> GetLatestAlbumsAsync(int count);

        /// <summary>
        /// 檢查專輯庫存是否充足
        /// </summary>
        Task<bool> IsStockAvailableAsync(int albumId, int quantity = 1);

        /// <summary>
        /// 新增專輯（後台管理用）
        /// </summary>
        Task<Album> CreateAlbumAsync(Album album);

        /// <summary>
        /// 更新專輯（後台管理用）
        /// </summary>
        Task UpdateAlbumAsync(Album album);

        /// <summary>
        /// 刪除專輯（後台管理用）
        /// </summary>
        Task DeleteAlbumAsync(int id);
    }
}
