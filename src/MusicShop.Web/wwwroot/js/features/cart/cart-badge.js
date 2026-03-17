/**
 * 購物車 Badge 即時更新（全域共用）
 * 任何需要更新導覽列購物車數字的地方都呼叫 updateCartBadge(count)
 */

/**
 * 更新導覽列購物車 Badge 數字
 * @param {number} count - 購物車商品數量
 */
function updateCartBadge(count) {
    var badge = document.querySelector('.cart-badge');
    if (!badge) return;

    // 更新數字
    badge.textContent = count;

    // 觸發彈跳動畫
    badge.classList.remove('updated');
    void badge.offsetWidth; // 強制重繪，讓動畫可以重新觸發
    badge.classList.add('updated');
}
