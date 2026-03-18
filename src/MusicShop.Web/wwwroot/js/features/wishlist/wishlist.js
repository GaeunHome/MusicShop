/**
 * 收藏清單 AJAX 切換
 * 點擊愛心按鈕時，透過 API 切換收藏狀態，即時更新愛心樣式
 *
 * API 端點：POST /api/wishlist/toggle（JSON 格式，不需要 CSRF Token）
 * 依賴：AjaxHelper (ajax-helper.js)
 */
(function () {
    'use strict';

    document.addEventListener('click', async function (e) {
        var btn = e.target.closest('.wishlist-toggle');
        if (!btn) return;

        var albumId = btn.dataset.albumId;
        if (!albumId) return;

        try {
            var data = await AjaxHelper.postJson('/api/wishlist/toggle', {
                albumId: parseInt(albumId)
            });

            if (!data.success) return;

            var icon = btn.querySelector('i');

            if (data.added) {
                btn.classList.add('wishlisted');
                btn.title = '取消收藏';
                if (icon) icon.className = 'bi bi-heart-fill text-danger';
                var label = btn.querySelector('.wishlist-label');
                if (label) label.textContent = '已收藏';
            } else {
                btn.classList.remove('wishlisted');
                btn.title = '加入收藏';
                if (icon) icon.className = 'bi bi-heart';
                var label = btn.querySelector('.wishlist-label');
                if (label) label.textContent = '加入最愛';
            }

            // 觸發愛心彈跳動畫
            btn.classList.add('animate');
            setTimeout(function () {
                btn.classList.remove('animate');
            }, 400);

            // Toast 提示（統一使用 ToastHelper）
            if (typeof ToastHelper !== 'undefined') {
                data.added ? ToastHelper.success(data.message) : ToastHelper.info(data.message);
            }
        } catch (err) {
            console.error('收藏切換失敗', err);
        }
    });
})();
