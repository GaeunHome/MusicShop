using Microsoft.AspNetCore.Http;

namespace MusicShop.Web.Services.Interfaces;

/// <summary>
/// 商品圖片上傳服務介面（Web 層基礎設施）
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
