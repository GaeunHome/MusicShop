namespace MusicShop.Service.ViewModels;

/// <summary>
/// 下拉選單選項 ViewModel（通用）
/// 用於各種下拉清單的選項顯示
/// </summary>
public class SelectItemViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int? ParentId { get; set; }
    public string? ParentName { get; set; }
    public int DisplayOrder { get; set; }
}
