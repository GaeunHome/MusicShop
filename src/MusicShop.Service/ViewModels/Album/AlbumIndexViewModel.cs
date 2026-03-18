using MusicShop.Library.Helpers;
using MusicShop.Service.ViewModels.Shared;

namespace MusicShop.Service.ViewModels.Album;

/// <summary>
/// 專輯列表頁 ViewModel（整合分頁、篩選、排序資訊）
/// </summary>
public class AlbumIndexViewModel
{
    // ===== 商品資料 =====
    public PagedResult<AlbumCardViewModel> PagedResult { get; set; } = null!;

    // ===== 篩選條件 =====
    public string? Search { get; set; }
    public int? ArtistCategoryId { get; set; }
    public int? ArtistId { get; set; }
    public int? ProductTypeId { get; set; }
    public int? ParentProductTypeId { get; set; }
    public string? SortBy { get; set; }

    // ===== 篩選顯示名稱 =====
    public string? ParentCategoryName { get; set; }
    public string? SelectedArtistName { get; set; }

    // ===== 下拉選單資料 =====
    public IEnumerable<SelectItemViewModel> ArtistCategories { get; set; } = [];
    public IEnumerable<SelectItemViewModel> ChildCategories { get; set; } = [];

    // ===== 收藏清單 =====
    public HashSet<int> WishlistIds { get; set; } = [];

    // ===== 排序標籤（供 View 直接使用，避免 View 寫 switch 邏輯）=====

    /// <summary>
    /// 排序下拉選單顯示文字（預設排序 / 最新上架 / 最舊上架 / 本週熱賣）
    /// </summary>
    public string SortLabel => SortBy switch
    {
        "date-new-old" => "最新上架",
        "date-old-new" => "最舊上架",
        "weekly-hot" => "本週熱賣",
        _ => "預設排序"
    };

    /// <summary>
    /// 價格排序下拉選單顯示文字
    /// </summary>
    public string PriceLabel => SortBy switch
    {
        "price-low-high" => "價格由低到高",
        "price-high-low" => "價格由高到低",
        _ => "價格"
    };

    /// <summary>
    /// 是否有指定排序（用於決定平鋪 vs 分組顯示）
    /// </summary>
    public bool IsSorted => !string.IsNullOrEmpty(SortBy);
}
