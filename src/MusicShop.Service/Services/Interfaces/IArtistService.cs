using MusicShop.Library.Helpers;
using MusicShop.Service.ViewModels.Admin;
using MusicShop.Service.ViewModels.Shared;

namespace MusicShop.Service.Services.Interfaces;

/// <summary>
/// 藝人商業邏輯介面
/// </summary>
public interface IArtistService
{
    /// <summary>
    /// 取得所有藝人下拉選單項目 ViewModel（供展示層 SelectList 使用）
    /// </summary>
    Task<IEnumerable<SelectItemViewModel>> GetArtistSelectItemsAsync();

    /// <summary>
    /// 根據藝人分類 ID 取得藝人下拉選單項目 ViewModel（供級聯下拉選單 API 使用）
    /// Controller 應使用此方法取代 GetArtistsByCategoryIdAsync，避免直接接觸 Entity
    /// </summary>
    Task<IEnumerable<SelectItemViewModel>> GetArtistSelectItemsByCategoryIdAsync(int artistCategoryId);

    /// <summary>
    /// 根據藝人 ID 取得藝人名稱（供展示層標題顯示使用）
    /// Controller 應使用此方法取代 GetArtistByIdAsync，避免直接接觸 Entity
    /// </summary>
    Task<string?> GetArtistNameByIdAsync(int id);

    /// <summary>
    /// 根據藝人 ID 取得其所屬藝人分類 ID（供後台編輯頁面初始化級聯下拉選單使用）
    /// </summary>
    Task<int?> GetArtistCategoryIdByArtistIdAsync(int artistId);

    /// <summary>
    /// 取得後台藝人列表（ViewModel）
    /// 【架構說明】服務層負責 Entity → ViewModel 轉換，Controller 與 View 只使用 ViewModel
    /// </summary>
    Task<IEnumerable<ArtistListItemViewModel>> GetArtistListItemsAsync();

    /// <summary>
    /// 分頁取得後台藝人列表（支援依分類及上架狀態篩選）
    /// </summary>
    Task<PagedResult<ArtistListItemViewModel>> GetArtistListItemsPagedAsync(
        int page, int pageSize, int? artistCategoryId = null, bool? isActive = null);

    /// <summary>
    /// 根據 ID 取得後台藝人表單 ViewModel（用於編輯頁面預填資料）
    /// 【架構說明】服務層負責 Entity → ViewModel 轉換，Controller 與 View 只使用 ViewModel
    /// </summary>
    Task<ArtistFormViewModel?> GetArtistFormByIdAsync(int id);

    /// <summary>
    /// 建立藝人（後台管理用）
    /// 【架構說明】服務層負責 ViewModel → Entity 轉換，Controller 只傳遞 ViewModel
    /// </summary>
    Task<ArtistFormViewModel> CreateArtistAsync(ArtistFormViewModel vm);

    /// <summary>
    /// 更新藝人（後台管理用）
    /// 【架構說明】服務層負責 ViewModel → Entity 轉換，Controller 只傳遞 ViewModel
    /// </summary>
    Task UpdateArtistAsync(ArtistFormViewModel vm);

    /// <summary>
    /// 刪除藝人
    /// </summary>
    Task DeleteArtistAsync(int id);

    /// <summary>
    /// 切換藝人上架/下架狀態
    /// </summary>
    Task ToggleArtistActiveAsync(int id);

    /// <summary>
    /// 取得目前最大的排序順序值（供新增藝人時參考）
    /// </summary>
    Task<int> GetMaxDisplayOrderAsync();

    /// <summary>
    /// 取得導覽列藝人分組 ViewModel（_Layout.cshtml K-ARTIST Mega Menu 使用）
    /// 取代直接使用 Dictionary&lt;ArtistCategory, IEnumerable&lt;Artist&gt;&gt;
    /// </summary>
    Task<List<NavArtistGroupViewModel>> GetNavArtistGroupsAsync();
}
