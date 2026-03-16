using MusicShop.Data.Entities;
using MusicShop.Service.ViewModels;
using MusicShop.Service.ViewModels.Admin;

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
    /// 根據 ID 取得商品類型 Entity（供內部業務邏輯使用）
    /// </summary>
    Task<ProductType?> GetProductTypeByIdAsync(int id);

    /// <summary>
    /// 根據 ID 取得後台商品類型表單 ViewModel（用於編輯頁面預填資料）
    /// 【架構說明】服務層負責 Entity → ViewModel 轉換，Controller 與 View 只使用 ViewModel
    /// </summary>
    Task<ProductTypeFormViewModel?> GetProductTypeFormByIdAsync(int id);

    /// <summary>
    /// 新增商品類型（後台管理用）
    /// 【架構說明】服務層負責 ViewModel → Entity 轉換，Controller 只傳遞 ViewModel
    /// </summary>
    Task<ProductTypeFormViewModel> CreateProductTypeAsync(ProductTypeFormViewModel vm);

    /// <summary>
    /// 更新商品類型（後台管理用）
    /// 【架構說明】服務層負責 ViewModel → Entity 轉換，Controller 只傳遞 ViewModel
    /// </summary>
    Task UpdateProductTypeAsync(ProductTypeFormViewModel vm);

    /// <summary>
    /// 刪除商品類型
    /// </summary>
    Task DeleteProductTypeAsync(int id);

    /// <summary>
    /// 取得所有父分類（頂層分類）
    /// 【公開前台使用，返回 Entity，供下拉選單使用】
    /// </summary>
    Task<IEnumerable<ProductType>> GetParentCategoriesAsync();

    /// <summary>
    /// 取得指定父分類下的所有子分類
    /// 【公開前台使用，返回 Entity，供 API 端點使用】
    /// </summary>
    Task<IEnumerable<ProductType>> GetChildrenByParentIdAsync(int parentId);

    /// <summary>
    /// 根據 ID 取得商品類型（包含子分類）
    /// </summary>
    Task<ProductType?> GetProductTypeWithChildrenAsync(int id);

    /// <summary>
    /// 取得所有子分類（只有實際可用於商品的分類）
    /// 【內部使用，返回 Entity】
    /// </summary>
    Task<IEnumerable<ProductType>> GetAllChildCategoriesAsync();

    /// <summary>
    /// 取得所有子分類下拉選單項目 ViewModel（供展示層使用，包含父分類資訊）
    /// </summary>
    Task<IEnumerable<SelectItemViewModel>> GetChildCategorySelectItemsAsync();
}
