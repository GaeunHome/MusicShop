// ===== 後台分類管理模組 =====
// 處理後台分類管理頁面的所有互動功能

const AdminCategory = {
    /**
     * 初始化分類管理模組
     */
    init() {
        this.bindArtistCategoryDeleteHandlers();
        this.bindProductTypeDeleteHandlers();
    },

    /**
     * 綁定藝人分類刪除按鈕
     */
    bindArtistCategoryDeleteHandlers() {
        const deleteButtons = document.querySelectorAll('[data-artist-category-delete]');
        deleteButtons.forEach(btn => {
            btn.addEventListener('click', (e) => {
                e.preventDefault();
                const categoryName = btn.dataset.categoryName;
                const categoryId = btn.dataset.categoryId;
                this.confirmDeleteArtistCategory(categoryName, categoryId);
            });
        });
    },

    /**
     * 確認刪除藝人分類
     */
    confirmDeleteArtistCategory(categoryName, categoryId) {
        showDeleteConfirm(categoryName, () => {
            document.getElementById('deleteArtistCat_' + categoryId).submit();
        });
    },

    /**
     * 綁定商品類型刪除按鈕（父分類和子分類）
     */
    bindProductTypeDeleteHandlers() {
        // 父分類刪除
        const parentDeleteButtons = document.querySelectorAll('[data-product-type-parent-delete]');
        parentDeleteButtons.forEach(btn => {
            btn.addEventListener('click', (e) => {
                e.preventDefault();
                const parentName = btn.dataset.parentName;
                const parentId = btn.dataset.parentId;
                this.confirmDeleteParent(parentName, parentId);
            });
        });

        // 子分類刪除
        const childDeleteButtons = document.querySelectorAll('[data-product-type-child-delete]');
        childDeleteButtons.forEach(btn => {
            btn.addEventListener('click', (e) => {
                e.preventDefault();
                const childName = btn.dataset.childName;
                const childId = btn.dataset.childId;
                this.confirmDeleteChild(childName, childId);
            });
        });
    },

    /**
     * 確認刪除父分類（會刪除所有子分類）
     */
    confirmDeleteParent(parentName, parentId) {
        Swal.fire({
            icon: 'warning',
            title: '確認刪除父分類',
            text: `確定要刪除「${parentName}」嗎？此操作會同時刪除所有子分類！此操作無法復原。`,
            showCancelButton: true,
            confirmButtonText: '確定刪除',
            cancelButtonText: '取消',
            confirmButtonColor: '#dc3545',
            cancelButtonColor: '#6c757d',
            reverseButtons: true
        }).then((result) => {
            if (result.isConfirmed) {
                document.getElementById('deleteParent_' + parentId).submit();
            }
        });
    },

    /**
     * 確認刪除子分類
     */
    confirmDeleteChild(childName, childId) {
        showDeleteConfirm(childName, () => {
            document.getElementById('deleteChild_' + childId).submit();
        });
    }
};

// 如果使用模組系統，導出
if (typeof module !== 'undefined' && module.exports) {
    module.exports = AdminCategory;
}
