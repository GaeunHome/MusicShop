using MusicShop.Repositories.Interface;
using MusicShop.Services.Interface;

namespace MusicShop.Services.Implementation
{
    /// <summary>
    /// 統計資訊商業邏輯實作
    /// </summary>
    public class StatisticsService : IStatisticsService
    {
        private readonly IStatisticsRepository _statisticsRepository;

        public StatisticsService(IStatisticsRepository statisticsRepository)
        {
            _statisticsRepository = statisticsRepository;
        }

        public async Task<int> GetAlbumCountAsync()
        {
            return await _statisticsRepository.GetAlbumCountAsync();
        }

        public async Task<int> GetCategoryCountAsync()
        {
            return await _statisticsRepository.GetCategoryCountAsync();
        }

        public async Task<int> GetOrderCountAsync()
        {
            return await _statisticsRepository.GetOrderCountAsync();
        }

        public async Task<int> GetUserCountAsync()
        {
            return await _statisticsRepository.GetUserCountAsync();
        }

        public async Task<decimal> GetTotalSalesAsync()
        {
            return await _statisticsRepository.GetTotalSalesAsync();
        }

        public async Task<int> GetPendingOrderCountAsync()
        {
            return await _statisticsRepository.GetPendingOrderCountAsync();
        }
    }
}
