// 首頁專用 JavaScript

// 等待 DOM 完全載入後執行
document.addEventListener('DOMContentLoaded', function() {
    console.log('首頁 JavaScript 已載入');

    // 初始化輪播圖
    initCarousel();
});

/**
 * 初始化輪播圖
 */
function initCarousel() {
    const carouselElement = document.querySelector('#homeCarousel');

    if (!carouselElement) {
        console.log('找不到輪播元素');
        return;
    }

    // Bootstrap 輪播圖已經透過 data-bs-ride="carousel" 自動初始化
    // 這裡只是記錄日誌
    console.log('輪播圖已就緒');
}

/**
 * 未來可以在這裡添加其他功能
 * 例如：動態文字、拖曳效果等
 */
