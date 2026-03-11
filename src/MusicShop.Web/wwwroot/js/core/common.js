// ===== 全站通用功能模組 =====
// 處理所有頁面共用的功能，如登出確認、通用表單驗證等

const Common = {
    /**
     * 初始化通用功能
     */
    init() {
        this.bindLogoutHandler();
        this.bindGenericDeleteHandlers();
        this.bindGenericConfirmHandlers();
        this.bindAuthGuard();
    },

    /**
     * 攔截所有帶有 data-require-auth 屬性的元素點擊
     * 顯示「請先登入」彈跳視窗，取代直接跳轉到登入頁
     */
    bindAuthGuard() {
        document.addEventListener('click', function (e) {
            const el = e.target.closest('[data-require-auth]');
            if (!el) return;
            e.preventDefault();
            e.stopPropagation();
            showLoginRequired();
        });
    },

    /**
     * 綁定登出按鈕處理
     */
    bindLogoutHandler() {
        const logoutBtn = document.querySelector('[data-action="logout"]');
        if (logoutBtn) {
            logoutBtn.addEventListener('click', (e) => {
                e.preventDefault();
                this.handleLogout();
            });
        }
    },

    /**
     * 處理登出確認
     */
    handleLogout() {
        showLogoutConfirm(() => {
            document.getElementById('logoutForm').submit();
        });
    },

    /**
     * 綁定通用刪除按鈕（使用 data-confirm-delete 屬性）
     */
    bindGenericDeleteHandlers() {
        const deleteButtons = document.querySelectorAll('[data-confirm-delete]');
        deleteButtons.forEach(btn => {
            btn.addEventListener('click', (e) => {
                e.preventDefault();
                const itemName = btn.dataset.itemName || '此項目';
                const formId = btn.dataset.formId;

                if (formId) {
                    showDeleteConfirm(itemName, () => {
                        document.getElementById(formId).submit();
                    });
                }
            });
        });
    },

    /**
     * 綁定通用確認按鈕（使用 data-confirm 屬性）
     */
    bindGenericConfirmHandlers() {
        const confirmButtons = document.querySelectorAll('[data-confirm]');
        confirmButtons.forEach(btn => {
            btn.addEventListener('click', (e) => {
                e.preventDefault();
                const message = btn.dataset.confirmMessage || '確定要執行此操作嗎？';
                const title = btn.dataset.confirmTitle || '確認';
                const formId = btn.dataset.formId;

                if (formId) {
                    showConfirm(message, title, () => {
                        document.getElementById(formId).submit();
                    });
                }
            });
        });
    }
};

// 如果使用模組系統，導出
if (typeof module !== 'undefined' && module.exports) {
    module.exports = Common;
}
