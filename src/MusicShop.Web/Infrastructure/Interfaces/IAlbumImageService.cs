using Microsoft.AspNetCore.Http;

namespace MusicShop.Web.Infrastructure.Interfaces;

/// <summary>
/// 商品圖片上傳介面（Web 層基礎設施）
///
/// 【放在 Web 層的原因】
/// 此介面依賴 IFormFile（HTTP 上傳檔案）與 IWebHostEnvironment（wwwroot 路徑），
/// 均為 ASP.NET Core Web 專屬元件，不適合放入商業邏輯層（MusicShop.Service）。
/// 放在 Infrastructure 資料夾以區別於商業邏輯 Service。
/// </summary>
public interface IAlbumImageService
{
    /// <summary>
    /// 依「商品類型（父）/ 藝人 / 商品ID」組成圖片子目錄路徑
    /// </summary>
    Task<string> BuildSubFolderAsync(int? productTypeId, int? artistId, int albumId);

    /// <summary>
    /// 儲存圖片至指定子目錄，回傳相對 URL。
    /// 若未上傳新檔案則回傳 existingUrl。
    /// </summary>
    Task<string?> SaveImageAsync(IFormFile? file, string subFolder, string filePrefix, string? existingUrl = null);
}
