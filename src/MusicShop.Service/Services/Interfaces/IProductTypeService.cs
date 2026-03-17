using MusicShop.Data.Entities;
using MusicShop.Service.ViewModels;
using MusicShop.Service.ViewModels.Admin;
using MusicShop.Service.ViewModels.Shared;

namespace MusicShop.Service.Services.Interfaces;

/// <summary>
/// 商品類型商業邏輯介面
/// </summary>
public interface IProductTypeService
{
    /// <summary>
    /// 取得所有商品類型（返回 Entity，僅供 Service 層內部使用）
    /// </summary>
    Task<IEnumerable<ProductType>> GetAllProductTypesAsync();

    /// <summary>
    /// 根據 ID 取得商品類型（返回 Entity，僅供 Service 層內部使用）
    /// Controller 應使用 GetProductTypeNameByIdAsync 或 GetParentIdByProductTypeIdAsync 取代
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
    /// 取得所有父分類（返回 Entity，僅供 Service 層內部使用）
    /// Controller 應使用 GetParentCategorySelectItemsAsync 取代
    /// </summary>
    Task<IEnumerable<ProductType>> GetParentCategoriesAsync();

    /// <summary>
    /// 取得指定父分類下的所有子分類（返回 Entity，僅供 Service 層內部使用）
    /// Controller 應使用 GetChildCategorySelectItemsByParentIdAsync 取代
    /// </summary>
    Task<IEnumerable<ProductType>> GetChildrenByParentIdAsync(int parentId);

    /// <summary>
    /// 根據 ID 取得商品類型含子分類（返回 Entity，僅供 Service 層內部使用）
    /// Controller 應使用 GetParentIdByProductTypeIdAsync 取代
    /// </summary>
    Task<ProductType?> GetProductTypeWithChildrenAsync(int id);

    /// <summary>
    /// 取得所有子分類（返回 Entity，僅供 Service 層內部使用）
    /// Controller 應使用 GetChildCategorySelectItemsAsync 取代
    /// </summary>
    Task<IEnumerable<ProductType>> GetAllChildCategoriesAsync();

    /// <summary>
    /// 取得所有父分類下拉選單項目 ViewModel（供展示層 SelectList 使用）
    /// Controller 應使用此方法取代 GetParentCategoriesAsync，避免直接接觸 Entity
    /// </summary>
    Task<IEnumerable<SelectItemViewModel>> GetParentCategorySelectItemsAsync();

    /// <summary>
    /// 取得指定父分類下的子分類下拉選單項目 ViewModel（供級聯下拉選單 API 使用）
    /// Controller 應使用此方法取代 GetChildrenByParentIdAsync，避免直接接觸 Entity
    /// </summary>
    Task<IEnumerable<SelectItemViewModel>> GetChildCategorySelectItemsByParentIdAsync(int parentId);

    /// <summary>
    /// 根據商品類型 ID 取得名稱（供展示層標題顯示使用）
    /// Controller 應使用此方法取代 GetProductTypeByIdAsync，避免直接接觸 Entity
    /// </summary>
    Task<string?> GetProductTypeNameByIdAsync(int id);

    /// <summary>
    /// 根據商品類型 ID 取得其父分類 ID（供後台編輯頁面初始化級聯下拉選單使用）
    /// </summary>
    Task<int?> GetParentIdByProductTypeIdAsync(int productTypeId);

    /// <summary>
    /// 取得所有子分類下拉選單項目 ViewModel（供展示層使用，包含父分類資訊）
    /// </summary>
    Task<IEnumerable<SelectItemViewModel>> GetChildCategorySelectItemsAsync();

    /// <summary>
    /// 取得商品類型分類樹 ViewModel（後台分類管理頁使用）
    /// </summary>
    Task<List<ProductTypeCategoryTreeViewModel>> GetCategoryTreeViewModelsAsync();

    /// <summary>
    /// 取得導覽列分類樹 ViewModel（_Layout.cshtml 使用）
    /// 取代直接在 View 中使用 Dictionary&lt;ProductType, IEnumerable&lt;ProductType&gt;&gt;
    /// </summary>
    Task<List<NavCategoryItemViewModel>> GetNavCategoryTreeAsync();
}
