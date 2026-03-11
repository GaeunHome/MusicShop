using Microsoft.AspNetCore.Http;

namespace MusicShop.Web.Services.Interfaces
{
    /// <summary>
    /// 幻燈片圖片上傳服務介面（Web 層基礎設施）
    /// </summary>
    public interface IBannerImageService
    {
        /// <summary>
        /// 儲存幻燈片圖片至 wwwroot/images/banners/，回傳相對 URL。
        /// 若未上傳新檔案則回傳 existingUrl。
        /// </summary>
        Task<string?> SaveBannerImageAsync(IFormFile? file, int bannerId, string? existingUrl = null);

        /// <summary>
        /// 刪除幻燈片圖片實體檔案（若存在）
        /// </summary>
        void DeleteBannerImage(string? imageUrl);
    }
}
