using System.ComponentModel.DataAnnotations;
using MusicShop.Models;

namespace MusicShop.ViewModels.Cart
{
    /// <summary>
    /// 結帳頁面 ViewModel
    /// 包含完整的訂單資訊驗證
    /// </summary>
    public class CheckoutViewModel
    {
        // ==================== 收件人資訊 ====================
        [Required(ErrorMessage = "請輸入收件人姓名")]
        [StringLength(100, ErrorMessage = "收件人姓名不可超過 100 個字元")]
        [Display(Name = "收件人姓名")]
        public string ReceiverName { get; set; } = string.Empty;

        [Required(ErrorMessage = "請輸入收件人電話")]
        [StringLength(20, ErrorMessage = "電話號碼不可超過 20 個字元")]
        [RegularExpression(@"^09\d{8}$|^\d{2,4}-\d{6,8}$", ErrorMessage = "請輸入有效的台灣電話號碼（手機：09XXXXXXXX 或 市話：XX-XXXXXXXX）")]
        [Display(Name = "收件人電話")]
        public string ReceiverPhone { get; set; } = string.Empty;

        // ==================== 收件地址 ====================
        [Required(ErrorMessage = "請選擇縣市")]
        [StringLength(50)]
        [Display(Name = "縣市")]
        public string City { get; set; } = string.Empty;

        [Required(ErrorMessage = "請選擇鄉鎮市區")]
        [StringLength(50)]
        [Display(Name = "鄉鎮市區")]
        public string District { get; set; } = string.Empty;

        [Required(ErrorMessage = "請輸入郵遞區號")]
        [StringLength(10)]
        [RegularExpression(@"^\d{3,5}$", ErrorMessage = "郵遞區號格式不正確")]
        [Display(Name = "郵遞區號")]
        public string PostalCode { get; set; } = string.Empty;

        [Required(ErrorMessage = "請輸入詳細地址")]
        [StringLength(500, ErrorMessage = "詳細地址不可超過 500 個字元")]
        [Display(Name = "詳細地址")]
        public string Address { get; set; } = string.Empty;

        // ==================== 配送資訊 ====================
        [Required(ErrorMessage = "請選擇配送方式")]
        [Display(Name = "配送方式")]
        public DeliveryMethod DeliveryMethod { get; set; }

        // 7-11 門市資訊
        [StringLength(50)]
        [Display(Name = "7-11 門市代號")]
        public string? SevenElevenStoreCode { get; set; }

        [StringLength(200)]
        [Display(Name = "7-11 門市名稱")]
        public string? SevenElevenStoreName { get; set; }

        [StringLength(500)]
        [Display(Name = "7-11 門市地址")]
        public string? SevenElevenStoreAddress { get; set; }

        // 全家門市資訊
        [StringLength(50)]
        [Display(Name = "全家門市代號")]
        public string? FamilyMartStoreCode { get; set; }

        [StringLength(200)]
        [Display(Name = "全家門市名稱")]
        public string? FamilyMartStoreName { get; set; }

        [StringLength(500)]
        [Display(Name = "全家門市地址")]
        public string? FamilyMartStoreAddress { get; set; }

        // ==================== 付款資訊 ====================
        [Required(ErrorMessage = "請選擇付款方式")]
        [Display(Name = "付款方式")]
        public PaymentMethod PaymentMethod { get; set; }

        // ==================== 發票資訊 ====================
        [Required(ErrorMessage = "請選擇發票類型")]
        [Display(Name = "發票類型")]
        public InvoiceType InvoiceType { get; set; }

        // 三聯式發票欄位
        [StringLength(8)]
        [RegularExpression(@"^\d{8}$", ErrorMessage = "統一編號必須為 8 位數字")]
        [Display(Name = "統一編號")]
        public string? CompanyTaxId { get; set; }

        [StringLength(200, ErrorMessage = "公司抬頭不可超過 200 個字元")]
        [Display(Name = "公司抬頭")]
        public string? CompanyName { get; set; }

        // 電子發票載具
        [StringLength(100, ErrorMessage = "載具號碼不可超過 100 個字元")]
        [Display(Name = "電子發票載具")]
        public string? InvoiceCarrier { get; set; }

        // ==================== 其他資訊 ====================
        [StringLength(1000, ErrorMessage = "訂單備註不可超過 1000 個字元")]
        [Display(Name = "訂單備註")]
        public string? OrderNote { get; set; }

        // ==================== 購物車項目（用於顯示訂單摘要）====================
        public List<CartItem> CartItems { get; set; } = new();

        // 總金額
        public decimal TotalAmount { get; set; }

        // ==================== 驗證方法 ====================

        /// <summary>
        /// 自訂驗證：超商取貨時必須選擇門市
        /// </summary>
        public bool IsStoreInfoValid()
        {
            if (DeliveryMethod == DeliveryMethod.SevenEleven)
            {
                return !string.IsNullOrWhiteSpace(SevenElevenStoreCode)
                    && !string.IsNullOrWhiteSpace(SevenElevenStoreName)
                    && !string.IsNullOrWhiteSpace(SevenElevenStoreAddress);
            }
            else if (DeliveryMethod == DeliveryMethod.FamilyMart)
            {
                return !string.IsNullOrWhiteSpace(FamilyMartStoreCode)
                    && !string.IsNullOrWhiteSpace(FamilyMartStoreName)
                    && !string.IsNullOrWhiteSpace(FamilyMartStoreAddress);
            }
            return true;
        }

        /// <summary>
        /// 自訂驗證：三聯式發票時必須填寫統編和抬頭
        /// </summary>
        public bool IsTriplicateInvoiceValid()
        {
            if (InvoiceType == InvoiceType.Triplicate)
            {
                return !string.IsNullOrWhiteSpace(CompanyTaxId)
                    && !string.IsNullOrWhiteSpace(CompanyName);
            }
            return true;
        }

        /// <summary>
        /// 自訂驗證：電子發票時必須填寫載具
        /// </summary>
        public bool IsEInvoiceValid()
        {
            if (InvoiceType == InvoiceType.EInvoice)
            {
                return !string.IsNullOrWhiteSpace(InvoiceCarrier);
            }
            return true;
        }

        /// <summary>
        /// 取得配送方式顯示文字
        /// </summary>
        public string GetDeliveryMethodText()
        {
            return Helpers.EnumHelper.GetDeliveryMethodText(DeliveryMethod);
        }

        /// <summary>
        /// 取得付款方式顯示文字
        /// </summary>
        public string GetPaymentMethodText()
        {
            return Helpers.EnumHelper.GetPaymentMethodText(PaymentMethod);
        }

        /// <summary>
        /// 取得發票類型顯示文字
        /// </summary>
        public string GetInvoiceTypeText()
        {
            return Helpers.EnumHelper.GetInvoiceTypeText(InvoiceType);
        }
    }
}
