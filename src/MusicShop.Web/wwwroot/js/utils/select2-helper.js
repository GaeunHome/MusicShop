/**
 * Select2Helper - Select2 初始化與管理工具
 * 提供統一的 Select2 初始化方法和預設配置
 *
 * 使用範例：
 * Select2Helper.init('#mySelect', { placeholder: '請選擇藝人' });
 * Select2Helper.destroy('#mySelect');
 */
const Select2Helper = {
    /**
     * 預設配置
     */
    defaultConfig: {
        theme: 'bootstrap-5',
        placeholder: '請選擇',
        allowClear: true,
        width: '100%',
        language: {
            noResults: function() {
                return "未找到符合的結果";
            },
            searching: function() {
                return "搜尋中...";
            },
            inputTooShort: function(args) {
                return "請至少輸入 " + args.minimum + " 個字元";
            }
        }
    },

    /**
     * 初始化 Select2
     * @param {string} selector - jQuery 選擇器
     * @param {Object} customOptions - 自訂選項（會覆蓋預設值）
     * @returns {jQuery} - Select2 元素
     */
    init: function(selector, customOptions = {}) {
        const $element = $(selector);

        // 如果已經初始化，先銷毀
        if ($element.hasClass("select2-hidden-accessible")) {
            this.destroy(selector);
        }

        // 合併配置
        const options = { ...this.defaultConfig, ...customOptions };

        // 初始化 Select2
        return $element.select2(options);
    },

    /**
     * 初始化多個 Select2（批次初始化）
     * @param {string} selector - jQuery 選擇器（可以選中多個元素）
     * @param {Object} customOptions - 自訂選項
     */
    initMultiple: function(selector, customOptions = {}) {
        $(selector).each((index, element) => {
            this.init(element, customOptions);
        });
    },

    /**
     * 銷毀 Select2
     * @param {string} selector - jQuery 選擇器
     */
    destroy: function(selector) {
        const $element = $(selector);
        if ($element.hasClass("select2-hidden-accessible")) {
            $element.select2('destroy');
        }
    },

    /**
     * 清空選項（銷毀並重設為初始狀態）
     * @param {string} selector - jQuery 選擇器
     * @param {string} emptyText - 空白選項的文字
     */
    clear: function(selector, emptyText = '請選擇') {
        const $element = $(selector);

        // 銷毀 Select2
        this.destroy(selector);

        // 清空選項
        $element.html(`<option value="">${emptyText}</option>`);
    },

    /**
     * 啟用可搜尋的 Select2（預設配置 + 搜尋功能）
     * @param {string} selector - jQuery 選擇器
     * @param {Object} customOptions - 自訂選項
     */
    initSearchable: function(selector, customOptions = {}) {
        const searchableOptions = {
            minimumInputLength: 0, // 不輸入也可以顯示選項
            ...customOptions
        };

        return this.init(selector, searchableOptions);
    },

    /**
     * 啟用支援新增的 Select2（Tags Mode）
     * @param {string} selector - jQuery 選擇器
     * @param {Object} customOptions - 自訂選項
     */
    initWithTags: function(selector, customOptions = {}) {
        const tagsOptions = {
            tags: true,
            createTag: function(params) {
                var term = $.trim(params.term);

                if (term === '') {
                    return null;
                }

                return {
                    id: term,
                    text: term,
                    newTag: true
                };
            },
            ...customOptions
        };

        return this.init(selector, tagsOptions);
    },

    /**
     * 啟用支援 AJAX 的 Select2
     * @param {string} selector - jQuery 選擇器
     * @param {string} ajaxUrl - AJAX 端點 URL
     * @param {Object} customOptions - 自訂選項
     */
    initWithAjax: function(selector, ajaxUrl, customOptions = {}) {
        const ajaxOptions = {
            ajax: {
                url: ajaxUrl,
                dataType: 'json',
                delay: 250,
                data: function(params) {
                    return {
                        q: params.term, // 搜尋關鍵字
                        page: params.page || 1
                    };
                },
                processResults: function(data, params) {
                    params.page = params.page || 1;

                    return {
                        results: data.items,
                        pagination: {
                            more: (params.page * 30) < data.total_count
                        }
                    };
                },
                cache: true
            },
            minimumInputLength: 1,
            ...customOptions
        };

        return this.init(selector, ajaxOptions);
    },

    /**
     * 設定選中的值
     * @param {string} selector - jQuery 選擇器
     * @param {string|number|Array} value - 要設定的值（單選或多選）
     * @param {boolean} triggerChange - 是否觸發 change 事件
     */
    setValue: function(selector, value, triggerChange = true) {
        const $element = $(selector);
        $element.val(value);

        if (triggerChange) {
            $element.trigger('change');
        }
    },

    /**
     * 取得選中的值
     * @param {string} selector - jQuery 選擇器
     * @returns {string|Array} - 選中的值
     */
    getValue: function(selector) {
        return $(selector).val();
    },

    /**
     * 檢查是否已初始化 Select2
     * @param {string} selector - jQuery 選擇器
     * @returns {boolean}
     */
    isInitialized: function(selector) {
        return $(selector).hasClass("select2-hidden-accessible");
    }
};

// 全域導出（支援非模組化使用）
if (typeof window !== 'undefined') {
    window.Select2Helper = Select2Helper;
}
