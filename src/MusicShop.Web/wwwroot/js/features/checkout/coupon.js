/**
 * 結帳頁面 - 優惠券套用功能
 * 當使用者選擇優惠券時，透過 AJAX 計算折扣金額並更新顯示
 *
 * 依賴：AjaxHelper (ajax-helper.js)
 */
(function () {
    'use strict';

    const couponSelect = document.getElementById('couponSelect');
    if (!couponSelect) return;

    const discountRow = document.getElementById('discount-row');
    const discountAmount = document.getElementById('discount-amount');
    const finalAmount = document.getElementById('final-amount');

    // 從頁面取得原始總金額
    const originalTotal = parseFloat(couponSelect.closest('.card-body')
        ?.querySelector('.summary-row span:last-child')?.textContent?.replace(/[^0-9.]/g, '') || '0');

    couponSelect.addEventListener('change', async function () {
        const userCouponId = this.value;

        if (!userCouponId) {
            // 取消選擇優惠券
            discountRow.style.display = 'none';
            finalAmount.textContent = 'NT$ ' + Math.round(originalTotal).toLocaleString();
            return;
        }

        try {
            // 使用 AjaxHelper 統一處理 AJAX 請求（含錯誤處理與 response.ok 檢查）
            const result = await AjaxHelper.postJson('/api/coupon/validate', {
                userCouponId: parseInt(userCouponId),
                totalAmount: originalTotal
            });

            if (result.success) {
                discountRow.style.display = 'flex';
                discountAmount.textContent = '-NT$ ' + Math.round(result.discountAmount).toLocaleString();
                finalAmount.textContent = 'NT$ ' + Math.round(result.finalAmount).toLocaleString();
            } else {
                discountRow.style.display = 'none';
                finalAmount.textContent = 'NT$ ' + Math.round(originalTotal).toLocaleString();
                if (typeof showError === 'function') {
                    showError(result.message || '優惠券驗證失敗');
                } else {
                    alert(result.message || '優惠券驗證失敗');
                }
                couponSelect.value = '';
            }
        } catch {
            if (typeof showError === 'function') {
                showError('驗證優惠券時發生錯誤');
            } else {
                alert('驗證優惠券時發生錯誤');
            }
            couponSelect.value = '';
        }
    });
})();
