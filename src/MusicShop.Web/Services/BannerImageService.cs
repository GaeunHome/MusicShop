using MusicShop.Web.Services.Interfaces;

namespace MusicShop.Web.Services
{
    /// <summary>
    /// 幻燈片圖片上傳服務實作
    /// 圖片統一儲存至 wwwroot/images/banners/
    /// </summary>
    public class BannerImageService : IBannerImageService
    {
        private readonly IWebHostEnvironment _env;

        private static readonly string[] AllowedExtensions = [".jpg", ".jpeg", ".png", ".webp"];
        private const long MaxFileSizeBytes = 10 * 1024 * 1024; // 10 MB

        public BannerImageService(IWebHostEnvironment env)
        {
            _env = env;
        }

        /// <inheritdoc />
        /// <inheritdoc />
        public void DeleteBannerImage(string? imageUrl)
        {
            if (string.IsNullOrEmpty(imageUrl)) return;

            // imageUrl 格式：/images/banners/banner-1.jpg
            var relativePath = imageUrl.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
            var fullPath = Path.Combine(_env.WebRootPath, relativePath);

            if (File.Exists(fullPath))
                File.Delete(fullPath);
        }

        public async Task<string?> SaveBannerImageAsync(IFormFile? file, int bannerId, string? existingUrl = null)
        {
            if (file == null || file.Length == 0)
                return existingUrl;

            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!AllowedExtensions.Contains(ext))
                throw new InvalidOperationException("不支援的圖片格式，請上傳 JPG、PNG 或 WebP。");

            if (file.Length > MaxFileSizeBytes)
                throw new InvalidOperationException("圖片大小不可超過 10MB。");

            var uploadDir = Path.Combine(_env.WebRootPath, "images", "banners");
            Directory.CreateDirectory(uploadDir);

            var fileName = $"banner-{bannerId}{ext}";
            var filePath = Path.Combine(uploadDir, fileName);

            await using var stream = new FileStream(filePath, FileMode.Create);
            await file.CopyToAsync(stream);

            return $"/images/banners/{fileName}";
        }
    }
}
