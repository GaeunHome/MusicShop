using Microsoft.Extensions.Logging;
using MusicShop.Data.UnitOfWork;
using MusicShop.Service.Services.Interfaces;

namespace MusicShop.Service.Services.Implementation
{
    /// <summary>
    /// 統計資訊商業邏輯實作
    /// </summary>
    public class StatisticsService : IStatisticsService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<StatisticsService> _logger;

        public StatisticsService(
            IUnitOfWork unitOfWork,
            ILogger<StatisticsService> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<int> GetAlbumCountAsync()
        {
            var count = await _unitOfWork.Statistics.GetAlbumCountAsync();

            // 商業邏輯：如果數量為 0，記錄警告
            if (count == 0)
            {
                _logger.LogWarning("系統中沒有任何專輯資料");
            }
            else
            {
                _logger.LogInformation("系統中共有 {AlbumCount} 個專輯", count);
            }

            return count;
        }

        public async Task<int> GetCategoryCountAsync()
        {
            var count = await _unitOfWork.Statistics.GetCategoryCountAsync();

            // 商業邏輯：如果分類數量為 0，記錄警告
            if (count == 0)
            {
                _logger.LogWarning("系統中沒有任何分類資料");
            }
            else
            {
                _logger.LogInformation("系統中共有 {CategoryCount} 個分類", count);
            }

            return count;
        }

        public async Task<int> GetOrderCountAsync()
        {
            var count = await _unitOfWork.Statistics.GetOrderCountAsync();

            // 商業邏輯：記錄訂單總數資訊
            if (count == 0)
            {
                _logger.LogInformation("系統中尚無訂單");
            }
            else
            {
                _logger.LogInformation("系統中共有 {OrderCount} 筆訂單", count);
            }

            return count;
        }

        public async Task<int> GetUserCountAsync()
        {
            var count = await _unitOfWork.Statistics.GetUserCountAsync();

            // 商業邏輯：記錄使用者數量，少於 5 人時提示
            if (count < 5)
            {
                _logger.LogInformation("系統中有 {UserCount} 位使用者（使用者數量較少）", count);
            }
            else
            {
                _logger.LogInformation("系統中共有 {UserCount} 位使用者", count);
            }

            return count;
        }

        public async Task<decimal> GetTotalSalesAsync()
        {
            var totalSales = await _unitOfWork.Statistics.GetTotalSalesAsync();

            // 商業邏輯：記錄總銷售額，如果為 0 記錄警告
            if (totalSales == 0)
            {
                _logger.LogWarning("系統總銷售額為 0，尚未產生任何收益");
            }
            else
            {
                _logger.LogInformation("系統總銷售額：NT$ {TotalSales:N2}", totalSales);
            }

            return totalSales;
        }

        public async Task<int> GetPendingOrderCountAsync()
        {
            var count = await _unitOfWork.Statistics.GetPendingOrderCountAsync();

            // 商業邏輯：如果待處理訂單過多（超過 10 筆），記錄警告
            if (count > 10)
            {
                _logger.LogWarning("待處理訂單數量過多：{PendingOrderCount} 筆，請盡快處理", count);
            }
            else if (count > 0)
            {
                _logger.LogInformation("目前有 {PendingOrderCount} 筆待處理訂單", count);
            }

            return count;
        }

        public async Task<int> GetArtistCountAsync()
        {
            var count = await _unitOfWork.Statistics.GetArtistCountAsync();

            // 商業邏輯：如果藝人數量為 0，記錄警告
            if (count == 0)
            {
                _logger.LogWarning("系統中沒有任何藝人資料");
            }
            else
            {
                _logger.LogInformation("系統中共有 {ArtistCount} 位藝人", count);
            }

            return count;
        }
    }
}
