/**
 * 搜尋即時建議功能
 * 使用者在搜尋框輸入時，透過 API 取得建議清單並顯示下拉選單
 *
 * API 端點：GET /api/album/suggestions?q=關鍵字
 * 依賴：AjaxHelper (ajax-helper.js)
 */
(function () {
    'use strict';

    var searchInput = document.querySelector('.search-input');
    if (!searchInput) return;

    var debounceTimer = null;
    var suggestionsContainer = null;

    // 建立建議清單容器
    function createContainer() {
        suggestionsContainer = document.createElement('div');
        suggestionsContainer.className = 'search-suggestions';
        suggestionsContainer.style.display = 'none';
        searchInput.parentNode.style.position = 'relative';
        searchInput.parentNode.appendChild(suggestionsContainer);
    }

    createContainer();

    // 監聽輸入事件（防抖：停止輸入 300ms 後才發送請求）
    searchInput.addEventListener('input', function () {
        var query = this.value.trim();

        clearTimeout(debounceTimer);

        if (query.length < 1) {
            hideSuggestions();
            return;
        }

        debounceTimer = setTimeout(function () {
            fetchSuggestions(query);
        }, 300);
    });

    // 呼叫 API 取得建議
    async function fetchSuggestions(query) {
        try {
            var data = await AjaxHelper.get('/api/album/suggestions', { q: query });

            if (!Array.isArray(data) || data.length === 0) {
                hideSuggestions();
                return;
            }

            renderSuggestions(data, query);
        } catch (err) {
            hideSuggestions();
        }
    }

    // HTML 特殊字元跳脫（防止 XSS）
    function escapeHtml(str) {
        if (!str) return '';
        var div = document.createElement('div');
        div.appendChild(document.createTextNode(str));
        return div.innerHTML;
    }

    // 渲染建議清單
    function renderSuggestions(items, query) {
        var html = items.map(function (item) {
            // 先跳脫 HTML，再套用關鍵字高亮
            var highlightedTitle = highlightMatch(escapeHtml(item.title), query);
            var highlightedArtist = item.artistName ? highlightMatch(escapeHtml(item.artistName), query) : '';
            var safeImageUrl = escapeHtml(item.coverImageUrl);
            var safeTitle = escapeHtml(item.title);
            var safeId = parseInt(item.id, 10) || 0;

            return '<a href="/Album/Detail/' + safeId + '" class="search-suggestion-item">'
                + '<div class="suggestion-image">'
                + (item.coverImageUrl
                    ? '<img src="' + safeImageUrl + '" alt="' + safeTitle + '" />'
                    : '<div class="suggestion-no-image"><i class="bi bi-music-note"></i></div>')
                + '</div>'
                + '<div class="suggestion-info">'
                + '<div class="suggestion-title">' + highlightedTitle + '</div>'
                + (highlightedArtist ? '<div class="suggestion-artist">' + highlightedArtist + '</div>' : '')
                + '<div class="suggestion-price">NT$ ' + escapeHtml(String(item.price)) + '</div>'
                + '</div>'
                + '</a>';
        }).join('');

        // 加上「查看全部結果」連結
        html += '<a href="/Album?search=' + encodeURIComponent(query) + '" class="search-suggestion-all">'
            + '查看「' + escapeHtml(query) + '」的所有結果 →'
            + '</a>';

        suggestionsContainer.innerHTML = html;
        suggestionsContainer.style.display = 'block';
    }

    // 關鍵字高亮（不區分大小寫，輸入已經過 escapeHtml 處理）
    function highlightMatch(text, query) {
        if (!query) return text;
        var escapedQuery = escapeHtml(query);
        var regex = new RegExp('(' + escapedQuery.replace(/[.*+?^${}()|[\]\\]/g, '\\$&') + ')', 'gi');
        return text.replace(regex, '<mark>$1</mark>');
    }

    // 隱藏建議清單
    function hideSuggestions() {
        if (suggestionsContainer) {
            suggestionsContainer.style.display = 'none';
            suggestionsContainer.innerHTML = '';
        }
    }

    // 點擊建議清單外部時關閉
    document.addEventListener('click', function (e) {
        if (!e.target.closest('.search-form')) {
            hideSuggestions();
        }
    });

    // 按 Escape 關閉建議
    searchInput.addEventListener('keydown', function (e) {
        if (e.key === 'Escape') {
            hideSuggestions();
        }
    });
})();
