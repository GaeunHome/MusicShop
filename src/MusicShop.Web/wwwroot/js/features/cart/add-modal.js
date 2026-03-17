/**
 * 加入購物車 Modal 共用邏輯
 * 用於首頁和商品列表頁
 *
 * 改版：從表單 POST（整頁跳轉）改為 AJAX JSON（留在原頁面 + Badge 即時更新）
 */

// 當 Modal 開啟時，填充商品資訊
document.getElementById('addToCartModal').addEventListener('show.bs.modal', function (event) {
    // 取得觸發按鈕
    var button = event.relatedTarget;

    // 取得商品資訊
    var albumId = button.getAttribute('data-album-id');
    var albumTitle = button.getAttribute('data-album-title');
    var albumArtist = button.getAttribute('data-album-artist');
    var albumPrice = button.getAttribute('data-album-price');
    var albumStock = button.getAttribute('data-album-stock');
    var albumImage = button.getAttribute('data-album-image');

    // 填充 Modal 內容
    document.getElementById('modalAlbumId').value = albumId;
    document.getElementById('modalAlbumTitle').textContent = albumTitle;
    document.getElementById('modalAlbumPrice').textContent = parseFloat(albumPrice).toLocaleString('zh-TW');
    document.getElementById('modalAlbumStock').textContent = albumStock;
    document.getElementById('modalAlbumImage').src = albumImage || '/images/default-album.png';
    document.getElementById('modalAlbumThumbnail').src = albumImage || '/images/default-album.png';

    // 重置數量為 1
    document.getElementById('modalQuantity').value = 1;
    document.getElementById('modalQuantityInput').value = 1;

    // 儲存庫存數量供數量選擇器使用
    document.getElementById('modalQuantity').setAttribute('data-max-stock', albumStock);
});

// 減少數量
document.getElementById('decreaseQty').addEventListener('click', function() {
    var quantityInput = document.getElementById('modalQuantity');
    var currentQty = parseInt(quantityInput.value);
    if (currentQty > 1) {
        quantityInput.value = currentQty - 1;
        document.getElementById('modalQuantityInput').value = currentQty - 1;
    }
});

// 增加數量
document.getElementById('increaseQty').addEventListener('click', function() {
    var quantityInput = document.getElementById('modalQuantity');
    var currentQty = parseInt(quantityInput.value);
    var maxStock = parseInt(quantityInput.getAttribute('data-max-stock'));

    if (currentQty < maxStock) {
        quantityInput.value = currentQty + 1;
        document.getElementById('modalQuantityInput').value = currentQty + 1;
    } else {
        if (typeof showWarning === 'function') {
            showWarning('已達庫存上限！');
        } else {
            alert('已達庫存上限！');
        }
    }
});

// ==================== AJAX 加入購物車 ====================

// 攔截表單提交，改用 AJAX
document.getElementById('addToCartForm').addEventListener('submit', async function(e) {
    // 阻止表單的預設行為（原本會整頁跳轉到 /Cart/Add）
    e.preventDefault();

    var albumId = parseInt(document.getElementById('modalAlbumId').value);
    var quantity = parseInt(document.getElementById('modalQuantityInput').value);
    var submitBtn = this.querySelector('button[type="submit"]');

    // 防止連點：按鈕暫時停用，避免重複送出
    submitBtn.disabled = true;
    submitBtn.textContent = '加入中...';

    try {
        // 呼叫 API Controller（JSON 格式）
        var result = await AjaxHelper.postJson('/api/cart/add', {
            albumId: albumId,
            quantity: quantity
        });

        if (result.success) {
            // 更新導覽列 Badge 數字（API 回傳最新數量）
            updateCartBadge(result.cartCount);

            // 關閉 Modal
            var modal = bootstrap.Modal.getInstance(document.getElementById('addToCartModal'));
            modal.hide();

            // 顯示成功通知
            if (typeof showSuccess === 'function') {
                showSuccess(result.message);
            }
        } else {
            // 顯示錯誤（如庫存不足）
            if (typeof showError === 'function') {
                showError(result.message);
            }
        }
    } catch (error) {
        if (typeof showError === 'function') {
            showError('加入購物車時發生錯誤，請稍後再試');
        }
    } finally {
        // 不管成功或失敗，都恢復按鈕狀態
        submitBtn.disabled = false;
        submitBtn.textContent = '加入購物車';
    }
});

// updateCartBadge() 定義在 cart-badge.js（全域共用）
