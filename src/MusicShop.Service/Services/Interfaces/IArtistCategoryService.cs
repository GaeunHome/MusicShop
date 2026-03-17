using MusicShop.Data.Entities;
using MusicShop.Service.ViewModels;
using MusicShop.Service.ViewModels.Admin;
using MusicShop.Service.ViewModels.Shared;

namespace MusicShop.Service.Services.Interfaces;

/// <summary>
/// 藝人分類商業邏輯介面
/// </summary>
public interface IArtistCategoryService
{
    /// <summary>
    /// 取得所有藝人分類（返回 Entity，僅供 Service 層內部使用）
    /// Controller 應使用 GetArtistCategorySelectItemsAsync 取代
    /// </summary>
    Task<IEnumerable<ArtistCategory>> GetAllArtistCategoriesAsync();

    /// <summary>
    /// 取得所有藝人分類下拉選單項目 ViewModel（供展示層使用）
    /// </summary>
    Task<IEnumerable<SelectItemViewModel>> GetArtistCategorySelectItemsAsync();

    /// <summary>
    /// 根據 ID 取得後台藝人分類表單 ViewModel（用於編輯頁面預填資料）
    /// 【架構說明】服務層負責 Entity → ViewModel 轉換，Controller 與 View 只使用 ViewModel
    /// </summary>
    Task<ArtistCategoryFormViewModel?> GetArtistCategoryFormByIdAsync(int id);

    /// <summary>
    /// 新增藝人分類（後台管理用）
    /// 【架構說明】服務層負責 ViewModel → Entity 轉換，Controller 只傳遞 ViewModel
    /// </summary>
    Task<ArtistCategoryFormViewModel> CreateArtistCategoryAsync(ArtistCategoryFormViewModel vm);

    /// <summary>
    /// 更新藝人分類（後台管理用）
    /// 【架構說明】服務層負責 ViewModel → Entity 轉換，Controller 只傳遞 ViewModel
    /// </summary>
    Task UpdateArtistCategoryAsync(ArtistCategoryFormViewModel vm);

    /// <summary>
    /// 刪除藝人分類
    /// </summary>
    Task DeleteArtistCategoryAsync(int id);

    /// <summary>
    /// 取得藝人分類列表 ViewModel（後台分類管理頁使用）
    /// </summary>
    Task<List<ArtistCategoryListItemViewModel>> GetArtistCategoryListItemsAsync();
}
