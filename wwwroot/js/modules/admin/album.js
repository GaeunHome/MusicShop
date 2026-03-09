// ===== 後台專輯管理模組 =====
// 處理後台專輯管理頁面的所有互動功能

const AdminAlbum = {
    /**
     * 初始化專輯管理模組
     */
    init() {
        this.bindDeleteHandlers();
    },

    /**
     * 綁定刪除專輯按鈕
     */
    bindDeleteHandlers() {
        const deleteButtons = document.querySelectorAll('[data-album-delete]');
        deleteButtons.forEach(btn => {
            btn.addEventListener('click', (e) => {
                e.preventDefault();
                const albumTitle = btn.dataset.albumTitle;
                const albumId = btn.dataset.albumId;
                this.confirmDelete(albumTitle, albumId);
            });
        });
    },

    /**
     * 確認刪除專輯
     */
    confirmDelete(albumTitle, albumId) {
        showDeleteConfirm(albumTitle, () => {
            document.getElementById('deleteForm_' + albumId).submit();
        });
    }
};

// 如果使用模組系統，導出
if (typeof module !== 'undefined' && module.exports) {
    module.exports = AdminAlbum;
}
