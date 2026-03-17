// ===== AJAX 統一處理工具 =====
// 提供統一的 AJAX 請求處理和錯誤處理

const AjaxHelper = {
    /**
     * 發送 POST 請求
     * @param {string} url - 請求 URL
     * @param {object} data - 請求資料（會轉換為 URL-encoded 格式）
     * @param {object} options - 額外選項（包含 headers、onSuccess、onError、onComplete）
     */
    async post(url, data = {}, options = {}) {
        try {
            // 取得 CSRF Token
            const token = document.querySelector('input[name="__RequestVerificationToken"]')?.value;

            // 將資料轉換為 URL-encoded 格式
            const formData = new URLSearchParams();
            for (const key in data) {
                formData.append(key, data[key]);
            }

            // 自動加入 CSRF Token（如果存在）
            if (token) {
                formData.append('__RequestVerificationToken', token);
            }

            // 發送請求
            const response = await fetch(url, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/x-www-form-urlencoded',
                    ...(options.headers || {})
                },
                body: formData.toString()
            });

            // 解析 JSON
            const result = await response.json();

            // 執行成功回調
            if (options.onSuccess && typeof options.onSuccess === 'function') {
                options.onSuccess(result);
            }

            return result;
        } catch (error) {
            console.error('AJAX 錯誤:', error);

            // 執行錯誤回調
            if (options.onError && typeof options.onError === 'function') {
                options.onError(error);
            } else {
                // 預設錯誤處理
                this.handleDefaultError(error);
            }

            return { success: false, message: error.message };
        } finally {
            // 執行完成回調
            if (options.onComplete && typeof options.onComplete === 'function') {
                options.onComplete();
            }
        }
    },

    /**
     * 發送 JSON POST 請求（供 API Controller 使用）
     * 與 post() 的差異：Content-Type 為 application/json，不需要 CSRF Token
     * @param {string} url - 請求 URL
     * @param {object} data - 請求資料（會轉換為 JSON 格式）
     * @param {object} options - 額外選項
     */
    async postJson(url, data = {}, options = {}) {
        try {
            const response = await fetch(url, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    ...(options.headers || {})
                },
                body: JSON.stringify(data)
            });

            const result = await response.json();

            if (options.onSuccess && typeof options.onSuccess === 'function') {
                options.onSuccess(result);
            }

            return result;
        } catch (error) {
            console.error('AJAX 錯誤:', error);

            if (options.onError && typeof options.onError === 'function') {
                options.onError(error);
            } else {
                this.handleDefaultError(error);
            }

            return { success: false, message: error.message };
        } finally {
            if (options.onComplete && typeof options.onComplete === 'function') {
                options.onComplete();
            }
        }
    },

    /**
     * 發送 GET 請求
     * @param {string} url - 請求 URL
     * @param {object} params - 查詢參數
     * @param {object} options - 額外選項
     */
    async get(url, params = {}, options = {}) {
        try {
            // 將參數轉換為 URL 查詢字串
            const queryString = new URLSearchParams(params).toString();
            const fullUrl = queryString ? `${url}?${queryString}` : url;

            // 發送請求
            const response = await fetch(fullUrl, {
                method: 'GET',
                headers: {
                    ...(options.headers || {})
                }
            });

            // 解析 JSON
            const result = await response.json();

            // 執行成功回調
            if (options.onSuccess && typeof options.onSuccess === 'function') {
                options.onSuccess(result);
            }

            return result;
        } catch (error) {
            console.error('AJAX 錯誤:', error);

            // 執行錯誤回調
            if (options.onError && typeof options.onError === 'function') {
                options.onError(error);
            } else {
                // 預設錯誤處理
                this.handleDefaultError(error);
            }

            return { success: false, message: error.message };
        } finally {
            // 執行完成回調
            if (options.onComplete && typeof options.onComplete === 'function') {
                options.onComplete();
            }
        }
    },

    /**
     * 預設錯誤處理
     * @param {Error} error - 錯誤物件
     */
    handleDefaultError(error) {
        if (typeof showError === 'function') {
            showError('網路錯誤，請稍後再試');
        } else {
            alert(`錯誤：${error.message}`);
        }
    },

    /**
     * 取得 CSRF Token
     * @returns {string|null} CSRF Token 或 null
     */
    getCsrfToken() {
        return document.querySelector('input[name="__RequestVerificationToken"]')?.value || null;
    }
};

// 如果使用模組系統，導出
if (typeof module !== 'undefined' && module.exports) {
    module.exports = AjaxHelper;
}
