using AutoMapper;
using Microsoft.Extensions.Logging;
using MusicShop.Data.Entities;
using MusicShop.Data.UnitOfWork;
using MusicShop.Library.Helpers;
using MusicShop.Service.Constants;
using MusicShop.Service.Services.Interfaces;
using MusicShop.Service.ViewModels.Admin;

namespace MusicShop.Service.Services.Implementation;

/// <summary>
/// 系統參數商業邏輯實作
/// </summary>
public class SystemSettingService : ISystemSettingService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<SystemSettingService> _logger;
    private readonly ICacheService _cacheService;

    public SystemSettingService(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<SystemSettingService> logger,
        ICacheService cacheService)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
        _cacheService = cacheService;
    }

    public async Task<IEnumerable<SystemSettingListItemViewModel>> GetAllSettingsAsync()
    {
        var settings = await _unitOfWork.SystemSettings.GetAllOrderedAsync();
        return _mapper.Map<IEnumerable<SystemSettingListItemViewModel>>(settings);
    }

    public async Task<SystemSettingFormViewModel?> GetSettingFormByIdAsync(int id)
    {
        var setting = await _unitOfWork.SystemSettings.GetByIdAsync(id);
        if (setting == null) return null;

        return _mapper.Map<SystemSettingFormViewModel>(setting);
    }

    public async Task CreateSettingAsync(SystemSettingFormViewModel vm, string updatedBy)
    {
        ValidationHelper.ValidateNotEmpty(vm.Key, "參數 Key", nameof(vm.Key));
        ValidationHelper.ValidateNotEmpty(updatedBy, "操作者", nameof(updatedBy));

        // 檢查 Key 是否已存在
        var existing = await _unitOfWork.SystemSettings.GetByKeyAsync(vm.Key);
        if (existing != null)
            throw new InvalidOperationException($"參數 Key '{vm.Key}' 已存在，請使用不同的 Key");

        var setting = _mapper.Map<SystemSetting>(vm);
        setting.UpdatedAt = DateTime.UtcNow;
        setting.UpdatedBy = updatedBy;

        await _unitOfWork.SystemSettings.AddAsync(setting);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("系統參數已新增：{Key}，操作者：{UpdatedBy}", vm.Key, updatedBy);
        _cacheService.RemoveByPrefix(CacheKeys.SystemPrefix);
    }

    public async Task UpdateSettingAsync(SystemSettingFormViewModel vm, string updatedBy)
    {
        ValidationHelper.ValidatePositive(vm.Id, "參數 ID", nameof(vm.Id));
        ValidationHelper.ValidateNotEmpty(updatedBy, "操作者", nameof(updatedBy));

        var existing = await _unitOfWork.SystemSettings.GetByIdAsync(vm.Id);
        ValidationHelper.ValidateEntityExists(existing, "系統參數", vm.Id);

        existing!.Value = vm.Value;
        existing.Description = vm.Description;
        existing.Group = vm.Group;
        existing.ValueType = vm.ValueType;
        existing.UpdatedAt = DateTime.UtcNow;
        existing.UpdatedBy = updatedBy;

        await _unitOfWork.SystemSettings.UpdateAsync(existing);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("系統參數已更新：{Key}，操作者：{UpdatedBy}", existing.Key, updatedBy);
        _cacheService.RemoveByPrefix(CacheKeys.SystemPrefix);
    }

    public async Task DeleteSettingAsync(int id)
    {
        ValidationHelper.ValidatePositive(id, "參數 ID", nameof(id));

        var setting = await _unitOfWork.SystemSettings.GetByIdAsync(id);
        ValidationHelper.ValidateEntityExists(setting, "系統參數", id);

        _logger.LogInformation("系統參數已刪除：{Key}", setting!.Key);

        await _unitOfWork.SystemSettings.DeleteAsync(setting);
        await _unitOfWork.SaveChangesAsync();

        _cacheService.RemoveByPrefix(CacheKeys.SystemPrefix);
    }

    public async Task<string?> GetValueAsync(string key)
    {
        var setting = await _unitOfWork.SystemSettings.GetByKeyAsync(key);
        return setting?.Value;
    }
}
