namespace MusicShop.Service.ViewModels.Cart
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
        /// 該商品小計金額
        /// </summary>
        public decimal Subtotal { get; set; }

        /// <summary>
        /// 購物車總金額
        /// </summary>
        public decimal CartTotal { get; set; }

        /// <summary>
        /// 購物車商品總數量
        /// </summary>
        public int CartItemCount { get; set; }
    }
}
