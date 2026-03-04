namespace MusicShop.Repositories.Interface;

/// <summary>
/// 統計資料存取介面
/// </summary>
public interface IStatisticsRepository
{
    /// <summary>
    /// 取得專輯總數
    /// </summary>
    Task<int> GetAlbumCountAsync();

    /// <summary>
    /// 取得分類總數（藝人分類 + 商品類型）
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
}
