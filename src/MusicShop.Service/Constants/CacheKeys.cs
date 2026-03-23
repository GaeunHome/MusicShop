namespace MusicShop.Service.Constants;

/// <summary>
/// 快取鍵值常數
/// </summary>
public static class CacheKeys
{
    /// <summary>
    /// 所有藝人分類
    /// </summary>
    public const string ArtistCategories = "categories:artist";

    /// <summary>
    /// 藝人分類下拉選單項目
    /// </summary>
    public const string ArtistCategorySelectItems = "categories:artist:select";

    /// <summary>
    /// 商品類型父分類
    /// </summary>
    public const string ProductTypeParents = "categories:product-types:parents";

    /// <summary>
    /// 商品類型子分類
    /// </summary>
    public const string ProductTypeChildren = "categories:product-types:children";

    /// <summary>
    /// 商品類型導覽樹
    /// </summary>
    public const string ProductTypeNavTree = "categories:product-types:nav-tree";

    /// <summary>
    /// 啟用中的幻燈片
    /// </summary>
    public const string Banners = "banners:active";

    /// <summary>
    /// 啟用中的精選藝人
    /// </summary>
    public const string FeaturedArtists = "featured-artists:active";

    /// <summary>
    /// 精選藝人相關快取前綴（用於批次清除）
    /// </summary>
    public const string FeaturedArtistsPrefix = "featured-artists:";

    /// <summary>
    /// 分類相關快取前綴（用於批次清除）
    /// </summary>
    public const string CategoriesPrefix = "categories:";

    /// <summary>
    /// 幻燈片相關快取前綴（用於批次清除）
    /// </summary>
    public const string BannersPrefix = "banners:";
}
