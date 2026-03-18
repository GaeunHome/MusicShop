/**
 * ArtistAdmin - 藝人管理模組
 * 負責後台藝人管理相關的前端邏輯
 */
const ArtistAdmin = {
    /**
     * 初始化（列表頁面）
     */
    initList: function() {
        // 未來可加入：搜尋、排序、篩選等功能
    }
};

// 全域導出
if (typeof window !== 'undefined') {
    window.ArtistAdmin = ArtistAdmin;
}
