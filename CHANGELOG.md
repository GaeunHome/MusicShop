# 版本歷程

## v1.9.0 — 全面程式碼審查、架構修正與社群登入優化

### 架構修正

- **PasswordHistoryRepository**：新建 `IPasswordHistoryRepository` 與實作，取代 `UserService` 直接注入 `ApplicationDbContext` 的反模式，統一透過 `IUnitOfWork` 存取
- **交易邊界明確化**：`CouponService.MarkCouponAsUsedAsync` / `ReleaseCouponAsync` 加入交易責任歸屬註解，明確標示由 `OrderService` 外層交易統一提交
- **命名空間統一**：Library 層 7 個檔案從大括號語法改為 file-scoped namespace，統一程式碼風格

### 效能優化

- **N+1 查詢修復**：`CouponService` 的 `IssueCouponToAllUsersAsync` 與 `IssueBirthdayCouponsAsync` 從迴圈逐一查詢改為批次查詢（新增 `GetReceivedUserIdsAsync`），由 N+1 次降為 2 次 DB 查詢
- **FK 索引新增**：`ApplicationDbContext` 新增 `CartItem.UserId`、`Order.UserId`、`OrderItem.OrderId`、`UserCoupon.UserId` 四個常用查詢索引
- **AsNoTracking 補齊**：`FeaturedArtistRepository` 唯讀查詢加上 `.AsNoTracking()`
- **Admin 訂單分頁**：後台訂單列表從全量載入改為分頁查詢（新增 `GetOrdersPagedAsync`），防止大量資料時記憶體溢出

### 安全性強化

- **裸 catch 修復**：`EmailValidationHelper` 的 DNS 查詢從裸 `catch` 改為只捕獲 `DnsResponseException`、`OperationCanceledException`、`SocketException`
- **庫存並發控制**：`OrderValidationService.DeductStockAsync` 加入扣庫存前再次檢查，`OrderService` 明確處理 `DbUpdateConcurrencyException` 並回傳使用者友善訊息
- **OrderStatus 驗證**：Admin `OrderController.UpdateStatus` 加入 `Enum.IsDefined` 驗證，防止非法狀態值

### 社群登入（LINE）優化

- **Email/Phone 改為選填**：`EditProfileViewModel` 的 Email 和 PhoneNumber 從 `[Required]` 改為選填，解決 LINE 用戶因無 Email 而無法儲存任何資料的問題
- **新用戶引導**：社群登入新用戶首次登入後自動導向個人資料頁，提示補充資料
- **帳戶首頁提醒**：`AccountIndexViewModel` 新增 `MissingProfileItems`，帳戶首頁顯示「個人資料尚未完整」黃色警示
- **Email 變更安全**：`UpdateProfileAsync` 新增 Email 唯一性檢查與 `EmailConfirmed = false` 重置

### 程式碼品質

- **日誌參數修正**：`CouponService.DeleteCouponAsync` 日誌佔位符與參數順序對調修復
- **庫存邏輯統一**：`WishlistItemViewModel` 和 `AlbumListItemViewModel` 硬編碼庫存閾值改用 `StockExtensions` 方法（新增 `GetStockBadgeClass`、`GetAdminStockDisplayText`、`GetStockStatusShortText`）
- **折扣公式統一**：3 個 Coupon ViewModel 重複的 `(100 - DiscountValue) / 10m` 公式抽取為 `PriceExtensions.ToTaiwanDiscount()`
- **Admin 訂單每頁筆數**：`DisplayConstants` 新增 `AdminOrderPageSize = 20`
