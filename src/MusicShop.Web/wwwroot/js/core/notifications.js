// ===== MusicShop 彈跳視窗提示系統 =====
// 使用 SweetAlert2 提供使用者友善的提示訊息

/**
 * 成功提示
 * @param {string} message - 提示訊息
 * @param {string} title - 標題（可選）
 */
function showSuccess(message, title = '成功') {
    Swal.fire({
        icon: 'success',
        title: title,
        text: message,
        confirmButtonText: '確定',
        confirmButtonColor: '#28a745'
    });
}

/**
 * 錯誤提示
 * @param {string} message - 提示訊息
 * @param {string} title - 標題（可選）
 */
function showError(message, title = '錯誤') {
    Swal.fire({
        icon: 'error',
        title: title,
        text: message,
        confirmButtonText: '確定',
        confirmButtonColor: '#dc3545'
    });
}

/**
 * 警告提示
 * @param {string} message - 提示訊息
 * @param {string} title - 標題（可選）
 */
function showWarning(message, title = '警告') {
    Swal.fire({
        icon: 'warning',
        title: title,
        text: message,
        confirmButtonText: '確定',
        confirmButtonColor: '#ffc107'
    });
}

/**
 * 資訊提示
 * @param {string} message - 提示訊息
 * @param {string} title - 標題（可選）
 */
function showInfo(message, title = '提示') {
    Swal.fire({
        icon: 'info',
        title: title,
        text: message,
        confirmButtonText: '確定',
        confirmButtonColor: '#17a2b8'
    });
}

/**
 * 確認對話框
 * @param {string} message - 確認訊息
 * @param {string} title - 標題（可選）
 * @param {Function} onConfirm - 確認後的回調函數
 * @param {Function} onCancel - 取消後的回調函數（可選）
 */
function showConfirm(message, title = '確認', onConfirm, onCancel) {
    Swal.fire({
        icon: 'question',
        title: title,
        text: message,
        showCancelButton: true,
        confirmButtonText: '確定',
        cancelButtonText: '取消',
        confirmButtonColor: '#007bff',
        cancelButtonColor: '#6c757d',
        reverseButtons: true
    }).then((result) => {
        if (result.isConfirmed && onConfirm) {
            onConfirm();
        } else if (result.isDismissed && onCancel) {
            onCancel();
        }
    });
}

/**
 * 確認刪除對話框
 * @param {string} itemName - 要刪除的項目名稱
 * @param {Function} onConfirm - 確認後的回調函數
 */
function showDeleteConfirm(itemName, onConfirm) {
    Swal.fire({
        icon: 'warning',
        title: '確認刪除',
        text: `確定要刪除「${itemName}」嗎？此操作無法復原。`,
        showCancelButton: true,
        confirmButtonText: '確定刪除',
        cancelButtonText: '取消',
        confirmButtonColor: '#dc3545',
        cancelButtonColor: '#6c757d',
        reverseButtons: true
    }).then((result) => {
        if (result.isConfirmed && onConfirm) {
            onConfirm();
        }
    });
}

/**
 * 需要登入提示對話框
 * 點擊「立即登入」後導向登入頁，並帶上 returnUrl 回到當前頁
 */
function showLoginRequired() {
    Swal.fire({
        icon: 'info',
        title: '請先登入',
        text: '您需要登入才能使用此功能',
        showCancelButton: true,
        confirmButtonText: '立即登入',
        cancelButtonText: '稍後再說',
        confirmButtonColor: '#b19cd9',
        cancelButtonColor: '#6c757d',
        reverseButtons: true
    }).then((result) => {
        if (result.isConfirmed) {
            const returnUrl = encodeURIComponent(window.location.pathname + window.location.search);
            window.location.href = '/Account/Login?returnUrl=' + returnUrl;
        }
    });
}

/**
 * 登出確認對話框
 * @param {Function} onConfirm - 確認後的回調函數
 */
function showLogoutConfirm(onConfirm) {
    Swal.fire({
        icon: 'question',
        title: '確認登出',
        text: '確定要登出嗎？',
        showCancelButton: true,
        confirmButtonText: '確定登出',
        cancelButtonText: '取消',
        confirmButtonColor: '#dc3545',
        cancelButtonColor: '#6c757d',
        reverseButtons: true
    }).then((result) => {
        if (result.isConfirmed && onConfirm) {
            onConfirm();
        }
    });
}

// ===== 自動處理 TempData 訊息 =====
// 在頁面載入時自動檢查 TempData 並顯示對應的提示
document.addEventListener('DOMContentLoaded', function () {
    // 檢查是否有 TempData Success 訊息
    const successMessage = document.querySelector('[data-tempdata-success]');
    if (successMessage) {
        const message = successMessage.getAttribute('data-tempdata-success');
        showSuccess(message);
    }

    // 檢查是否有 TempData Error 訊息
    const errorMessage = document.querySelector('[data-tempdata-error]');
    if (errorMessage) {
        const message = errorMessage.getAttribute('data-tempdata-error');
        showError(message);
    }

    // 檢查是否有 TempData Warning 訊息
    const warningMessage = document.querySelector('[data-tempdata-warning]');
    if (warningMessage) {
        const message = warningMessage.getAttribute('data-tempdata-warning');
        showWarning(message);
    }

    // 檢查是否有 TempData Info 訊息
    const infoMessage = document.querySelector('[data-tempdata-info]');
    if (infoMessage) {
        const message = infoMessage.getAttribute('data-tempdata-info');
        showInfo(message);
    }
});
