namespace MusicShop.Service.ViewModels.Admin;

/// <summary>
/// 系統參數列表項目 ViewModel
/// </summary>
public class SystemSettingListItemViewModel
{
    public int Id { get; set; }
    public string Key { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Group { get; set; }
    public string ValueType { get; set; } = "string";
    public DateTime UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }
}
