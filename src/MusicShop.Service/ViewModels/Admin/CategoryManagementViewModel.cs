namespace MusicShop.Service.ViewModels.Admin;

/// <summary>
/// 後台分類管理 ViewModel（Admin/Category/Index.cshtml 使用）
/// 取代直接在 View 中使用 Data 層 ArtistCategory / ProductType Entity
/// </summary>
public class CategoryManagementViewModel
{
    public List<ArtistCategoryListItemViewModel> ArtistCategories { get; set; } = new();
    public List<ProductTypeCategoryTreeViewModel> CategoryTree { get; set; } = new();
}

/// <summary>
/// 藝人分類列表項目 ViewModel
/// </summary>
public class ArtistCategoryListItemViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int DisplayOrder { get; set; }
}

/// <summary>
/// 商品類型父分類（含子分類樹）ViewModel
/// </summary>
public class ProductTypeCategoryTreeViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int DisplayOrder { get; set; }
    public List<ProductTypeChildItemViewModel> Children { get; set; } = new();
}

/// <summary>
/// 商品類型子分類 ViewModel
/// </summary>
public class ProductTypeChildItemViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int DisplayOrder { get; set; }
}
