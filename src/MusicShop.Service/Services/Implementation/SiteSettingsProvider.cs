using Microsoft.Extensions.Logging;
using MusicShop.Data.UnitOfWork;
using MusicShop.Service.Constants;
using MusicShop.Service.Services.Interfaces;
using MusicShop.Service.ViewModels.Shared;

namespace MusicShop.Service.Services.Implementation;

/// <summary>
/// 網站設定提供者實作
/// 從資料庫 SystemSettings 讀取 site.* 開頭的參數，組合成 SiteSettingsViewModel 並快取
/// 資料庫不可用時回傳預設值，確保前台不因設定載入失敗而崩潰
/// </summary>
public class SiteSettingsProvider : ISiteSettingsProvider
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICacheService _cacheService;
    private readonly ILogger<SiteSettingsProvider> _logger;

    public SiteSettingsProvider(
        IUnitOfWork unitOfWork,
        ICacheService cacheService,
        ILogger<SiteSettingsProvider> logger)
    {
        _unitOfWork = unitOfWork;
        _cacheService = cacheService;
        _logger = logger;
    }

    public async Task<SiteSettingsViewModel> GetSiteSettingsAsync()
    {
        try
        {
            return await _cacheService.GetOrCreateAsync(
                CacheKeys.SiteSettings,
                async () =>
                {
                    var allSettings = await _unitOfWork.SystemSettings.GetAllOrderedAsync();
                    var dict = allSettings.ToDictionary(s => s.Key, s => s.Value);

                    return new SiteSettingsViewModel
                    {
                        SiteName = GetValue(dict, "site.name", "MusicShop"),
                        CustomerServicePhone = GetValue(dict, "site.customer_service_phone"),
                        CustomerServiceHours = GetValue(dict, "site.customer_service_hours"),
                        CustomerServiceEmail = GetValue(dict, "site.customer_service_email"),
                        CompanyTaxId = GetValue(dict, "site.company_tax_id"),
                        MembershipId = GetValue(dict, "site.membership_id"),
                        FacebookUrl = GetValue(dict, "site.social.facebook"),
                        InstagramUrl = GetValue(dict, "site.social.instagram"),
                        LineUrl = GetValue(dict, "site.social.line"),
                        MaintenanceMode = GetBoolValue(dict, "site.maintenance_mode"),
                        MaintenanceMessage = GetValue(dict, "site.maintenance_message"),
                        Announcement = GetValue(dict, "site.announcement")
                    };
                });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "載入網站設定失敗，使用預設值");
            return new SiteSettingsViewModel();
        }
    }

    private static string GetValue(Dictionary<string, string> dict, string key, string defaultValue = "")
    {
        return dict.TryGetValue(key, out var value) ? value : defaultValue;
    }

    private static bool GetBoolValue(Dictionary<string, string> dict, string key, bool defaultValue = false)
    {
        return dict.TryGetValue(key, out var value)
            && bool.TryParse(value, out var result) ? result : defaultValue;
    }
}
