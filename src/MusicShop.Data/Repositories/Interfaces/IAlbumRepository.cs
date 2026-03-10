using MusicShop.Data.Entities;

namespace MusicShop.Data.Repositories.Interfaces
{
    /// <summary>
    /// 專輯資料存取介面
    /// </summary>
    public interface IAlbumRepository
    {
        /// <summary>
        /// 取得所有專輯（含分類資訊）
        /// </summary>
        Task<IEnumerable<Album>> GetAllAlbumsAsync();

        /// <summary>
        /// 根據條件查詢專輯
        /// </summary>
        /// <param name="searchTerm">搜尋關鍵字（標題或演出者）</param>
        /// <param name="artistCategoryId">藝人分類 ID</param>
        /// <param name="artistId">藝人/團體 ID</param>
        /// <param name="productTypeId">商品類型 ID（子分類）</param>
        /// <param name="parentProductTypeId">商品父分類 ID</param>
        /// <param name="sortBy">排序方式</param>
        Task<IEnumerable<Album>> GetAlbumsAsync(
            string? searchTerm = null,
            int? artistCategoryId = null,
            int? artistId = null,
            int? productTypeId = null,
            int? parentProductTypeId = null,
            string? sortBy = null);

        /// <summary>
        /// 根據 ID 取得單一專輯（含分類資訊）
        /// </summary>
        Task<Album?> GetAlbumByIdAsync(int id);

        /// <summary>
        /// 取得最新的 N 個專輯
        /// </summary>
        Task<IEnumerable<Album>> GetLatestAlbumsAsync(int count);

        /// <summary>
        /// 新增專輯
        /// </summary>
        Task<Album> AddAlbumAsync(Album album);

        /// <summary>
        /// 更新專輯
        /// </summary>
        Task UpdateAlbumAsync(Album album);

        /// <summary>
        /// 刪除專輯
        /// </summary>
        Task DeleteAlbumAsync(int id);

        /// <summary>
        /// 檢查專輯是否存在
        /// </summary>
        Task<bool> AlbumExistsAsync(int id);
    }
}
