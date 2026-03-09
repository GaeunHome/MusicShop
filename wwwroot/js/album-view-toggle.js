/**
 * 專輯列表視圖切換功能
 */

document.addEventListener('DOMContentLoaded', function () {
    const gridViewBtn = document.getElementById('gridViewBtn');
    const listViewBtn = document.getElementById('listViewBtn');
    const productContainer = document.getElementById('productContainer');

    // 從 localStorage 讀取上次選擇的視圖模式
    const savedViewMode = localStorage.getItem('albumViewMode') || 'grid';

    // 設定初始視圖
    if (savedViewMode === 'list') {
        switchToListView();
    } else {
        switchToGridView();
    }

    // 網格視圖按鈕事件
    if (gridViewBtn) {
        gridViewBtn.addEventListener('click', function () {
            switchToGridView();
            localStorage.setItem('albumViewMode', 'grid');
        });
    }

    // 條列視圖按鈕事件
    if (listViewBtn) {
        listViewBtn.addEventListener('click', function () {
            switchToListView();
            localStorage.setItem('albumViewMode', 'list');
        });
    }

    function switchToGridView() {
        if (!productContainer) return;

        productContainer.classList.remove('view-mode-list');
        productContainer.classList.add('view-mode-grid');

        if (gridViewBtn) {
            gridViewBtn.classList.add('active');
        }
        if (listViewBtn) {
            listViewBtn.classList.remove('active');
        }
    }

    function switchToListView() {
        if (!productContainer) return;

        productContainer.classList.remove('view-mode-grid');
        productContainer.classList.add('view-mode-list');

        if (listViewBtn) {
            listViewBtn.classList.add('active');
        }
        if (gridViewBtn) {
            gridViewBtn.classList.remove('active');
        }
    }
});
