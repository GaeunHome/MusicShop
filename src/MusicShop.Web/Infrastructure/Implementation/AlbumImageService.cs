using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Http;
using MusicShop.Service.Services.Interfaces;
namespace MusicShop.Web.Infrastructure;

/// <summary>
/// 商品圖片上傳實作（Web 層基礎設施）
///
/// 【職責】
/// 處理 HTTP 上傳的圖片檔案，儲存至 wwwroot/images/albums/ 並回傳相對 URL。
/// 此類別屬於 Web 層基礎設施，不包含商業邏輯，故放在 Infrastructure 而非 MusicShop.Service。
/// </summary>
public class AlbumImageService : IAlbumImageService
{
    private readonly IWebHostEnvironment _env;
    private readonly IProductTypeService _productTypeService;
    private readonly IArtistService _artistService;

    // 允許 GIF 是因為部分專輯封面使用動態圖（如限定版動態封面），
    // 而 WebP 兼顧品質與檔案大小，適合網頁商品圖展示
    private static readonly string[] AllowedExtensions = [".jpg", ".jpeg", ".png", ".webp", ".gif"];
    private const long MaxFileSizeBytes = 5 * 1024 * 1024; // 5 MB

    public AlbumImageService(
        IWebHostEnvironment env,
        IProductTypeService productTypeService,
        IArtistService artistService)
    {
        _env = env;
        _productTypeService = productTypeService;
        _artistService = artistService;
    }

    /// <inheritdoc />
    public async Task<string> BuildSubFolderAsync(int? productTypeId, int? artistId, int albumId)
    {
        string typePart = "general";
        if (productTypeId.HasValue)
        {
            var productType = await _productTypeService.GetProductTypeByIdAsync(productTypeId.Value);
            if (productType != null)
            {
                // 子分類使用父分類名稱作為資料夾，確保同一大類的商品圖片
                // 歸檔於同一目錄（例如 K-ALBUM/artist/123），避免子分類改名
                // 導致路徑變動而產生孤立檔案
                var nameSource = productType.ParentId.HasValue
                    ? await _productTypeService.GetProductTypeByIdAsync(productType.ParentId.Value)
                    : productType;

                if (nameSource != null)
                    typePart = Sanitize(nameSource.Name);
            }
        }

        string artistPart = "unknown";
        if (artistId.HasValue)
        {
            var artistName = await _artistService.GetArtistNameByIdAsync(artistId.Value);
            if (artistName != null)
                artistPart = Sanitize(artistName);
        }

        return Path.Combine(typePart, artistPart, albumId.ToString());
    }

    /// <inheritdoc />
    public async Task<string?> SaveImageAsync(IFormFile? file, string subFolder, string filePrefix, string? existingUrl = null)
    {
        if (file == null || file.Length == 0)
            return existingUrl;

        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!AllowedExtensions.Contains(ext))
            throw new InvalidOperationException("不支援的圖片格式，請上傳 JPG、PNG、WebP 或 GIF。");

        if (file.Length > MaxFileSizeBytes)
            throw new InvalidOperationException("圖片大小不可超過 5MB。");

        var uploadDir = Path.Combine(_env.WebRootPath, "images", "albums", subFolder);
        Directory.CreateDirectory(uploadDir);

        var fileName = $"{filePrefix}{ext}";
        var filePath = Path.Combine(uploadDir, fileName);

        await using var stream = new FileStream(filePath, FileMode.Create);
        await file.CopyToAsync(stream);

        var urlPath = subFolder.Replace(Path.DirectorySeparatorChar, '/');
        return $"/images/albums/{urlPath}/{fileName}";
    }

    /// <summary>
    /// 將字串轉為安全的資料夾名稱（小寫、空白轉連字號、移除特殊字元）
    /// </summary>
    private static string Sanitize(string name)
    {
        var safe = Regex.Replace(name.ToLowerInvariant(), @"[^\w\-]", "-");
        safe = Regex.Replace(safe, @"-{2,}", "-").Trim('-');
        return string.IsNullOrEmpty(safe) ? "unnamed" : safe;
    }
}
