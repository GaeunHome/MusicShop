namespace MusicShop.Library.Helpers;

/// <summary>
/// 顯示相關常數（前端版面配置對應的資料筆數限制）
/// </summary>
public static class DisplayConstants
{
    /// <summary>
    /// 商品詳情頁「相關商品」最多顯示筆數（前端一行 4 張卡片、最多兩行）
    /// </summary>
    public const int RelatedAlbumsCount = 8;

    /// <summary>
    /// 精選藝人區塊每位藝人最多顯示的專輯數
    /// </summary>
    public const int FeaturedArtistAlbumsCount = 4;

    /// <summary>
    /// 訂單列表中商品摘要最多顯示筆數
    /// </summary>
    public const int OrderItemsPreviewCount = 3;

    /// <summary>
    /// 「最近訂單」預設顯示筆數
    /// </summary>
    public const int RecentOrdersDefaultCount = 5;

    /// <summary>
    /// 前台商品列表每頁顯示筆數
    /// </summary>
    public const int AlbumPageSize = 12;

    /// <summary>
    /// 後台藝人列表每頁顯示筆數
    /// </summary>
    public const int AdminArtistPageSize = 10;

    /// <summary>
    /// 後台訂單列表每頁顯示筆數
    /// </summary>
    public const int AdminOrderPageSize = 20;
}
