using System.ComponentModel.DataAnnotations;

namespace MusicShop.Service.ViewModels.Admin
{
    /// <summary>
    /// 後台商品類型新增／編輯表單 ViewModel。
    /// 遵循三層式架構，View 層透過此 ViewModel 與 Controller 溝通，
    /// Service 層負責 ViewModel ↔ Entity 的轉換，View 層不直接接觸 ProductType 實體。
    /// </summary>
    public class ProductTypeFormViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "類型名稱為必填")]
        [StringLength(50, ErrorMessage = "類型名稱不得超過 50 字")]
        [Display(Name = "類型名稱")]
        public string Name { get; set; } = string.Empty;

        [StringLength(200, ErrorMessage = "描述不得超過 200 字")]
        [Display(Name = "描述（選填）")]
        public string? Description { get; set; }

        [Display(Name = "父分類（選填）")]
        public int? ParentId { get; set; }

        [Display(Name = "排序順序")]
        public int DisplayOrder { get; set; }
    }
}
