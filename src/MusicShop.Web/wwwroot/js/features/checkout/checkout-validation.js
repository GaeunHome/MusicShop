/**
 * 結帳表單即時驗證模組
 * 在使用者離開欄位時（blur 事件）進行前端驗證，提供即時回饋
 * 此為 UX 改善，不取代後端驗證邏輯
 */
const CheckoutValidation = (function () {

    // ==================== 配送方式與付款方式常數 ====================
    const DELIVERY_HOME = '0';       // 宅配到府
    const DELIVERY_SEVEN = '1';      // 7-11 超商取貨
    const DELIVERY_FAMILY = '2';     // 全家超商取貨
    const PAYMENT_CREDIT_CARD = '1'; // 信用卡線上刷卡

    // ==================== 驗證規則 ====================

    /**
     * 取得目前選取的配送方式
     */
    function getDeliveryMethod() {
        var checked = document.querySelector('input[name="DeliveryMethod"]:checked');
        return checked ? checked.value : null;
    }

    /**
     * 取得目前選取的付款方式
     */
    function getPaymentMethod() {
        var checked = document.querySelector('input[name="PaymentMethod"]:checked');
        return checked ? checked.value : null;
    }

    /**
     * 顯示欄位驗證錯誤
     * @param {HTMLElement} field - 欄位元素
     * @param {string} message - 錯誤訊息
     */
    function showError(field, message) {
        field.classList.remove('is-valid');
        field.classList.add('is-invalid');

        // 嘗試找到既有的錯誤訊息容器（如 span[id$="-error"]）
        var errorSpan = document.getElementById(field.id + '-error') ||
                        field.parentElement.querySelector('.invalid-feedback');

        if (!errorSpan) {
            // 建立新的錯誤訊息元素
            errorSpan = document.createElement('div');
            errorSpan.className = 'invalid-feedback';
            // 插入到欄位後方（若有 input-group 則插入到 input-group 後方）
            var parent = field.closest('.input-group') || field;
            parent.parentElement.insertBefore(errorSpan, parent.nextSibling);
        } else {
            // 確保使用 Bootstrap 的 invalid-feedback 樣式
            if (!errorSpan.classList.contains('invalid-feedback')) {
                errorSpan.classList.add('invalid-feedback');
            }
        }
        errorSpan.textContent = message;
        errorSpan.style.display = 'block';
    }

    /**
     * 清除欄位驗證錯誤並標記為有效
     * @param {HTMLElement} field - 欄位元素
     */
    function showValid(field) {
        field.classList.remove('is-invalid');
        field.classList.add('is-valid');

        var errorSpan = document.getElementById(field.id + '-error') ||
                        field.parentElement.querySelector('.invalid-feedback');
        if (errorSpan) {
            errorSpan.textContent = '';
            errorSpan.style.display = 'none';
        }
    }

    /**
     * 清除欄位的所有驗證狀態
     * @param {HTMLElement} field - 欄位元素
     */
    function clearValidation(field) {
        field.classList.remove('is-invalid', 'is-valid');

        var errorSpan = document.getElementById(field.id + '-error') ||
                        field.parentElement.querySelector('.invalid-feedback');
        if (errorSpan) {
            errorSpan.textContent = '';
            errorSpan.style.display = 'none';
        }
    }

    // ==================== 各欄位驗證函式 ====================

    /**
     * 驗證收件人姓名
     */
    function validateReceiverName() {
        var field = document.getElementById('ReceiverName');
        if (!field) return true;

        var value = field.value.trim();

        if (!value) {
            showError(field, '請輸入收件人姓名');
            return false;
        }
        if (value.length > 50) {
            showError(field, '收件人姓名不可超過 50 個字元');
            return false;
        }

        showValid(field);
        return true;
    }

    /**
     * 驗證收件人電話
     */
    function validateReceiverPhone() {
        var field = document.getElementById('ReceiverPhone');
        if (!field) return true;

        var value = field.value.trim();

        if (!value) {
            showError(field, '請輸入收件人電話');
            return false;
        }
        // 手機格式：09XXXXXXXX 或 市話格式：XX-XXXXXXXX
        if (!/^09\d{8}$/.test(value) && !/^\d{2,4}-\d{6,8}$/.test(value)) {
            showError(field, '請輸入有效的台灣電話號碼（手機：09XXXXXXXX 或 市話：XX-XXXXXXXX）');
            return false;
        }

        showValid(field);
        return true;
    }

    /**
     * 驗證宅配地址欄位（僅在宅配到府時驗證）
     */
    function validateAddressField(fieldId, fieldLabel) {
        var field = document.getElementById(fieldId);
        if (!field) return true;

        // 非宅配模式時清除驗證狀態
        if (getDeliveryMethod() !== DELIVERY_HOME) {
            clearValidation(field);
            return true;
        }

        var value = field.value.trim();
        if (!value) {
            showError(field, '請' + (field.tagName === 'SELECT' ? '選擇' : '輸入') + fieldLabel);
            return false;
        }

        showValid(field);
        return true;
    }

    /**
     * 驗證信用卡卡號
     */
    function validateCardNumber() {
        var field = document.getElementById('CardNumber');
        if (!field) return true;

        // 非信用卡付款時清除驗證狀態
        if (getPaymentMethod() !== PAYMENT_CREDIT_CARD) {
            clearValidation(field);
            return true;
        }

        var value = field.value.replace(/\s/g, '');
        if (!value) {
            showError(field, '請輸入信用卡號');
            return false;
        }
        if (!/^\d{16}$/.test(value)) {
            showError(field, '信用卡號必須為 16 位數字');
            return false;
        }

        showValid(field);
        return true;
    }

    /**
     * 驗證持卡人姓名
     */
    function validateCardHolderName() {
        var field = document.getElementById('CardHolderName');
        if (!field) return true;

        if (getPaymentMethod() !== PAYMENT_CREDIT_CARD) {
            clearValidation(field);
            return true;
        }

        var value = field.value.trim();
        if (!value) {
            showError(field, '請輸入持卡人姓名');
            return false;
        }

        showValid(field);
        return true;
    }

    /**
     * 驗證信用卡有效期限
     */
    function validateCardExpiry() {
        var field = document.getElementById('CardExpiry');
        if (!field) return true;

        if (getPaymentMethod() !== PAYMENT_CREDIT_CARD) {
            clearValidation(field);
            return true;
        }

        var value = field.value.trim();
        if (!value) {
            showError(field, '請輸入有效期限');
            return false;
        }
        if (!/^\d{2}\/\d{2}$/.test(value)) {
            showError(field, '有效期限格式為 MM/YY');
            return false;
        }

        showValid(field);
        return true;
    }

    /**
     * 驗證信用卡安全碼
     */
    function validateCardCVV() {
        var field = document.getElementById('CardCVV');
        if (!field) return true;

        if (getPaymentMethod() !== PAYMENT_CREDIT_CARD) {
            clearValidation(field);
            return true;
        }

        var value = field.value.trim();
        if (!value) {
            showError(field, '請輸入安全碼');
            return false;
        }
        if (!/^\d{3}$/.test(value)) {
            showError(field, '安全碼必須為 3 位數字');
            return false;
        }

        showValid(field);
        return true;
    }

    // ==================== 事件綁定 ====================

    /**
     * 為欄位綁定 blur 事件驗證
     * @param {string} fieldId - 欄位 ID
     * @param {Function} validateFn - 驗證函式
     */
    function bindBlur(fieldId, validateFn) {
        var field = document.getElementById(fieldId);
        if (field) {
            field.addEventListener('blur', validateFn);
        }
    }

    /**
     * 初始化結帳表單即時驗證
     */
    function init() {
        // 確認是否在結帳頁面
        var form = document.getElementById('checkoutForm');
        if (!form) return;

        // 收件人資訊 blur 驗證
        bindBlur('ReceiverName', validateReceiverName);
        bindBlur('ReceiverPhone', validateReceiverPhone);

        // 宅配地址欄位 blur 驗證
        bindBlur('City', function () { validateAddressField('City', '縣市'); });
        bindBlur('District', function () { validateAddressField('District', '鄉鎮市區'); });
        bindBlur('Address', function () { validateAddressField('Address', '詳細地址'); });

        // 信用卡欄位 blur 驗證
        bindBlur('CardNumber', validateCardNumber);
        bindBlur('CardHolderName', validateCardHolderName);
        bindBlur('CardExpiry', validateCardExpiry);
        bindBlur('CardCVV', validateCardCVV);

        // 配送方式切換時，清除不相關欄位的驗證狀態
        var deliveryRadios = document.querySelectorAll('input[name="DeliveryMethod"]');
        deliveryRadios.forEach(function (radio) {
            radio.addEventListener('change', function () {
                // 清除所有地址欄位驗證狀態
                ['City', 'District', 'PostalCode', 'Address'].forEach(function (id) {
                    var f = document.getElementById(id);
                    if (f) clearValidation(f);
                });
            });
        });

        // 付款方式切換時，清除信用卡欄位驗證狀態
        var paymentRadios = document.querySelectorAll('input[name="PaymentMethod"]');
        paymentRadios.forEach(function (radio) {
            radio.addEventListener('change', function () {
                ['CardNumber', 'CardHolderName', 'CardExpiry', 'CardCVV'].forEach(function (id) {
                    var f = document.getElementById(id);
                    if (f) clearValidation(f);
                });
            });
        });
    }

    // ==================== 公開介面 ====================
    return {
        init: init
    };

})();

// 頁面載入後初始化
document.addEventListener('DOMContentLoaded', function () {
    CheckoutValidation.init();
});
