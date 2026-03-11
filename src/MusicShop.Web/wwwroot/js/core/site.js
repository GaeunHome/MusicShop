// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

// 回到頂端按鈕功能
window.addEventListener('scroll', function() {
    const backToTopButton = document.getElementById('backToTop');
    if (backToTopButton) {
        if (window.pageYOffset > 300) {
            backToTopButton.classList.add('show');
        } else {
            backToTopButton.classList.remove('show');
        }
    }
});

function scrollToTop() {
    window.scrollTo({
        top: 0,
        behavior: 'smooth'
    });
}

// 用戶下拉選單
document.addEventListener('DOMContentLoaded', function() {
    const userMenuToggle = document.getElementById('userMenuToggle');
    const userDropdownMenu = document.getElementById('userDropdownMenu');

    if (userMenuToggle && userDropdownMenu) {
        userMenuToggle.addEventListener('click', function(e) {
            e.stopPropagation();
            userDropdownMenu.classList.toggle('show');
        });

        // 點擊外部關閉選單
        document.addEventListener('click', function(e) {
            if (!userMenuToggle.contains(e.target) && !userDropdownMenu.contains(e.target)) {
                userDropdownMenu.classList.remove('show');
            }
        });
    }

    // 搜尋彈出框
    const searchToggle = document.getElementById('searchToggle');
    const searchOverlay = document.getElementById('searchOverlay');
    const searchClose = document.getElementById('searchClose');

    if (searchToggle && searchOverlay) {
        searchToggle.addEventListener('click', function() {
            searchOverlay.classList.add('show');
            // 聚焦搜尋輸入框
            const searchInput = searchOverlay.querySelector('.search-input');
            if (searchInput) {
                setTimeout(() => searchInput.focus(), 100);
            }
        });
    }

    if (searchClose && searchOverlay) {
        searchClose.addEventListener('click', function() {
            searchOverlay.classList.remove('show');
        });
    }

    if (searchOverlay) {
        searchOverlay.addEventListener('click', function(e) {
            if (e.target === searchOverlay) {
                searchOverlay.classList.remove('show');
            }
        });
    }

    // ESC 鍵關閉搜尋框
    document.addEventListener('keydown', function(e) {
        if (e.key === 'Escape' && searchOverlay && searchOverlay.classList.contains('show')) {
            searchOverlay.classList.remove('show');
        }
    });

    // 手機版導覽列子選單展開／收合
    document.querySelectorAll('[data-nav-mobile-toggle]').forEach(function(btn) {
        btn.addEventListener('click', function(e) {
            e.stopPropagation();
            var li = btn.closest('li');
            var isOpen = li.classList.contains('mobile-open');

            // 關閉所有已展開的項目
            document.querySelectorAll('#mainNavCollapse .nav-menu-item.mobile-open').forEach(function(openLi) {
                openLi.classList.remove('mobile-open');
            });

            // 若原本是關閉的，才展開；已開著的就讓它維持關閉
            if (!isOpen) {
                li.classList.add('mobile-open');
            }
        });
    });
});
