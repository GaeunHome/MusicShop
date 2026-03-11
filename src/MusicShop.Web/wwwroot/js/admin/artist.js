/**
 * ArtistAdmin - 藝人管理模組
 * 負責後台藝人管理相關的前端邏輯
 */
const ArtistAdmin = {
    /**
     * 初始化（列表頁面）
     */
    initList: function() {
        console.log('藝人列表頁面已初始化');
        // 未來可加入：搜尋、排序、篩選等功能
    },

    /**
     * 初始化新增頁面
     */
    initCreate: function() {
        console.log('藝人新增頁面已初始化');
        this.initFormValidation();
    },

    /**
     * 初始化編輯頁面
     */
    initEdit: function() {
        console.log('藝人編輯頁面已初始化');
        this.initFormValidation();
    },

    /**
     * 初始化表單驗證
     */
    initFormValidation: function() {
        // 使用 jQuery Validation（如果有引入的話）
        // 或自訂驗證邏輯
        const $form = $('form');

        $form.on('submit', function(e) {
            // 基本驗證：藝人名稱不可為空
            const artistName = $('#Name').val().trim();
            if (!artistName) {
                e.preventDefault();
                showWarning('請輸入藝人名稱');
                return false;
            }

            // 基本驗證：藝人分類必須選擇
            const categoryId = $('#ArtistCategoryId').val();
            if (!categoryId) {
                e.preventDefault();
                showWarning('請選擇藝人分類');
                return false;
            }
        });
    }
};

// 全域導出
if (typeof window !== 'undefined') {
    window.ArtistAdmin = ArtistAdmin;
}
