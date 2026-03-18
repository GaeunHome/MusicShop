using System.ComponentModel.DataAnnotations;

namespace MusicShop.Service.ViewModels.Admin;

/// <summary>
/// 後台精選藝人新增/編輯表單 ViewModel
/// </summary>
public class FeaturedArtistFormViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "請選擇藝人")]
    [Display(Name = "藝人")]
    public int ArtistId { get; set; }

    [Display(Name = "顯示順序")]
    public int DisplayOrder { get; set; } = 0;

    [Display(Name = "立即顯示")]
    public bool IsActive { get; set; } = true;
}
