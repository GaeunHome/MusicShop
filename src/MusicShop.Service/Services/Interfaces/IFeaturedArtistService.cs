using MusicShop.Service.ViewModels.Admin;
using MusicShop.Service.ViewModels.Home;

namespace MusicShop.Service.Services.Interfaces;

/// <summary>
/// 精選藝人商業邏輯介面
/// </summary>
public interface IFeaturedArtistService
{
    /// <summary>
    /// 取得首頁精選藝人展示資料
    /// </summary>
    Task<IEnumerable<FeaturedArtistDisplayViewModel>> GetActiveFeaturedArtistDisplaysAsync();

    /// <summary>
    /// 取得後台精選藝人列表
    /// </summary>
    Task<IEnumerable<FeaturedArtistListItemViewModel>> GetFeaturedArtistListItemsAsync();

    /// <summary>
    /// 根據 ID 取得精選藝人表單
    /// </summary>
    Task<FeaturedArtistFormViewModel?> GetFeaturedArtistFormByIdAsync(int id);

    /// <summary>
    /// 新增精選藝人
    /// </summary>
    Task CreateFeaturedArtistAsync(FeaturedArtistFormViewModel vm);

    /// <summary>
    /// 更新精選藝人
    /// </summary>
    Task UpdateFeaturedArtistAsync(FeaturedArtistFormViewModel vm);

    /// <summary>
    /// 刪除精選藝人
    /// </summary>
    Task DeleteFeaturedArtistAsync(int id);
}
