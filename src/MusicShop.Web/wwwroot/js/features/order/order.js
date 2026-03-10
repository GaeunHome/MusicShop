// ===== 訂單模組 =====
// 處理訂單相關頁面的所有互動功能

const Order = {
    /**
     * 初始化訂單模組
     */
    init() {
        this.bindCancelOrderHandlers();
    },

    /**
     * 綁定取消訂單按鈕（訂單列表頁）
     */
    bindCancelOrderHandlers() {
        // 訂單列表頁的取消按鈕
        const cancelButtons = document.querySelectorAll('[data-order-cancel]');
        cancelButtons.forEach(btn => {
            btn.addEventListener('click', (e) => {
                e.preventDefault();
                const orderId = btn.dataset.orderId;
                this.confirmCancelOrder(orderId);
            });
        });

        // 訂單詳情頁的取消按鈕
        const cancelDetailBtn = document.querySelector('[data-order-cancel-detail]');
        if (cancelDetailBtn) {
            cancelDetailBtn.addEventListener('click', (e) => {
                e.preventDefault();
                this.confirmCancelOrderDetail();
            });
        }
    },

    /**
     * 確認取消訂單（訂單列表頁）
     */
    confirmCancelOrder(orderId) {
        Swal.fire({
            icon: 'warning',
            title: '確認取消訂單',
            text: '確定要取消此訂單嗎？此操作無法復原。',
            showCancelButton: true,
            confirmButtonText: '確定取消',
            cancelButtonText: '返回',
            confirmButtonColor: '#dc3545',
            cancelButtonColor: '#6c757d',
            reverseButtons: true
        }).then((result) => {
            if (result.isConfirmed) {
                document.getElementById('cancelOrderForm_' + orderId).submit();
            }
        });
    },

    /**
     * 確認取消訂單（訂單詳情頁）
     */
    confirmCancelOrderDetail() {
        Swal.fire({
            icon: 'warning',
            title: '確認取消訂單',
            text: '確定要取消此訂單嗎？此操作無法復原。',
            showCancelButton: true,
            confirmButtonText: '確定取消',
            cancelButtonText: '返回',
            confirmButtonColor: '#dc3545',
            cancelButtonColor: '#6c757d',
            reverseButtons: true
        }).then((result) => {
            if (result.isConfirmed) {
                document.getElementById('cancelOrderFormDetail').submit();
            }
        });
    }
};

// 如果使用模組系統，導出
if (typeof module !== 'undefined' && module.exports) {
    module.exports = Order;
}
