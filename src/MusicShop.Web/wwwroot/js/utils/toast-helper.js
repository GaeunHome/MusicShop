// ===== Toast 提示工具 =====
// 提供統一的 Toast 提示方法（輕量級提示）

const ToastHelper = {
    /**
     * 預設 Toast 配置
     */
    defaultConfig: {
        toast: true,
        position: 'bottom-end',
        showConfirmButton: false,
        timer: 1500,
        timerProgressBar: true
    },

    /**
     * 顯示成功 Toast
     * @param {string} message - 提示訊息
     * @param {number} timer - 顯示時間（毫秒），預設 1500ms
     */
    success(message = '操作成功', timer = 1500) {
        this.show(message, 'success', timer);
    },

    /**
     * 顯示錯誤 Toast
     * @param {string} message - 提示訊息
     * @param {number} timer - 顯示時間（毫秒），預設 2000ms
     */
    error(message = '操作失敗', timer = 2000) {
        this.show(message, 'error', timer);
    },

    /**
     * 顯示警告 Toast
     * @param {string} message - 提示訊息
     * @param {number} timer - 顯示時間（毫秒），預設 2000ms
     */
    warning(message, timer = 2000) {
        this.show(message, 'warning', timer);
    },

    /**
     * 顯示資訊 Toast
     * @param {string} message - 提示訊息
     * @param {number} timer - 顯示時間（毫秒），預設 2000ms
     */
    info(message, timer = 2000) {
        this.show(message, 'info', timer);
    },

    /**
     * 顯示 Toast（基礎方法）
     * @param {string} message - 提示訊息
     * @param {string} icon - 圖示類型（success, error, warning, info）
     * @param {number} timer - 顯示時間（毫秒）
     */
    show(message, icon = 'success', timer = 1500) {
        const Toast = Swal.mixin({
            ...this.defaultConfig,
            timer: timer
        });

        Toast.fire({
            icon: icon,
            title: message
        });
    },

    /**
     * 自訂配置的 Toast
     * @param {object} config - 自訂配置（會覆蓋預設配置）
     */
    custom(config) {
        const Toast = Swal.mixin({
            ...this.defaultConfig,
            ...config
        });

        Toast.fire(config);
    }
};

// 如果使用模組系統，導出
if (typeof module !== 'undefined' && module.exports) {
    module.exports = ToastHelper;
}
