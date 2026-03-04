using MusicShop.Models;

namespace MusicShop.Repositories.Interface;

/// <summary>
/// 商品類型資料存取介面
/// </summary>
public interface IProductTypeRepository
{
    /// <summary>
    /// 取得所有商品類型（依排序順序）
    /// </summary>
    Task<IEnumerable<ProductType>> GetAllAsync();

    /// <summary>
    /// 根據 ID 取得商品類型
    /// </summary>
    Task<ProductType?> GetByIdAsync(int id);

    /// <summary>
    /// 新增商品類型
    /// </summary>
    Task<ProductType> CreateAsync(ProductType productType);

    /// <summary>
    /// 更新商品類型
    /// </summary>
    Task UpdateAsync(ProductType productType);

    /// <summary>
    /// 刪除商品類型
    /// </summary>
    Task DeleteAsync(int id);

    /// <summary>
    /// 取得所有父分類（頂層分類，如 K-ALBUM、K-MAGAZINE 等）
    /// </summary>
    Task<IEnumerable<ProductType>> GetParentCategoriesAsync();

    /// <summary>
    /// 取得指定父分類下的所有子分類
    /// </summary>
    Task<IEnumerable<ProductType>> GetChildrenByParentIdAsync(int parentId);

    /// <summary>
    /// 根據 ID 取得商品類型（包含子分類）
    /// </summary>
    Task<ProductType?> GetByIdWithChildrenAsync(int id);

    /// <summary>
    /// 根據 ID 取得商品類型（包含父分類）
    /// </summary>
    Task<ProductType?> GetByIdWithParentAsync(int id);

    /// <summary>
    /// 取得所有子分類（只有實際可用於商品的分類）
    /// </summary>
    Task<IEnumerable<ProductType>> GetAllChildCategoriesAsync();
}
