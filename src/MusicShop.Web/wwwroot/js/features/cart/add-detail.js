/**
 * 商品詳細頁「加入購物車」AJAX 邏輯
 * 與 add-modal.js 的差別：Modal 是從列表頁彈出，這裡是詳細頁內嵌表單
 */
(function () {
    var form = document.getElementById('detailAddToCartForm');
    if (!form) return;

    form.addEventListener('submit', async function (e) {
        e.preventDefault();

        var albumId = parseInt(document.getElementById('detailAlbumId').value);
        var quantity = parseInt(document.getElementById('detailQuantityInput').value);
        var submitBtn = form.querySelector('button[type="submit"]');

        submitBtn.disabled = true;
        submitBtn.textContent = '加入中...';

        try {
            var result = await AjaxHelper.postJson('/api/cart/add', {
                albumId: albumId,
                quantity: quantity
            });

            if (result.success) {
                updateCartBadge(result.cartCount);

                if (typeof showSuccess === 'function') {
                    showSuccess(result.message);
                }
            } else {
                if (typeof showError === 'function') {
                    showError(result.message);
                }
            }
        } catch (error) {
            if (typeof showError === 'function') {
                showError('加入購物車時發生錯誤，請稍後再試');
            }
        } finally {
            submitBtn.disabled = false;
            submitBtn.textContent = '加入購物車';
        }
    });
})();
