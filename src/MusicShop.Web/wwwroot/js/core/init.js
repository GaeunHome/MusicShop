// ===== MusicShop 統一初始化機制 =====
// 根據頁面自動初始化對應的模組

document.addEventListener('DOMContentLoaded', function () {
    // 初始化全站通用功能
    if (typeof Common !== 'undefined') {
        Common.init();
    }

    // 根據頁面 data-page 屬性初始化對應模組
    const page = document.body.dataset.page;

    if (!page) {
        return;
    }

    // 根據頁面載入對應模組
    switch (page) {
        // 購物車相關
        case 'cart-index':
        case 'cart-checkout':
            if (typeof Cart !== 'undefined') {
                Cart.init();
            }
            break;

        // 訂單相關
        case 'order-index':
        case 'order-detail':
            if (typeof Order !== 'undefined') {
                Order.init();
            }
            break;

        // 後台管理 - 專輯（AlbumAdmin 沒有 init()，列表頁用 initList()）
        case 'admin-albums':
            if (typeof AdminAlbum !== 'undefined') {
                AdminAlbum.initList();
            }
            break;

        // 後台管理 - 藝人（ArtistAdmin 沒有 init()，列表頁用 initList()）
        case 'admin-artists':
            if (typeof ArtistAdmin !== 'undefined') {
                ArtistAdmin.initList();
            }
            break;

        // 後台管理 - 分類
        case 'admin-categories':
            if (typeof AdminCategory !== 'undefined') {
                AdminCategory.init();
            }
            break;

        // 後台管理 - 使用者
        case 'admin-users':
            if (typeof AdminUser !== 'undefined') {
                AdminUser.init();
            }
            break;
    }
});
