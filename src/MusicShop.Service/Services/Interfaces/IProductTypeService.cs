using MusicShop.Data.Entities;

namespace MusicShop.Service.Services.Interfaces;

/// <summary>
/// 商品類型商業邏輯介面
/// </summary>
public interface IProductTypeService
{
    /// <summary>
    /// 取得所有商品類型
    /// </summary>
    Task<IEnumerable<ProductType>> GetAllProductTypesAsync();

    /// <summary>
    /// 根據 ID 取得商品類型
    /// </summary>
    Task<ProductType?> GetProductTypeByIdAsync(int id);

    /// <summary>
    /// 新增商品類型
    /// </summary>
    Task<ProductType> CreateProductTypeAsync(ProductType productType);

    /// <summary>
    /// 更新商品類型
    /// </summary>
    Task UpdateProductTypeAsync(ProductType productType);

    /// <summary>
    /// 刪除商品類型
    /// </summary>
    Task DeleteProductTypeAsync(int id);

    /// <summary>
    /// 取得所有父分類（頂層分類）
    /// </summary>
    Task<IEnumerable<ProductType>> GetParentCategoriesAsync();

    /// <summary>
    /// 取得指定父分類下的所有子分類
    /// </summary>
    Task<IEnumerable<ProductType>> GetChildrenByParentIdAsync(int parentId);

    /// <summary>
    /// 根據 ID 取得商品類型（包含子分類）
    /// </summary>
    Task<ProductType?> GetProductTypeWithChildrenAsync(int id);

    /// <summary>
    /// 取得所有子分類（只有實際可用於商品的分類）
    /// </summary>
    Task<IEnumerable<ProductType>> GetAllChildCategoriesAsync();
}
