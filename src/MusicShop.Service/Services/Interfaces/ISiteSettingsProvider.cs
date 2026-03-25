using MusicShop.Service.ViewModels.Shared;

namespace MusicShop.Service.Services.Interfaces;

/// <summary>
/// 網站設定提供者介面
/// 從資料庫 SystemSettings 讀取網站相關設定並快取
/// </summary>
public interface ISiteSettingsProvider
{
    /// <summary>
    /// 取得網站設定（快取 + 資料庫）
    /// </summary>
    Task<SiteSettingsViewModel> GetSiteSettingsAsync();
}
