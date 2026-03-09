// ===== 價格格式化工具 =====
// 提供統一的價格格式化方法

const PriceFormatter = {
    /**
     * 格式化價格為千分位，無小數點（例：1,200）
     * @param {number|string} price - 價格
     * @returns {string} 格式化後的價格字串
     */
    format(price) {
        const num = typeof price === 'string' ? parseFloat(price) : price;

        if (isNaN(num)) {
            return '0';
        }

        // 使用 toLocaleString 格式化為千分位，無小數點
        return Math.floor(num).toLocaleString('zh-TW', {
            minimumFractionDigits: 0,
            maximumFractionDigits: 0
        });
    },

    /**
     * 格式化價格為千分位，含貨幣符號（例：NT$ 1,200）
     * @param {number|string} price - 價格
     * @param {string} currency - 貨幣符號，預設為 'NT$'
     * @returns {string} 格式化後的價格字串
     */
    formatWithCurrency(price, currency = 'NT$') {
        return `${currency} ${this.format(price)}`;
    },

    /**
     * 解析格式化後的價格字串為數字
     * @param {string} formattedPrice - 格式化後的價格字串（例：'1,200'）
     * @returns {number} 價格數字
     */
    parse(formattedPrice) {
        if (typeof formattedPrice !== 'string') {
            return parseFloat(formattedPrice) || 0;
        }

        // 移除千分位逗號和貨幣符號
        const cleaned = formattedPrice.replace(/[NT$\s,]/g, '');
        return parseFloat(cleaned) || 0;
    },

    /**
     * 批次格式化價格清單
     * @param {Array<number|string>} prices - 價格清單
     * @returns {Array<string>} 格式化後的價格字串清單
     */
    formatBatch(prices) {
        return prices.map(price => this.format(price));
    }
};

// 如果使用模組系統，導出
if (typeof module !== 'undefined' && module.exports) {
    module.exports = PriceFormatter;
}
