/**
 * 加入購物車 Modal 共用邏輯
 * 用於首頁和商品列表頁
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
        alert('已達庫存上限！');
    }
});
