namespace MusicShop.Service.ViewModels.Admin
{
    /// <summary>
    /// 後台專輯列表顯示 ViewModel。
    /// 用於後台商品管理列表頁，包含顯示所需的摘要欄位及計算屬性。
    /// 遵循三層式架構：View 層只使用 ViewModel，不直接接觸 Data 層實體。
    /// </summary>
    public class AlbumListItemViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? ArtistName { get; set; }
        public string? ArtistCategoryName { get; set; }
        public string? ProductTypeName { get; set; }
        public decimal Price { get; set; }
        public int Stock { get; set; }

        /// <summary>
        /// 格式化後的價格顯示文字，例如 "NT$ 1,200"
        /// </summary>
        public string FormattedPrice => $"NT$ {Price:N0}";

        /// <summary>
        /// 庫存狀態對應的 Bootstrap Badge CSS 類別
        /// </summary>
        public string StockBadgeClass => Stock <= 0
            ? "bg-danger"
            : Stock <= 5
                ? "bg-warning text-dark"
                : "bg-success";

        /// <summary>
        /// 庫存狀態顯示文字
        /// </summary>
        public string StockDisplayText => Stock <= 0
            ? "已售完"
            : Stock <= 5
                ? $"{Stock}（低庫存）"
                : Stock.ToString();
    }
}
