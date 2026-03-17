/**
 * AlbumAdmin - 專輯管理模組
 * 負責後台專輯管理相關的前端邏輯（新增、編輯、刪除、級聯選擇等）
 *
 * 依賴：
 * - CascadeSelect (cascade-select.js)
 * - Select2Helper (select2-helper.js)
 * - jQuery
 */
const AlbumAdmin = {
    // 級聯選擇器實例
    artistCascade: null,
    productTypeCascade: null,

    /**
     * 初始化列表頁面
     */
    initList: function() {
        this.bindDeleteHandlers();
    },

    /**
     * 初始化新增頁面
     */
    initCreate: function() {
        this.initArtistCascade();
        this.initProductTypeCascade();
    },

    /**
     * 初始化編輯頁面
     * @param {number} selectedArtistCategoryId - 已選擇的藝人分類 ID
     * @param {number} selectedArtistId - 已選擇的藝人 ID
     * @param {number} selectedParentCategoryId - 已選擇的父分類 ID
     * @param {number} selectedProductTypeId - 已選擇的商品類型 ID
     */
    initEdit: function(selectedArtistCategoryId, selectedArtistId, selectedParentCategoryId, selectedProductTypeId) {

        // 初始化藝人級聯選擇
        this.initArtistCascade();

        // 初始化商品類型級聯選擇
        this.initProductTypeCascade();

        // 載入編輯頁面的預設值
        this.loadEditDefaults(selectedArtistCategoryId, selectedArtistId, selectedParentCategoryId, selectedProductTypeId);
    },

    /**
     * 初始化藝人級聯選擇（藝人分類 → 藝人）
     */
    initArtistCascade: function() {
        this.artistCascade = new CascadeSelect({
            parentSelect: '#ArtistCategoryFilter',
            childSelect: '#ArtistSelect',
            apiUrl: '/Admin/GetArtistsByCategory',
            paramName: 'categoryId',
            enableSelect2: true,
            select2Options: {
                placeholder: '搜尋或選擇藝人',
                allowClear: true
            },
            emptyText: '請先選擇藝人分類',
            noDataText: '此分類下沒有藝人'
        });
    },

    /**
     * 初始化商品類型級聯選擇（父分類 → 子分類）
     */
    initProductTypeCascade: function() {
        this.productTypeCascade = new CascadeSelect({
            parentSelect: '#ParentCategorySelect',
            childSelect: '#ProductTypeSelect',
            apiUrl: '/Admin/GetChildCategories',
            paramName: 'parentId',
            enableSelect2: false,
            emptyText: '請先選擇大分類',
            noDataText: '此分類下沒有子分類'
        });
    },

    /**
     * 載入編輯頁面的預設值
     */
    loadEditDefaults: function(artistCategoryId, artistId, parentCategoryId, productTypeId) {
        // 載入藝人預設值
        if (artistCategoryId && artistId) {
            this.artistCascade.triggerLoad(artistCategoryId, artistId);
        }

        // 載入商品類型預設值
        if (parentCategoryId && productTypeId) {
            this.productTypeCascade.triggerLoad(parentCategoryId, productTypeId);
        }
    },

    /**
     * 綁定刪除專輯按鈕
     */
    bindDeleteHandlers: function() {
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
    confirmDelete: function(albumTitle, albumId) {
        showDeleteConfirm(albumTitle, () => {
            const form = document.getElementById('deleteForm_' + albumId);
            if (form) {
                form.submit();
            }
        });
    }
};

// 全域導出（支援非模組化使用）
if (typeof window !== 'undefined') {
    window.AlbumAdmin = AlbumAdmin;
}

// 舊版兼容（保留舊的 AdminAlbum 名稱）
if (typeof window !== 'undefined') {
    window.AdminAlbum = AlbumAdmin;
}
