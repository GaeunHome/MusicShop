// ===== 後台使用者管理模組 =====
// 處理後台使用者管理頁面的所有互動功能

const AdminUser = {
    /**
     * 初始化使用者管理模組
     */
    init() {
        this.bindToggleAdminHandlers();
    },

    /**
     * 綁定切換管理員角色按鈕
     */
    bindToggleAdminHandlers() {
        const toggleButtons = document.querySelectorAll('[data-toggle-admin]');
        toggleButtons.forEach(btn => {
            btn.addEventListener('click', (e) => {
                e.preventDefault();
                const userEmail = btn.dataset.userEmail;
                const userId = btn.dataset.userId;
                const isRemove = btn.dataset.isRemove === 'true';
                this.confirmToggleAdmin(userEmail, userId, isRemove);
            });
        });
    },

    /**
     * 確認切換管理員角色
     */
    confirmToggleAdmin(userEmail, userId, isRemove) {
        const title = isRemove ? '確認移除管理員權限' : '確認設為管理員';
        const text = isRemove
            ? `確定要移除 ${userEmail} 的管理員權限嗎？`
            : `確定要將 ${userEmail} 設為管理員嗎？`;
        const icon = isRemove ? 'warning' : 'question';

        Swal.fire({
            icon: icon,
            title: title,
            text: text,
            showCancelButton: true,
            confirmButtonText: '確定',
            cancelButtonText: '取消',
            confirmButtonColor: isRemove ? '#ffc107' : '#28a745',
            cancelButtonColor: '#6c757d',
            reverseButtons: true
        }).then((result) => {
            if (result.isConfirmed) {
                document.getElementById('toggleAdmin_' + userId).submit();
            }
        });
    }
};

// 如果使用模組系統，導出
if (typeof module !== 'undefined' && module.exports) {
    module.exports = AdminUser;
}
