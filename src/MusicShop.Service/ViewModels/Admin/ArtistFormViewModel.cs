using System.ComponentModel.DataAnnotations;

namespace MusicShop.Service.ViewModels.Admin
{
    /// <summary>
    /// 後台藝人新增／編輯表單 ViewModel。
    /// 遵循三層式架構，View 層透過此 ViewModel 與 Controller 溝通，
    /// Service 層負責 ViewModel ↔ Entity 的轉換，View 層不直接接觸 Artist 實體。
    /// </summary>
    public class ArtistFormViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "藝人名稱為必填")]
        [StringLength(100, ErrorMessage = "藝人名稱不得超過 100 字")]
        [Display(Name = "藝人名稱")]
        public string Name { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "描述不得超過 500 字")]
        [Display(Name = "藝人簡介（選填）")]
        public string? Description { get; set; }

        [Display(Name = "形象圖片 URL（選填）")]
        public string? ProfileImageUrl { get; set; }

        [Required(ErrorMessage = "請選擇藝人分類")]
        [Display(Name = "藝人分類")]
        public int ArtistCategoryId { get; set; }

        [Display(Name = "排序順序")]
        public int DisplayOrder { get; set; }
    }
}
