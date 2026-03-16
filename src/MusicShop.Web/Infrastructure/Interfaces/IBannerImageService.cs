using Microsoft.AspNetCore.Http;

namespace MusicShop.Web.Infrastructure.Interfaces;

/// <summary>
/// 幻燈片圖片上傳介面（Web 層基礎設施）
///
/// 【放在 Web 層的原因】
/// 此介面依賴 IFormFile（HTTP 上傳檔案）與 IWebHostEnvironment（wwwroot 路徑），
/// 均為 ASP.NET Core Web 專屬元件，不適合放入商業邏輯層（MusicShop.Service）。
/// 放在 Infrastructure 資料夾以區別於商業邏輯 Service。
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
