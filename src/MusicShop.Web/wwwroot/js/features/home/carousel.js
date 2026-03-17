// 首頁專用 JavaScript

// 等待 DOM 完全載入後執行
document.addEventListener('DOMContentLoaded', function() {
    // 初始化輪播圖
    initCarousel();
});

/**
 * 初始化輪播圖
 */
function initCarousel() {
    const carouselElement = document.querySelector('#homeCarousel');

    if (!carouselElement) {
        return;
    }

    // Bootstrap 輪播圖已經透過 data-bs-ride="carousel" 自動初始化
}
