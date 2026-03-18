// ===== 購物車模組 =====
// 處理購物車頁面的所有互動功能（包含 AJAX 即時更新）

const Cart = {
    /**
     * 初始化購物車模組
     */
    init() {
        this.bindQuantityHandlers();
        this.bindRemoveHandlers();
        this.bindClearCartHandler();
        this.initCheckoutPage();
    },

    // ==================== 數量控制（AJAX 即時更新）====================

    /**
     * 綁定數量增加/減少按鈕
     */
    bindQuantityHandlers() {
        // 增加數量按鈕
        const increaseButtons = document.querySelectorAll('.quantity-increase');
        increaseButtons.forEach(btn => {
            btn.addEventListener('click', (e) => {
                e.preventDefault();
                const cartItemId = btn.dataset.cartItemId;
                const input = document.querySelector(`.quantity-input[data-cart-item-id="${cartItemId}"]`);
                const currentQty = parseInt(input.value);
                const maxQty = parseInt(input.max) || 99;

                if (currentQty < maxQty) {
                    this.updateQuantityAjax(cartItemId, currentQty + 1);
                } else {
                    showWarning(`最多只能購買 ${maxQty} 個`);
                }
            });
        });

        // 減少數量按鈕
        const decreaseButtons = document.querySelectorAll('.quantity-decrease');
        decreaseButtons.forEach(btn => {
            btn.addEventListener('click', (e) => {
                e.preventDefault();
                const cartItemId = btn.dataset.cartItemId;
                const input = document.querySelector(`.quantity-input[data-cart-item-id="${cartItemId}"]`);
                const currentQty = parseInt(input.value);

                if (currentQty > 1) {
                    // 數量大於 1，正常減少
                    this.updateQuantityAjax(cartItemId, currentQty - 1);
                } else {
                    // 數量為 1，直接觸發移除確認
                    const albumTitle = btn.dataset.albumTitle;
                    const formId = btn.dataset.formId;
                    this.confirmRemove(albumTitle, formId);
                }
            });
        });
    },

    /**
     * AJAX 更新購物車數量
     * @param {number} cartItemId - 購物車項目 ID
     * @param {number} newQuantity - 新數量
     */
    updateQuantityAjax(cartItemId, newQuantity) {
        // 顯示載入動畫
        this.showLoading(cartItemId);

        // 使用 AjaxHelper 統一處理 AJAX 請求
        AjaxHelper.post('/Cart/UpdateQuantityAjax', {
            cartItemId: cartItemId,
            quantity: newQuantity
        }, {
            onSuccess: (data) => {
                if (data.success) {
                    // 更新 UI
                    this.updateUI(cartItemId, data);

                    // 使用 ToastHelper 顯示成功訊息
                    ToastHelper.success('已更新');
                } else {
                    showError(data.message || '更新失敗');
                }
            },
            onError: (error) => {
                console.error('AJAX 錯誤:', error);
                showError('網路錯誤，請稍後再試');
            },
            onComplete: () => {
                // 隱藏載入動畫
                this.hideLoading(cartItemId);
            }
        });
    },

    /**
     * 更新 UI（數量、小計、總計、購物車徽章）
     * @param {number} cartItemId - 購物車項目 ID
     * @param {object} data - 伺服器返回的資料
     */
    updateUI(cartItemId, data) {
        // 1. 更新數量輸入框
        const input = document.querySelector(`.quantity-input[data-cart-item-id="${cartItemId}"]`);
        if (input) {
            input.value = data.quantity;
        }

        // 2. 更新該商品小計
        const subtotalElement = document.getElementById(`subtotal-${cartItemId}`);
        if (subtotalElement) {
            subtotalElement.textContent = data.subtotal;
        }

        // 3. 更新購物車總計（兩處）
        const cartTotalDisplay = document.getElementById('cart-total-display');
        if (cartTotalDisplay) {
            cartTotalDisplay.textContent = data.cartTotal;
        }

        const finalTotalDisplay = document.getElementById('final-total-display');
        if (finalTotalDisplay) {
            finalTotalDisplay.textContent = data.cartTotal;
        }

        // 4. 更新購物車徽章數量（使用全域 cart-badge.js 的 updateCartBadge）
        updateCartBadge(data.cartItemCount);
    },

    /**
     * 顯示載入動畫
     * @param {number} cartItemId - 購物車項目 ID
     */
    showLoading(cartItemId) {
        const loadingOverlay = document.getElementById(`loading-${cartItemId}`);
        if (loadingOverlay) {
            loadingOverlay.style.display = 'flex';
        }
    },

    /**
     * 隱藏載入動畫
     * @param {number} cartItemId - 購物車項目 ID
     */
    hideLoading(cartItemId) {
        const loadingOverlay = document.getElementById(`loading-${cartItemId}`);
        if (loadingOverlay) {
            loadingOverlay.style.display = 'none';
        }
    },


    // ==================== 移除商品 ====================

    /**
     * 綁定移除商品按鈕
     */
    bindRemoveHandlers() {
        const removeButtons = document.querySelectorAll('[data-cart-remove]');
        removeButtons.forEach(btn => {
            btn.addEventListener('click', (e) => {
                e.preventDefault();
                const albumTitle = btn.dataset.albumTitle;
                const formId = btn.dataset.formId;
                this.confirmRemove(albumTitle, formId);
            });
        });
    },

    /**
     * 確認移除單一商品
     */
    confirmRemove(albumTitle, formId) {
        showDeleteConfirm(albumTitle, () => {
            document.getElementById(formId).submit();
        });
    },

    // ==================== 清空購物車 ====================

    /**
     * 綁定清空購物車按鈕
     */
    bindClearCartHandler() {
        const clearBtn = document.querySelector('[data-cart-clear]');
        if (clearBtn) {
            clearBtn.addEventListener('click', (e) => {
                e.preventDefault();
                this.confirmClearCart();
            });
        }
    },

    /**
     * 確認清空購物車
     */
    confirmClearCart() {
        showConfirm('確定要清空購物車中的所有商品嗎？此操作無法復原。', '確認清空購物車', () => {
            document.getElementById('clearCartForm').submit();
        });
    },

    // ==================== 結帳頁面功能 ====================

    /**
     * 初始化結帳頁面功能（縣市/鄉鎮市區動態選單）
     */
    initCheckoutPage() {
        // 縣市選單
        const citySelect = document.getElementById('City');
        if (!citySelect) return; // 不在結帳頁面，直接返回

        const districtSelect = document.getElementById('District');
        const postalCodeInput = document.getElementById('PostalCode');

        // 綁定表單提交驗證
        this.bindCheckoutFormValidation();

        // 監聽縣市變更
        citySelect.addEventListener('change', () => {
            const selectedCity = citySelect.value;

            if (!selectedCity) {
                // 清空鄉鎮市區和郵遞區號
                districtSelect.innerHTML = '<option value="">請選擇鄉鎮市區</option>';
                postalCodeInput.value = '';
                districtSelect.disabled = true;
                return;
            }

            // 使用 AjaxHelper 取得該縣市的鄉鎮市區清單
            AjaxHelper.get('/Cart/GetDistricts', { city: selectedCity }, {
                onSuccess: (districts) => {
                    // 清空並重新填充鄉鎮市區選單
                    districtSelect.innerHTML = '<option value="">請選擇鄉鎮市區</option>';

                    districts.forEach(item => {
                        const option = document.createElement('option');
                        option.value = item.district;
                        option.textContent = item.district;
                        option.dataset.postalCode = item.postalCode;
                        districtSelect.appendChild(option);
                    });

                    districtSelect.disabled = false;
                    postalCodeInput.value = '';
                },
                onError: (error) => {
                    console.error('取得鄉鎮市區失敗:', error);
                    showError('載入鄉鎮市區資料失敗');
                }
            });
        });

        // 監聽鄉鎮市區變更（自動填入郵遞區號）
        districtSelect.addEventListener('change', () => {
            const selectedOption = districtSelect.options[districtSelect.selectedIndex];
            const postalCode = selectedOption.dataset.postalCode;

            if (postalCode) {
                postalCodeInput.value = postalCode;
            } else {
                postalCodeInput.value = '';
            }
        });

        // 配送方式切換（顯示/隱藏超商門市欄位）
        this.initDeliveryMethodToggle();

        // 發票類型切換（顯示/隱藏對應欄位）
        this.initInvoiceTypeToggle();

        // 付款方式切換（顯示/隱藏信用卡欄位）
        this.initPaymentMethodToggle();

        // 門市選擇功能
        this.initStoreSelection();

        // 信用卡輸入格式化
        this.initCreditCardFormatting();
    },

    /**
     * 通用 Radio 切換邏輯
     * @param {string} radioName - radio input 的 name 屬性
     * @param {Object} valueToElementsMap - { radioValue: [顯示的元素ID], ... }，未列出的元素會自動隱藏
     */
    initRadioToggle(radioName, valueToElementsMap) {
        const radios = document.querySelectorAll(`input[name="${radioName}"]`);
        if (!radios.length) return;

        // 收集所有受控的元素 ID
        const allElementIds = new Set();
        Object.values(valueToElementsMap).forEach(ids => ids.forEach(id => allElementIds.add(id)));

        const toggleFields = () => {
            const selectedValue = document.querySelector(`input[name="${radioName}"]:checked`)?.value;

            // 先隱藏所有受控欄位
            allElementIds.forEach(id => {
                const el = document.getElementById(id);
                if (el) el.style.display = 'none';
            });

            // 顯示選中值對應的欄位
            const showIds = valueToElementsMap[selectedValue] || [];
            showIds.forEach(id => {
                const el = document.getElementById(id);
                if (el) el.style.display = 'block';
            });
        };

        radios.forEach(radio => radio.addEventListener('change', toggleFields));
        toggleFields(); // 初始化
    },

    /**
     * 配送方式切換邏輯
     * 0 = HomeDelivery, 1 = SevenEleven, 2 = FamilyMart (enum 值)
     */
    initDeliveryMethodToggle() {
        this.initRadioToggle('DeliveryMethod', {
            '0': ['address-fields'],
            '1': ['seven-eleven-fields'],
            '2': ['family-mart-fields']
        });
    },

    /**
     * 付款方式切換邏輯
     * 0 = CashOnDelivery, 1 = CreditCard (enum 值)
     */
    initPaymentMethodToggle() {
        this.initRadioToggle('PaymentMethod', {
            '1': ['credit-card-fields']
        });
    },

    /**
     * 發票類型切換邏輯
     * 0 = Duplicate (二聯式), 1 = Triplicate (三聯式), 2 = EInvoice (電子發票)
     */
    initInvoiceTypeToggle() {
        this.initRadioToggle('InvoiceType', {
            '1': ['triplicate-fields'],
            '2': ['einvoice-fields']
        });
    },

    // ==================== 門市選擇功能 ====================

    /**
     * 初始化門市選擇功能
     */
    initStoreSelection() {
        // 7-11 門市選擇按鈕
        const selectSevenElevenBtn = document.getElementById('select-seven-eleven-btn');
        if (selectSevenElevenBtn) {
            selectSevenElevenBtn.addEventListener('click', () => {
                this.openStoreSelectionModal('sevenEleven');
            });
        }

        // 全家門市選擇按鈕
        const selectFamilyMartBtn = document.getElementById('select-family-mart-btn');
        if (selectFamilyMartBtn) {
            selectFamilyMartBtn.addEventListener('click', () => {
                this.openStoreSelectionModal('familyMart');
            });
        }

        // 綁定搜尋輸入框
        const searchInput = document.getElementById('store-search-input');
        if (searchInput) {
            searchInput.addEventListener('input', () => {
                this.filterStores();
            });
        }

        // 綁定縣市篩選
        const cityFilter = document.getElementById('store-city-filter');
        if (cityFilter) {
            cityFilter.addEventListener('change', () => {
                this.filterStores();
            });
        }
    },

    /**
     * 打開門市選擇 Modal
     * @param {string} storeType - 'sevenEleven' 或 'familyMart'
     */
    openStoreSelectionModal(storeType) {
        let modalTitle;
        let cvsType;

        if (storeType === 'sevenEleven') {
            modalTitle = '選擇 7-11 門市';
            cvsType = 'UNIMART';
        } else if (storeType === 'familyMart') {
            modalTitle = '選擇全家門市';
            cvsType = 'FAMI';
        } else {
            showError('無效的超商類型');
            return;
        }

        // 更新 Modal 標題（modalTitle 為硬編碼值，安全無 XSS 風險）
        const labelEl = document.getElementById('storeSelectionModalLabel');
        labelEl.textContent = '';
        const icon = document.createElement('i');
        icon.className = 'bi bi-shop me-2';
        labelEl.appendChild(icon);
        labelEl.appendChild(document.createTextNode(modalTitle));

        // 顯示 Modal
        const modal = new bootstrap.Modal(document.getElementById('storeSelectionModal'));
        modal.show();

        // 儲存當前超商類型（供後續使用）
        this.currentStoreType = storeType;

        // 清空搜尋和篩選
        const searchInput = document.getElementById('store-search-input');
        const cityFilter = document.getElementById('store-city-filter');
        if (searchInput) searchInput.value = '';
        if (cityFilter) cityFilter.value = '';

        // 從後端 API 載入門市清單
        this.fetchStoresFromApi(cvsType);
    },

    /**
     * 從後端 API 取得門市清單（串接 ECPay GetStoreList）
     * @param {string} cvsType - 'UNIMART' 或 'FAMI'
     */
    fetchStoresFromApi(cvsType) {
        // 顯示載入中
        const storeList = document.getElementById('store-list');
        const noStoresMessage = document.getElementById('no-stores-message');
        storeList.innerHTML = '<div class="text-center py-4"><div class="spinner-border text-primary" role="status"></div><p class="mt-2 text-muted">載入門市資料中...</p></div>';
        if (noStoresMessage) noStoresMessage.style.display = 'none';

        AjaxHelper.get('/Cart/GetStoreList', { type: cvsType }, {
            onSuccess: (data) => {
                if (data.success && data.stores) {
                    // 將 ECPay 回傳格式轉換為內部格式並快取
                    this.cachedStores = data.stores.map(s => ({
                        code: s.storeId,
                        name: s.storeName,
                        address: s.storeAddr,
                        city: s.storeAddr ? s.storeAddr.substring(0, 3) : ''
                    }));

                    // 建立縣市篩選清單
                    this.buildCityFilter(this.cachedStores);

                    // 渲染門市清單
                    this.renderStoreList(this.cachedStores);
                } else {
                    storeList.innerHTML = '';
                    showError(data.message || '載入門市資料失敗');
                }
            },
            onError: () => {
                storeList.innerHTML = '';
                showError('網路錯誤，無法載入門市資料');
            }
        });
    },

    /**
     * 建立縣市篩選選項（從門市資料動態產生）
     */
    buildCityFilter(stores) {
        const cityFilter = document.getElementById('store-city-filter');
        if (!cityFilter) return;

        const cities = [...new Set(stores.map(s => s.city))].sort();

        cityFilter.innerHTML = '<option value="">所有縣市</option>';
        cities.forEach(city => {
            const option = document.createElement('option');
            option.value = city;
            option.textContent = city;
            cityFilter.appendChild(option);
        });
    },

    /**
     * 篩選門市（從已快取的資料中過濾，不重複呼叫 API）
     */
    filterStoresFromCache() {
        if (!this.cachedStores) return;

        const searchKeyword = (document.getElementById('store-search-input')?.value || '').toLowerCase();
        const city = document.getElementById('store-city-filter')?.value || '';

        let filtered = this.cachedStores;

        if (city) {
            filtered = filtered.filter(s => s.city === city);
        }
        if (searchKeyword) {
            filtered = filtered.filter(s =>
                s.name.toLowerCase().includes(searchKeyword) ||
                s.address.toLowerCase().includes(searchKeyword) ||
                s.code.includes(searchKeyword)
            );
        }

        this.renderStoreList(filtered);
    },

    /**
     * 渲染門市清單（最多顯示 100 筆，避免 DOM 爆炸）
     */
    renderStoreList(stores) {
        const storeList = document.getElementById('store-list');
        const noStoresMessage = document.getElementById('no-stores-message');
        const MAX_DISPLAY = 100;

        // 清空清單
        storeList.innerHTML = '';

        if (!stores || stores.length === 0) {
            if (noStoresMessage) noStoresMessage.style.display = 'block';
            return;
        }

        if (noStoresMessage) noStoresMessage.style.display = 'none';

        const displayStores = stores.slice(0, MAX_DISPLAY);
        const fragment = document.createDocumentFragment();

        // 超過 100 筆時顯示提示
        if (stores.length > MAX_DISPLAY) {
            const hint = document.createElement('div');
            hint.className = 'alert alert-info py-2 mb-2';
            const hintIcon = document.createElement('i');
            hintIcon.className = 'bi bi-info-circle me-1';
            hint.appendChild(hintIcon);
            hint.appendChild(document.createTextNode(`共 ${stores.length} 間門市，目前顯示前 ${MAX_DISPLAY} 筆。請輸入關鍵字或選擇縣市縮小範圍。`));
            fragment.appendChild(hint);
        }

        displayStores.forEach(store => {
            const storeItem = document.createElement('a');
            storeItem.href = '#';
            storeItem.className = 'list-group-item list-group-item-action';

            // 使用 DOM API 建立元素，避免 innerHTML XSS 風險
            const wrapper = document.createElement('div');
            wrapper.className = 'd-flex w-100 justify-content-between align-items-start';

            const infoDiv = document.createElement('div');
            const nameEl = document.createElement('h6');
            nameEl.className = 'mb-1';
            nameEl.textContent = store.name;
            const addrEl = document.createElement('p');
            addrEl.className = 'mb-1 text-muted small';
            addrEl.textContent = store.address;
            const codeEl = document.createElement('small');
            codeEl.className = 'text-muted';
            codeEl.textContent = '門市代號：' + store.code;
            infoDiv.appendChild(nameEl);
            infoDiv.appendChild(addrEl);
            infoDiv.appendChild(codeEl);

            const badge = document.createElement('span');
            badge.className = 'badge bg-primary rounded-pill';
            badge.textContent = store.city;

            wrapper.appendChild(infoDiv);
            wrapper.appendChild(badge);
            storeItem.appendChild(wrapper);

            storeItem.addEventListener('click', (e) => {
                e.preventDefault();
                this.selectStore(store);
            });

            fragment.appendChild(storeItem);
        });

        storeList.appendChild(fragment);
    },

    /**
     * 篩選門市（使用快取資料，避免重複呼叫 API）
     */
    filterStores() {
        this.filterStoresFromCache();
    },

    /**
     * 選擇門市
     */
    selectStore(store) {
        // 根據當前超商類型填入對應的欄位
        if (this.currentStoreType === 'sevenEleven') {
            document.getElementById('SevenElevenStoreCode').value = store.code;
            document.getElementById('SevenElevenStoreName').value = store.name;
            document.getElementById('SevenElevenStoreAddress').value = store.address;
        } else if (this.currentStoreType === 'familyMart') {
            document.getElementById('FamilyMartStoreCode').value = store.code;
            document.getElementById('FamilyMartStoreName').value = store.name;
            document.getElementById('FamilyMartStoreAddress').value = store.address;
        }

        // 關閉 Modal
        const modal = bootstrap.Modal.getInstance(document.getElementById('storeSelectionModal'));
        if (modal) {
            modal.hide();
        }

        // 顯示成功訊息
        ToastHelper.success(`已選擇門市：${store.name}`);

        // 清空搜尋和篩選
        const searchInput = document.getElementById('store-search-input');
        const cityFilter = document.getElementById('store-city-filter');
        if (searchInput) searchInput.value = '';
        if (cityFilter) cityFilter.value = '';
    },

    // ==================== 信用卡輸入格式化 ====================

    /**
     * 初始化信用卡輸入欄位的格式化
     */
    initCreditCardFormatting() {
        // 信用卡號格式化（自動加入空格）
        const cardNumberInput = document.getElementById('CardNumber');
        if (cardNumberInput) {
            cardNumberInput.addEventListener('input', (e) => {
                let value = e.target.value.replace(/\s/g, ''); // 移除空格
                value = value.replace(/\D/g, ''); // 只保留數字

                // 每 4 位數加一個空格
                let formattedValue = value.match(/.{1,4}/g)?.join(' ') || value;

                e.target.value = formattedValue;
            });
        }

        // 有效期格式化（自動加入斜線）
        const cardExpiryInput = document.getElementById('CardExpiry');
        if (cardExpiryInput) {
            cardExpiryInput.addEventListener('input', (e) => {
                let value = e.target.value.replace(/\D/g, ''); // 只保留數字

                if (value.length >= 2) {
                    value = value.substring(0, 2) + '/' + value.substring(2, 4);
                }

                e.target.value = value;
            });
        }

        // CVV 只允許數字
        const cardCVVInput = document.getElementById('CardCVV');
        if (cardCVVInput) {
            cardCVVInput.addEventListener('input', (e) => {
                e.target.value = e.target.value.replace(/\D/g, ''); // 只保留數字
            });
        }
    },

    // ==================== 表單提交驗證 ====================

    /**
     * 綁定結帳表單提交前的自訂驗證
     */
    bindCheckoutFormValidation() {
        const checkoutForm = document.getElementById('checkoutForm');
        if (!checkoutForm) return;

        checkoutForm.addEventListener('submit', (e) => {
            // 取得配送方式
            const deliveryMethod = document.querySelector('input[name="DeliveryMethod"]:checked')?.value;

            // 驗證配送資訊
            if (deliveryMethod === '0') {
                // 宅配到府 - 驗證地址資訊
                const city = document.getElementById('City')?.value;
                const district = document.getElementById('District')?.value;
                const postalCode = document.getElementById('PostalCode')?.value;
                const address = document.getElementById('Address')?.value;

                if (!city || city.trim() === '' ||
                    !district || district.trim() === '' ||
                    !postalCode || postalCode.trim() === '' ||
                    !address || address.trim() === '') {
                    e.preventDefault();
                    this.showAddressWarning();
                    return false;
                }
            } else if (deliveryMethod === '1') {
                // 7-11 超商取貨
                const storeCode = document.getElementById('SevenElevenStoreCode')?.value;
                if (!storeCode || storeCode.trim() === '') {
                    e.preventDefault();
                    this.showStoreSelectionWarning('7-11');
                    return false;
                }
            } else if (deliveryMethod === '2') {
                // 全家超商取貨
                const storeCode = document.getElementById('FamilyMartStoreCode')?.value;
                if (!storeCode || storeCode.trim() === '') {
                    e.preventDefault();
                    this.showStoreSelectionWarning('全家');
                    return false;
                }
            }

            // 驗證發票資訊
            const invoiceType = document.querySelector('input[name="InvoiceType"]:checked')?.value;

            if (invoiceType === '1') {
                // 三聯式發票
                const companyTaxId = document.getElementById('CompanyTaxId')?.value;
                const companyName = document.getElementById('CompanyName')?.value;

                if (!companyTaxId || companyTaxId.trim() === '' || !companyName || companyName.trim() === '') {
                    e.preventDefault();
                    this.showInvoiceWarning('三聯式發票', '請填寫統一編號和公司抬頭');
                    return false;
                }
            } else if (invoiceType === '2') {
                // 電子發票
                const invoiceCarrier = document.getElementById('InvoiceCarrier')?.value;

                if (!invoiceCarrier || invoiceCarrier.trim() === '') {
                    e.preventDefault();
                    this.showInvoiceWarning('電子發票', '請填寫載具號碼');
                    return false;
                }
            }

            // 所有驗證通過
            return true;
        });
    },

    /**
     * 顯示地址資訊未填寫警告
     */
    showAddressWarning() {
        Swal.fire({
            icon: 'warning',
            title: '請填寫收件地址',
            html: '您選擇了<strong>宅配到府</strong>，請填寫完整的收件地址資訊（縣市、鄉鎮市區、郵遞區號、詳細地址）。',
            confirmButtonText: '我知道了',
            confirmButtonColor: '#b19cd9'
        });
    },

    /**
     * 顯示門市未選擇警告
     */
    showStoreSelectionWarning(storeName) {
        Swal.fire({
            icon: 'warning',
            title: '請選擇門市',
            html: `
                <div class="text-start">
                    <p>您選擇了<strong>${storeName}超商取貨</strong>，但尚未選擇門市。</p>
                    <p class="mb-0">請點擊「<strong>選擇${storeName}門市</strong>」按鈕來選擇取貨門市。</p>
                </div>
            `,
            confirmButtonText: '我知道了',
            confirmButtonColor: '#b19cd9',
            didOpen: () => {
                // 自動滾動到門市選擇區域
                const storeFields = storeName === '7-11'
                    ? document.getElementById('seven-eleven-fields')
                    : document.getElementById('family-mart-fields');

                if (storeFields) {
                    setTimeout(() => {
                        storeFields.scrollIntoView({ behavior: 'smooth', block: 'center' });
                    }, 300);
                }
            }
        });
    },

    /**
     * 顯示發票資訊未填寫警告
     */
    showInvoiceWarning(invoiceType, message) {
        Swal.fire({
            icon: 'warning',
            title: '發票資訊未完整',
            html: `您選擇了<strong>${invoiceType}</strong>，${message}。`,
            confirmButtonText: '我知道了',
            confirmButtonColor: '#b19cd9'
        });
    }
};

// 如果使用模組系統，導出
if (typeof module !== 'undefined' && module.exports) {
    module.exports = Cart;
}
