using Microsoft.Extensions.Logging;
using MusicShop.Data.UnitOfWork;
using MusicShop.Service.Services.Interfaces;
using MusicShop.Service.ViewModels.Admin;

namespace MusicShop.Service.Services.Implementation
{
    /// <summary>
    /// 統計資訊商業邏輯實作
    /// </summary>
    public class StatisticsService : IStatisticsService
    {
        /// <summary>
        /// 待處理訂單超過此值時記錄警告。
        /// 門檻值 10 是基於人工處理訂單的合理上限——超過此數量表示出貨流程可能積壓，
        /// 需要管理員及時介入處理，避免客戶等待過久。
        /// </summary>
        private const int HighPendingOrderThreshold = 10;

        private readonly IUnitOfWork _unitOfWork;
        private readonly ISystemSettingService _systemSettingService;
        private readonly ILogger<StatisticsService> _logger;

        public StatisticsService(
            IUnitOfWork unitOfWork,
            ISystemSettingService systemSettingService,
            ILogger<StatisticsService> logger)
        {
            _unitOfWork = unitOfWork;
            _systemSettingService = systemSettingService;
            _logger = logger;
        }

        public async Task<DashboardStatsViewModel> GetDashboardStatsAsync()
        {
            var stats = new DashboardStatsViewModel
            {
                AlbumCount = await _unitOfWork.Statistics.GetAlbumCountAsync(),
                ArtistCount = await _unitOfWork.Statistics.GetArtistCountAsync(),
                CategoryCount = await _unitOfWork.Statistics.GetCategoryCountAsync(),
                OrderCount = await _unitOfWork.Statistics.GetOrderCountAsync(),
                UserCount = await _unitOfWork.Statistics.GetUserCountAsync(),
                TotalSales = await _unitOfWork.Statistics.GetTotalSalesAsync(),
                PendingOrderCount = await _unitOfWork.Statistics.GetPendingOrderCountAsync(),
                BannerCount = await _unitOfWork.Statistics.GetBannerCountAsync(),
                FeaturedArtistCount = await _unitOfWork.Statistics.GetFeaturedArtistCountAsync(),
                CouponCount = await _unitOfWork.Statistics.GetCouponCountAsync()
            };

            // 從系統參數讀取門檻值，若未設定則使用預設常數
            var thresholdStr = await _systemSettingService.GetValueAsync("order.high_pending_threshold");
            var threshold = int.TryParse(thresholdStr, out var t) ? t : HighPendingOrderThreshold;

            // 僅記錄有業務價值的警告，避免每次 Dashboard 載入都產生大量日誌
            if (stats.PendingOrderCount > threshold)
            {
                _logger.LogWarning("待處理訂單數量過多：{PendingOrderCount} 筆，請盡快處理",
                    stats.PendingOrderCount);
            }

            return stats;
        }

        public async Task<int> GetAlbumCountAsync()
            => await _unitOfWork.Statistics.GetAlbumCountAsync();

        public async Task<int> GetCategoryCountAsync()
            => await _unitOfWork.Statistics.GetCategoryCountAsync();

        public async Task<int> GetOrderCountAsync()
            => await _unitOfWork.Statistics.GetOrderCountAsync();

        public async Task<int> GetUserCountAsync()
            => await _unitOfWork.Statistics.GetUserCountAsync();

        public async Task<decimal> GetTotalSalesAsync()
            => await _unitOfWork.Statistics.GetTotalSalesAsync();

        public async Task<int> GetPendingOrderCountAsync()
            => await _unitOfWork.Statistics.GetPendingOrderCountAsync();

        public async Task<int> GetArtistCountAsync()
            => await _unitOfWork.Statistics.GetArtistCountAsync();

        public async Task<int> GetBannerCountAsync()
            => await _unitOfWork.Statistics.GetBannerCountAsync();

        public async Task<int> GetFeaturedArtistCountAsync()
            => await _unitOfWork.Statistics.GetFeaturedArtistCountAsync();

        public async Task<int> GetCouponCountAsync()
            => await _unitOfWork.Statistics.GetCouponCountAsync();

        public async Task<List<(DateTime Date, decimal Amount, int Count)>> GetDailySalesTrendAsync(int days = 30)
        {
            return await _unitOfWork.Statistics.GetDailySalesTrendAsync(days);
        }

        public async Task<List<(string AlbumTitle, int Quantity)>> GetTopSellingAlbumsAsync(int count = 10)
        {
            return await _unitOfWork.Statistics.GetTopSellingAlbumsAsync(count);
        }
    }
}
