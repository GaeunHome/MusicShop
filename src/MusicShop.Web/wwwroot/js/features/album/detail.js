// ==================== 商品詳情頁面 JavaScript ====================

// 全域變數（由 Razor 頁面注入）
// const imageUrls = [...]; // 商品圖片 URL 陣列
// const maxStock = ...; // 最大庫存數量

let currentImageIndex = 0;

// ==================== 圖片輪播功能 ====================

/**
 * 切換圖片
 * @param {number} direction - 方向 (-1: 上一張, 1: 下一張)
 */
function changeImage(direction) {
    if (!imageUrls || imageUrls.length === 0) return;

    currentImageIndex += direction;

    // 循環處理
    if (currentImageIndex < 0) {
        currentImageIndex = imageUrls.length - 1;
    } else if (currentImageIndex >= imageUrls.length) {
        currentImageIndex = 0;
    }

    updateMainImage();
}

/**
 * 選擇特定圖片
 * @param {number} index - 圖片索引
 */
function selectImage(index) {
    if (!imageUrls || imageUrls.length === 0) return;

    currentImageIndex = index;
    updateMainImage();
}

/**
 * 更新主圖顯示
 */
function updateMainImage() {
    const mainImage = document.getElementById('mainImage');
    if (!mainImage) return;

    // 更新圖片
    mainImage.src = imageUrls[currentImageIndex];

    // 更新縮圖選中狀態
    const thumbnails = document.querySelectorAll('.thumbnail-item');
    thumbnails.forEach((thumb, index) => {
        if (index === currentImageIndex) {
            thumb.classList.add('active');
        } else {
            thumb.classList.remove('active');
        }
    });
}

// ==================== 數量選擇器功能 ====================

/**
 * 減少數量
 */
function decreaseQuantity() {
    const quantityInput = document.getElementById('quantity');
    const hiddenInput = document.getElementById('detailQuantityInput');

    let currentValue = parseInt(quantityInput.value);
    if (currentValue > 1) {
        currentValue--;
        quantityInput.value = currentValue;
        if (hiddenInput) {
            hiddenInput.value = currentValue;
        }
    }
}

/**
 * 增加數量
 */
function increaseQuantity() {
    const quantityInput = document.getElementById('quantity');
    const hiddenInput = document.getElementById('detailQuantityInput');

    let currentValue = parseInt(quantityInput.value);
    if (currentValue < maxStock) {
        currentValue++;
        quantityInput.value = currentValue;
        if (hiddenInput) {
            hiddenInput.value = currentValue;
        }
    }
}

/**
 * 手動輸入數量時的驗證
 */
function validateQuantityInput() {
    const quantityInput = document.getElementById('quantity');
    const hiddenInput = document.getElementById('detailQuantityInput');

    let value = parseInt(quantityInput.value);

    // 驗證範圍
    if (isNaN(value) || value < 1) {
        value = 1;
    } else if (value > maxStock) {
        value = maxStock;
    }

    quantityInput.value = value;
    if (hiddenInput) {
        hiddenInput.value = value;
    }
}



// ==================== 鍵盤快捷鍵 ====================

/**
 * 鍵盤事件處理
 */
document.addEventListener('keydown', function (e) {
    // 只在商品詳情頁面生效
    if (!document.getElementById('album-detail-page')) return;

    // 左右箭頭切換圖片
    if (e.key === 'ArrowLeft') {
        changeImage(-1);
    } else if (e.key === 'ArrowRight') {
        changeImage(1);
    }
});

// ==================== 頁面載入完成後的初始化 ====================

document.addEventListener('DOMContentLoaded', function () {
    // 數量輸入框事件監聽（如果需要手動輸入）
    const quantityInput = document.getElementById('quantity');
    if (quantityInput) {
        quantityInput.addEventListener('change', validateQuantityInput);
        quantityInput.addEventListener('blur', validateQuantityInput);
    }

    // 圖片預載入（提升用戶體驗）
    if (imageUrls && imageUrls.length > 0) {
        imageUrls.forEach(url => {
            const img = new Image();
            img.src = url;
        });
    }

});

