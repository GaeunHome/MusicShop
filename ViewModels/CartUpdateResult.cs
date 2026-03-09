namespace MusicShop.ViewModels
{
    /// <summary>
    /// 購物車更新結果 DTO（用於 AJAX 響應）
    /// </summary>
    public class CartUpdateResult
    {
        /// <summary>
        /// 是否成功
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// 訊息（成功或錯誤訊息）
        /// </summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// 更新後的數量
        /// </summary>
        public int Quantity { get; set; }

        /// <summary>
        /// 該商品小計（格式化後的字串，如 "1,200"）
        /// </summary>
        public string Subtotal { get; set; } = string.Empty;

        /// <summary>
        /// 購物車總金額（格式化後的字串，如 "5,600"）
        /// </summary>
        public string CartTotal { get; set; } = string.Empty;

        /// <summary>
        /// 購物車商品總數量
        /// </summary>
        public int CartItemCount { get; set; }
    }
}
