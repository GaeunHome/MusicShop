using System.ComponentModel.DataAnnotations;

namespace MusicShop.Service.ViewModels.Admin
{
    /// <summary>
    /// 後台幻燈片新增／編輯表單 ViewModel。
    /// 遵循三層式架構，View 層透過此 ViewModel 與 Controller 溝通，
    /// Service 層負責 ViewModel ↔ Entity 的轉換，View 層不直接接觸 Banner 實體。
    /// </summary>
    public class BannerFormViewModel
    {
        public int Id { get; set; }

        [Display(Name = "連結商品（可留空）")]
        public int? AlbumId { get; set; }

        [Display(Name = "顯示順序")]
        [Range(0, int.MaxValue, ErrorMessage = "顯示順序不可為負數")]
        public int DisplayOrder { get; set; } = 0;

        [Display(Name = "立即顯示/顯示中")]
        public bool IsActive { get; set; } = true;

        public string? ImageUrl { get; set; }
    }
}
