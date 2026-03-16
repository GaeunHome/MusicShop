using MusicShop.Data.Entities;
using MusicShop.Service.ViewModels.Admin;

namespace MusicShop.Service.Services.Interfaces;

/// <summary>
/// 藝人商業邏輯介面
/// </summary>
public interface IArtistService
{
    /// <summary>
    /// 取得所有藝人
    /// 【公開前台使用，返回 Entity，供下拉選單使用】
    /// </summary>
    Task<IEnumerable<Artist>> GetAllArtistsAsync();

    /// <summary>
    /// 根據藝人分類 ID 取得藝人列表（用於級聯下拉選單）
    /// 【公開前台使用，返回 Entity，供 API 端點使用】
    /// </summary>
    Task<IEnumerable<Artist>> GetArtistsByCategoryIdAsync(int artistCategoryId);

    /// <summary>
    /// 取得藝人分組資料（依藝人分類分組，用於 Mega Menu 和側邊欄）
    /// </summary>
    Task<Dictionary<ArtistCategory, IEnumerable<Artist>>> GetArtistsGroupedByCategoryAsync();

    /// <summary>
    /// 根據 ID 取得單一藝人（返回 Entity）
    /// </summary>
    Task<Artist?> GetArtistByIdAsync(int id);

    /// <summary>
    /// 取得後台藝人列表（ViewModel）
    /// 【架構說明】服務層負責 Entity → ViewModel 轉換，Controller 與 View 只使用 ViewModel
    /// </summary>
    Task<IEnumerable<ArtistListItemViewModel>> GetArtistListItemsAsync();

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
}
