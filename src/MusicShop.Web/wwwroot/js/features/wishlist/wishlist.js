/**
 * 收藏清單 AJAX 切換
 * 點擊愛心按鈕時，送出 POST /Wishlist/Toggle，即時切換愛心樣式
 */
(function () {
    'use strict';

    const form = document.getElementById('wishlistForm');
    if (!form) return; // 未登入時不掛載

    const token = form.querySelector('[name=__RequestVerificationToken]')?.value;

    document.addEventListener('click', async function (e) {
        const btn = e.target.closest('.wishlist-toggle');
        if (!btn) return;

        const albumId = btn.dataset.albumId;
        if (!albumId) return;

        try {
            const res = await fetch('/Wishlist/Toggle', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/x-www-form-urlencoded',
                    'RequestVerificationToken': token
                },
                body: `albumId=${albumId}`
            });

            if (!res.ok) return;

            const data = await res.json();
            const icon = btn.querySelector('i');

            if (data.added) {
                btn.classList.add('wishlisted');
                btn.title = '取消收藏';
                if (icon) icon.className = 'bi bi-heart-fill text-danger';
                const label = btn.querySelector('.wishlist-label');
                if (label) label.textContent = '已收藏';
            } else {
                btn.classList.remove('wishlisted');
                btn.title = '加入收藏';
                if (icon) icon.className = 'bi bi-heart';
                const label = btn.querySelector('.wishlist-label');
                if (label) label.textContent = '加入最愛';
            }
        } catch (err) {
            console.error('收藏切換失敗', err);
        }
    });
})();
