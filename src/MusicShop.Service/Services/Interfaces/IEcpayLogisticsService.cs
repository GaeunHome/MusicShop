namespace MusicShop.Service.Services.Interfaces
{
    /// <summary>
    /// ECPay 物流服務介面
    /// 負責與綠界超商門市 API 介接
    /// </summary>
    public interface IEcpayLogisticsService
    {
        /// <summary>
        /// 取得超商門市清單
        /// </summary>
        /// <param name="cvsType">超商類型：UNIMART（7-11）、FAMI（全家）、HILIFE（萊爾富）、OKMART（OK Mart）</param>
        /// <returns>門市清單</returns>
        Task<IEnumerable<EcpayStoreInfo>> GetStoreListAsync(string cvsType);
    }

    /// <summary>
    /// ECPay 門市資訊
    /// </summary>
    public class EcpayStoreInfo
    {
        public string StoreId { get; set; } = string.Empty;
        public string StoreName { get; set; } = string.Empty;
        public string StoreAddr { get; set; } = string.Empty;
        public string StorePhone { get; set; } = string.Empty;
    }
}
