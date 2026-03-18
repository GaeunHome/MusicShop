namespace MusicShop.Service.Services.Interfaces
{
    /// <summary>
    /// 統計資訊服務介面
    /// </summary>
    public interface IStatisticsService
    {
        /// <summary>
        /// 取得專輯總數
        /// </summary>
        Task<int> GetAlbumCountAsync();

        /// <summary>
        /// 取得分類總數
        /// </summary>
        Task<int> GetCategoryCountAsync();

        /// <summary>
        /// 取得訂單總數
        /// </summary>
        Task<int> GetOrderCountAsync();

        /// <summary>
        /// 取得使用者總數
        /// </summary>
        Task<int> GetUserCountAsync();

        /// <summary>
        /// 取得總銷售額
        /// </summary>
        Task<decimal> GetTotalSalesAsync();

        /// <summary>
        /// 取得待處理訂單數量
        /// </summary>
        Task<int> GetPendingOrderCountAsync();

        /// <summary>
        /// 取得藝人總數
        /// </summary>
        Task<int> GetArtistCountAsync();

        /// <summary>
        /// 取得幻燈片總數
        /// </summary>
        Task<int> GetBannerCountAsync();

        /// <summary>
        /// 取得精選藝人總數
        /// </summary>
        Task<int> GetFeaturedArtistCountAsync();

        /// <summary>
        /// 取得優惠券總數
        /// </summary>
        Task<int> GetCouponCountAsync();
    }
}
