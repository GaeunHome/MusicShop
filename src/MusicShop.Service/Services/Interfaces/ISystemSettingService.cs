using MusicShop.Service.ViewModels.Admin;

namespace MusicShop.Service.Services.Interfaces;

/// <summary>
/// 系統參數商業邏輯介面
/// </summary>
public interface ISystemSettingService
{
    /// <summary>
    /// 取得所有系統參數列表
    /// </summary>
    Task<IEnumerable<SystemSettingListItemViewModel>> GetAllSettingsAsync();

    /// <summary>
    /// 根據 ID 取得系統參數表單 ViewModel
    /// </summary>
    Task<SystemSettingFormViewModel?> GetSettingFormByIdAsync(int id);

    /// <summary>
    /// 新增系統參數
    /// </summary>
    Task CreateSettingAsync(SystemSettingFormViewModel vm, string updatedBy);

    /// <summary>
    /// 更新系統參數
    /// </summary>
    Task UpdateSettingAsync(SystemSettingFormViewModel vm, string updatedBy);

    /// <summary>
    /// 刪除系統參數
    /// </summary>
    Task DeleteSettingAsync(int id);

    /// <summary>
    /// 根據 Key 取得參數值
    /// </summary>
    Task<string?> GetValueAsync(string key);
}
