/**
 * CascadeSelect - 通用級聯下拉選單工具類別
 * 用於處理父子關聯的下拉選單（如：藝人分類 → 藝人、商品大分類 → 子分類）
 *
 * 使用範例：
 * const artistCascade = new CascadeSelect({
 *     parentSelect: '#ArtistCategoryFilter',
 *     childSelect: '#ArtistSelect',
 *     apiUrl: '/Admin/GetArtistsByCategory',
 *     paramName: 'categoryId'
 * });
 */
class CascadeSelect {
    /**
     * @param {Object} options - 設定選項
     * @param {string} options.parentSelect - 父選單的 jQuery 選擇器
     * @param {string} options.childSelect - 子選單的 jQuery 選擇器
     * @param {string} options.apiUrl - API 端點 URL
     * @param {string} options.paramName - API 參數名稱（預設：'parentId'）
     * @param {boolean} options.enableSelect2 - 是否啟用 Select2（預設：false）
     * @param {Object} options.select2Options - Select2 自訂選項
     * @param {string} options.emptyText - 未選擇時的提示文字（預設：'請先選擇上層分類'）
     * @param {string} options.loadingText - 載入中的文字（預設：'載入中...'）
     * @param {string} options.noDataText - 無資料時的文字（預設：'此分類下沒有資料'）
     */
    constructor(options) {
        this.config = {
            parentSelect: options.parentSelect,
            childSelect: options.childSelect,
            apiUrl: options.apiUrl,
            paramName: options.paramName || 'parentId',
            enableSelect2: options.enableSelect2 || false,
            select2Options: options.select2Options || {},
            emptyText: options.emptyText || '請先選擇上層分類',
            loadingText: options.loadingText || '載入中...',
            noDataText: options.noDataText || '此分類下沒有資料'
        };

        this.parentSelect = $(this.config.parentSelect);
        this.childSelect = $(this.config.childSelect);

        this.init();
    }

    /**
     * 初始化級聯選單
     */
    init() {
        // 綁定父選單變更事件
        this.parentSelect.on('change', () => {
            const parentId = this.parentSelect.val();
            this.loadChildren(parentId);
        });
    }

    /**
     * 載入子選單資料
     * @param {string|number} parentId - 父項目 ID
     * @param {string|number} selectedId - 預設選中的子項目 ID（選填）
     */
    loadChildren(parentId, selectedId = null) {
        // 如果有啟用 Select2，先銷毀
        if (this.config.enableSelect2 && this.childSelect.hasClass("select2-hidden-accessible")) {
            this.childSelect.select2('destroy');
        }

        // 清空子選單並顯示載入中
        this.childSelect.html(`<option value="">${this.config.loadingText}</option>`);
        this.childSelect.prop('disabled', true);

        // 如果父選單沒有選擇任何值
        if (!parentId) {
            this.childSelect.html(`<option value="">${this.config.emptyText}</option>`);
            return;
        }

        // 發送 AJAX 請求獲取子項目
        $.ajax({
            url: this.config.apiUrl,
            type: 'GET',
            data: { [this.config.paramName]: parentId },
            success: (data) => {
                this.onLoadSuccess(data, selectedId);
            },
            error: () => {
                this.onLoadError();
            }
        });
    }

    /**
     * AJAX 載入成功時的處理
     * @param {Array} data - 子項目資料陣列
     * @param {string|number} selectedId - 預設選中的 ID
     */
    onLoadSuccess(data, selectedId) {
        // 清空子選單
        this.childSelect.html('<option value="">請選擇</option>');

        if (data && data.length > 0) {
            // 填入子項目選項
            $.each(data, (i, item) => {
                const option = $('<option></option>')
                    .val(item.id)
                    .text(item.name);

                // 如果有指定預選值，標記為 selected
                if (selectedId && item.id == selectedId) {
                    option.prop('selected', true);
                }

                this.childSelect.append(option);
            });

            // 啟用子選單
            this.childSelect.prop('disabled', false);

            // 如果啟用 Select2，初始化
            if (this.config.enableSelect2) {
                this.initSelect2();
            }
        } else {
            // 無資料
            this.childSelect.html(`<option value="">${this.config.noDataText}</option>`);
        }
    }

    /**
     * AJAX 載入失敗時的處理
     */
    onLoadError() {
        this.childSelect.html('<option value="">載入失敗</option>');
        alert('載入資料失敗，請重試');
    }

    /**
     * 初始化 Select2（如果啟用）
     */
    initSelect2() {
        const defaultSelect2Options = {
            theme: 'bootstrap-5',
            placeholder: '搜尋或選擇',
            allowClear: true,
            width: '100%'
        };

        const finalOptions = { ...defaultSelect2Options, ...this.config.select2Options };
        this.childSelect.select2(finalOptions);
    }

    /**
     * 手動觸發載入（用於編輯頁面預設值）
     * @param {string|number} parentId - 父項目 ID
     * @param {string|number} selectedId - 預設選中的子項目 ID
     */
    triggerLoad(parentId, selectedId) {
        if (parentId) {
            this.parentSelect.val(parentId);
            this.loadChildren(parentId, selectedId);
        }
    }
}

// 全域導出（支援非模組化使用）
if (typeof window !== 'undefined') {
    window.CascadeSelect = CascadeSelect;
}
