using System.ComponentModel.DataAnnotations;

namespace MusicShop.Service.ViewModels.Admin
{
    /// <summary>
    /// 後台專輯新增／編輯表單 ViewModel（三層式架構：View 層與 Data 層完全隔離）。
    ///
    /// 架構說明：
    ///   Write: View(ViewModel) → Controller(ViewModel) → Service（將 ViewModel 轉換為 Entity）→ Repository(Entity) → DB
    ///   Read:  DB → Repository(Entity) → Service（將 Entity 轉換為 ViewModel）→ Controller(ViewModel) → View(ViewModel)
    ///
    /// 此 ViewModel 相較於 Album 實體：
    ///   - 不包含導航屬性（Artist、ProductType、OrderItems）
    ///   - 不暴露非必要欄位
    ///   - 包含 Data Annotations 驗證規則，適合直接作為表單模型繫結
    /// </summary>
    public class AlbumFormViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "商品名稱為必填")]
        [StringLength(200, ErrorMessage = "商品名稱不得超過 200 字")]
        [Display(Name = "商品名稱")]
        public string Title { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "描述不得超過 500 字")]
        [Display(Name = "描述")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "價格為必填")]
        [Range(1, 999999, ErrorMessage = "價格必須介於 1 到 999999 之間")]
        [Display(Name = "價格")]
        public decimal Price { get; set; }

        [Required(ErrorMessage = "庫存為必填")]
        [Range(0, 99999, ErrorMessage = "庫存必須介於 0 到 99999 之間")]
        [Display(Name = "庫存")]
        public int Stock { get; set; }

        [Required(ErrorMessage = "請選擇藝人")]
        [Display(Name = "藝人/歌手")]
        public int? ArtistId { get; set; }

        [Display(Name = "商品類型（子分類）")]
        public int? ProductTypeId { get; set; }

        public string? CoverImageUrl { get; set; }

        public string? DescriptionImageUrl { get; set; }

        /// <summary>
        /// 樂觀並發控制欄位，對應 Album 實體的 RowVersion
        /// </summary>
        public byte[]? RowVersion { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
