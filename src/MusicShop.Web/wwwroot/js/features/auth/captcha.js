// 驗證碼重整功能
document.addEventListener('DOMContentLoaded', function () {
    const captchaImage = document.getElementById('captchaImage');
    const refreshBtn = document.getElementById('refreshCaptcha');

    function refreshCaptcha() {
        if (captchaImage) {
            // 加上時間戳避免瀏覽器快取
            captchaImage.src = captchaImage.src.split('?')[0] + '?t=' + Date.now();
        }
    }

    // 按鈕點擊重整
    if (refreshBtn) {
        refreshBtn.addEventListener('click', refreshCaptcha);
    }

    // 點擊圖片也可重整
    if (captchaImage) {
        captchaImage.addEventListener('click', refreshCaptcha);
    }
});
