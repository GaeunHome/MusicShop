namespace MusicShop.Service.ViewModels.Shared;

/// <summary>
/// 導覽列分類項目 ViewModel（_Layout.cshtml Mega Menu 使用）
/// 取代直接在 _Layout 中使用 Data 層 ProductType Entity
/// </summary>
public class NavCategoryItemViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int DisplayOrder { get; set; }
    public List<NavCategoryItemViewModel> Children { get; set; } = new();
}

/// <summary>
/// 導覽列藝人分組 ViewModel（_Layout.cshtml K-ARTIST Mega Menu 使用）
/// 取代直接使用 Dictionary&lt;ArtistCategory, IEnumerable&lt;Artist&gt;&gt;
/// </summary>
public class NavArtistGroupViewModel
{
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public int DisplayOrder { get; set; }
    public List<NavArtistItemViewModel> Artists { get; set; } = new();
}

/// <summary>
/// 導覽列藝人項目 ViewModel
/// </summary>
public class NavArtistItemViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
}
