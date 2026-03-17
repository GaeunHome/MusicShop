/**
 * 圖片淡入載入器
 * 為商品圖片加入 fade-in 效果，提升頁面載入視覺體驗
 * 搭配 css/components/animations.css 中的 .fade-in-image 類別使用
 */
(function () {
    'use strict';

    /**
     * 初始化圖片淡入效果
     * 選取所有商品圖片，加入 fade-in-image 類別，
     * 並在圖片載入完成後加入 loaded 類別觸發淡入過渡
     */
    function initFadeInImages() {
        /* 選取商品卡片中的圖片 */
        var images = document.querySelectorAll('.album-card img, .card-img-top, .product-image');

        images.forEach(function (img) {
            /* 若圖片已經載入完成（例如從快取讀取），直接顯示 */
            if (img.complete && img.naturalHeight > 0) {
                img.classList.add('fade-in-image', 'loaded');
                return;
            }

            /* 先隱藏圖片，等待載入完成後淡入 */
            img.classList.add('fade-in-image');

            img.addEventListener('load', function () {
                img.classList.add('loaded');
            });

            /* 載入失敗時也要顯示（避免圖片永遠隱藏） */
            img.addEventListener('error', function () {
                img.classList.add('loaded');
            });
        });
    }

    /* DOM 載入完成後初始化 */
    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', initFadeInImages);
    } else {
        initFadeInImages();
    }
})();
