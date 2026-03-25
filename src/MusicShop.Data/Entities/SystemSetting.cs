namespace MusicShop.Data.Entities
{
    /// <summary>
    /// 系統參數設定（通用 Key-Value 結構）
    /// 由 SuperAdmin 透過後台管理，不使用軟刪除
    /// </summary>
    public class SystemSetting
    {
        public int Id { get; set; }

        /// <summary>
        /// 參數識別鍵（唯一，如 site.name、order.shipping_fee）
        /// </summary>
        public string Key { get; set; } = string.Empty;

        /// <summary>
        /// 參數值
        /// </summary>
        public string Value { get; set; } = string.Empty;

        /// <summary>
        /// 參數說明
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// 分組名稱（如「網站設定」、「訂單設定」、「顯示設定」）
        /// </summary>
        public string? Group { get; set; }

        /// <summary>
        /// 值類型（string / int / decimal / bool）
        /// </summary>
        public string ValueType { get; set; } = "string";

        /// <summary>
        /// 最後更新時間
        /// </summary>
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// 最後更新者（Email 或姓名）
        /// </summary>
        public string? UpdatedBy { get; set; }
    }
}
