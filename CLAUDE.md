# CLAUDE.md

此檔案為 Claude Code (claude.ai/code) 在此專案中工作時的指引文件。

## 專案概述

MusicShop 是一個基於 ASP.NET Core MVC 的線上音樂專輯商店網站應用程式。使用 .NET 10.0 建構，具備使用者認證、專輯瀏覽、購物車和訂單管理功能。

## 建置與執行指令

```bash
# 還原相依套件
dotnet restore

# 建置專案
dotnet build

# 執行應用程式（預設：http://localhost:5281）
dotnet run

# 建置正式版本
dotnet build --configuration Release
```

## 資料庫指令

本專案使用 Entity Framework Core 搭配 SQL Server。

```bash
# 新增遷移
dotnet ef migrations add <MigrationName>

# 更新資料庫
dotnet ef database update

# 移除最後一次遷移（若尚未套用）
dotnet ef migrations remove

# 刪除資料庫
dotnet ef database drop
```

**資料庫連線**：設定於 `appsettings.json` 的 `ConnectionStrings:DefaultConnection`。
- 使用範例檔案 `appsettings.example.json` 作為參考
- `appsettings.json` 已加入 `.gitignore`，不會提交到版控
- 請依據實際環境修改 Server、Database、User Id 和 Password

## 專案結構

本專案採用**多專案解決方案（Multi-Project Solution）**架構，將應用程式分為 4 個獨立專案：

```
MusicShop/
├── src/
│   ├── MusicShop.Data/          # 資料存取層
│   │   ├── Entities/             # 實體模型（AppUser, Album, Order...）
│   │   ├── Repositories/         # Repository 介面與實作
│   │   │   ├── Interfaces/       # Repository 介面定義
│   │   │   └── Implementation/   # Repository 實作
│   │   ├── UnitOfWork/           # UnitOfWork 模式實作
│   │   ├── ApplicationDbContext.cs
│   │   ├── DbInitializer.cs
│   │   └── SeedArtists.cs
│   │
│   ├── MusicShop.Service/        # 商業邏輯層
│   │   ├── Services/
│   │   │   ├── Interfaces/       # Service 介面定義
│   │   │   └── Implementation/   # Service 實作
│   │   ├── ViewModels/           # ViewModel 定義
│   │   └── Mapper/               # AutoMapper 設定
│   │
│   ├── MusicShop.Library/        # 共用工具庫
│   │   ├── Helpers/              # 工具類別（ValidationHelper, OrderHelper...）
│   │   └── Extensions/           # 擴充方法
│   │
│   └── MusicShop.Web/            # 展示層（Web UI）
│       ├── Controllers/          # MVC 控制器
│       ├── Views/                # Razor 視圖
│       ├── ViewComponents/       # View Components
│       ├── wwwroot/              # 靜態資源（CSS, JS, 圖片）
│       ├── Models/               # Web 專屬 Models（ErrorViewModel）
│       └── Program.cs            # 應用程式進入點
```

### 專案相依關係

```
MusicShop.Web
    ├─> MusicShop.Service
    │       ├─> MusicShop.Data
    │       │       └─> Entity Framework Core
    │       └─> MusicShop.Library
    │               └─> MusicShop.Data
    ├─> MusicShop.Data
    └─> MusicShop.Library
```

### 命名空間規範

- `MusicShop.Data.Entities` - 實體模型
- `MusicShop.Data.Repositories.Interfaces` - Repository 介面
- `MusicShop.Data.Repositories.Implementation` - Repository 實作
- `MusicShop.Data.UnitOfWork` - UnitOfWork 介面與實作
- `MusicShop.Service.Services.Interfaces` - Service 介面
- `MusicShop.Service.Services.Implementation` - Service 實作
- `MusicShop.Service.ViewModels` - ViewModel 定義
- `MusicShop.Library.Helpers` - 共用工具類別
- `MusicShop.Web.Controllers` - MVC 控制器
- `MusicShop.Web.Models` - Web 專屬模型

## 架構說明

### 三層式架構（Three-Tier Architecture）

本專案採用標準的三層式架構設計，並透過**多專案解決方案**確保關注點分離：

**展示層（Presentation Layer）** - `MusicShop.Web`
- 負責處理使用者輸入與畫面顯示
- 不包含商業邏輯，僅負責資料傳遞與顯示
- Controllers、Views、ViewComponents、靜態資源

**商業邏輯層（Business Logic Layer）** - `MusicShop.Service`
- 包含所有業務規則與驗證邏輯
- 協調資料存取層與展示層之間的互動
- 介面與實作分離（Interfaces/ 與 Implementation/）
- 使用 UnitOfWork 模式管理資料存取

**資料存取層（Data Access Layer）** - `MusicShop.Data`
- 負責資料庫 CRUD 操作
- 使用 Repository 模式封裝資料存取
- 使用 UnitOfWork 模式管理交易
- 介面與實作分離（Interfaces/ 與 Implementation/）

**共用工具層（Shared Library）** - `MusicShop.Library`
- 提供跨層共用的工具類別和擴充方法
- ValidationHelper、OrderHelper、EnumHelper 等

### 核心元件

**UnitOfWork 模式**（`MusicShop.Data.UnitOfWork`）：
- `IUnitOfWork` 介面：統一管理所有 Repository 的存取點
- `UnitOfWork` 實作：實現 Repository 的集中管理與交易控制
- **優點**：
  - 確保多個 Repository 操作在同一個 DbContext 中執行
  - 簡化依賴注入（Service 只需注入 `IUnitOfWork` 而非多個 Repository）
  - 提供統一的 SaveChanges 控制點
  - 支援交易管理（Transaction）
- **提供的 Repository 屬性**：
  - `Albums` - IAlbumRepository
  - `Artists` - IArtistRepository
  - `ArtistCategories` - IArtistCategoryRepository
  - `ProductTypes` - IProductTypeRepository
  - `Cart` - ICartRepository
  - `Orders` - IOrderRepository
  - `Statistics` - IStatisticsRepository

**Generic Repository 模式**（`MusicShop.Data.Repositories`）：
- `IGenericRepository<T>` 泛型介面：定義通用的 CRUD 操作
- `GenericRepository<T>` 泛型實作：提供基礎的資料存取功能
- **通用方法**：
  - `GetByIdAsync(int id)` - 根據 ID 取得單一實體
  - `GetAllAsync()` - 取得所有實體
  - `CreateAsync(T entity)` - 新增實體
  - `UpdateAsync(T entity)` - 更新實體
  - `DeleteAsync(int id)` - 刪除實體
- **好處**：
  - 減少重複的 CRUD 程式碼
  - 確保資料存取邏輯的一致性
  - 易於維護和擴充

**資料層** (`MusicShop.Data`)：
- `ApplicationDbContext`：繼承自 `IdentityDbContext<AppUser>`，整合 ASP.NET Core Identity
- DbSets：Albums、Artists、ArtistCategories、ProductTypes、Orders、OrderItems、CartItems
- 使用 `IDbContextFactory<ApplicationDbContext>` 提供更好的並行處理能力
- **Repository 介面與實作**：
  - `IAlbumRepository` / `AlbumRepository`：專輯資料存取（繼承自 GenericRepository）
  - `IArtistRepository` / `ArtistRepository`：藝人資料存取（繼承自 GenericRepository）
  - `ICartRepository` / `CartRepository`：購物車資料存取
  - `IOrderRepository` / `OrderRepository`：訂單資料存取
  - `IArtistCategoryRepository` / `ArtistCategoryRepository`：藝人分類資料存取（繼承自 GenericRepository）
  - `IProductTypeRepository` / `ProductTypeRepository`：商品類型資料存取（繼承自 GenericRepository）
  - `IStatisticsRepository` / `StatisticsRepository`：統計資料存取

**實體關聯設定** (`OnModelCreating`)：
- Album-ArtistCategory：一對多關聯，限制刪除（Restrict）
- Album-ProductType：一對多關聯，限制刪除（Restrict）
- ProductType 自我關聯（Parent-Children）：階層式分類結構
- Order-AppUser：一對多關聯，串聯刪除（Cascade）
- OrderItem-Order：一對多關聯，串聯刪除（Cascade）
- OrderItem-Album：一對多關聯，限制刪除（Restrict）
- CartItem-AppUser：一對多關聯，串聯刪除（Cascade）
- CartItem-Album：一對多關聯，限制刪除（Restrict）

**實體模型** (`MusicShop.Data.Entities/`)：
- `AppUser`：擴展 `IdentityUser`，包含 FullName、PhoneNumber、Birthday、Gender、RegisteredAt，以及 Orders 和 CartItems 導航屬性
- `Album`：Title、Artist、Description、Price（decimal）、CoverImageUrl、Stock、RowVersion（並發控制）、ArtistCategoryId、ArtistId、ProductTypeId
- `Artist`：Name、ProfileImageUrl、Description、ArtistCategoryId、DisplayOrder
- `ArtistCategory`：藝人分類（BOY GROUP、GIRL GROUP、SOLO）
- `ProductType`：商品類型（階層式架構，包含 ParentId 實現父子關係）
  - 父分類：K-ALBUM、K-MAGAZINE、K-MERCH、K-EVENT
  - 子分類：ALBUM、PHOTOBOOK、DVD、寫真雜誌、官方周邊等
- `Order`：UserId、OrderDate、Status（列舉：Pending/Paid/Shipped/Completed/Cancelled）、TotalAmount、PaymentMethod、DeliveryMethod、OrderItems 集合
- `OrderItem`：連結 Order 與 Album，包含 Quantity 和 UnitPrice
- `CartItem`：連結 User 與 Album，包含 Quantity 和 AddedAt 時間戳

**服務層** (`MusicShop.Service/`)：
- **依賴注入**：所有 Service 統一注入 `IUnitOfWork`，透過 UnitOfWork 存取 Repository
- **AutoMapper**：使用 AutoMapper 進行 Entity ↔ ViewModel 的物件映射
  - 設定檔：`Mapper/MapperProfile.cs`
  - 映射規則：AppUser ↔ EditProfileViewModel、AppUser → UserManagementViewModel
- **介面** (`Services/Interfaces/`)：
  - `IAlbumService`：專輯業務邏輯介面
  - `IArtistService`：藝人業務邏輯介面
  - `ICartService`：購物車業務邏輯介面
  - `IOrderService`：訂單業務邏輯介面
  - `IOrderValidationService`：訂單驗證業務邏輯介面
  - `IArtistCategoryService`：藝人分類業務邏輯介面
  - `IProductTypeService`：商品類型業務邏輯介面
  - `IStatisticsService`：統計業務邏輯介面
  - `IUserService`：使用者管理業務邏輯介面
- **實作** (`Services/Implementation/`)：
  - 所有 Service 實作類別都透過 `IUnitOfWork` 存取資料層
  - 範例：`AlbumService` 注入 `IUnitOfWork`，透過 `_unitOfWork.Albums` 存取 AlbumRepository
  - 優點：簡化依賴注入、確保資料一致性、支援交易管理

**檢視模型** (`MusicShop.Service/ViewModels/`)：
- **Account**：RegisterViewModel、LoginViewModel、AccountIndexViewModel、EditProfileViewModel
- **Album**：AlbumCardViewModel、AlbumDetailViewModel
- **Cart**：CheckoutViewModel、CartUpdateResult
- **Admin**：UserManagementViewModel

**控制器** (`MusicShop.Web/Controllers/`)：
- `AccountController`：帳號管理（註冊、登入、登出、個人資料編輯、訂單查詢）
- `AlbumController`：專輯瀏覽，支援搜尋與雙分類篩選
- `HomeController`：主要首頁與輪播
- `CartController`：購物車功能（✅ 已完成）
- `OrderController`：訂單管理（✅ 已完成）
- `AdminController`：後台管理（商品、分類、訂單、使用者管理）（✅ 已完成）

**View Components** (`MusicShop.Web/ViewComponents/`)：
- `CartBadgeViewComponent`：購物車數量徽章元件，動態顯示購物車商品數量

**工具類別** (`MusicShop.Library/Helpers/`)：
- `ValidationHelper`（v1.1.1 新增）：集中管理所有驗證邏輯的靜態工具類別
  - `ValidateNotEmpty(string?, string, string)`：驗證字串不為空
  - `ValidateMaxLength(string, string, int, string)`：驗證字串長度
  - `ValidateString(string?, string, int, string)`：綜合字串驗證（不為空 + 長度檢查）
  - `ValidatePositive(decimal, string, string)`：驗證 decimal 值大於 0
  - `ValidatePositive(int, string, string)`：驗證 int 值大於 0
  - `ValidateId(int, string, string)`：驗證 ID 有效性（大於 0）
  - `ValidateEntityExists<T>(T?, string, int)`：驗證實體存在
  - `ValidateCondition(bool, string)`：驗證條件為真
  - `ValidateCollectionNotEmpty<T>(IEnumerable<T>?, string)`：驗證集合不為空
- `OrderHelper`（v1.2.1 新增）：集中管理訂單相關業務邏輯的靜態工具類別
  - `GetPaymentStatusText(Order)`：根據付款方式和訂單狀態取得付款狀態文字
  - `GetDeliveryStatusText(Order)`：取得配送狀態文字
  - `GetOrderStatusText(OrderStatus)`：取得訂單狀態顯示文字
  - `GetOrderStatusDescription(OrderStatus)`：取得訂單狀態說明文字
  - `GetOrderStatusBadgeClass(OrderStatus)`：取得訂單狀態的 Badge CSS 類別
  - `GetPaymentMethodText(PaymentMethod)`：取得付款方式顯示文字
  - `GetDeliveryMethodText(DeliveryMethod)`：取得配送方式顯示文字
  - `GetValidNextStatuses(Order)`：取得訂單的有效下一步狀態選項
  - `CanUpdateStatus(Order)`：判斷訂單是否可以更新狀態
  - 內含 `OrderStatusOption` 類別：封裝訂單狀態選項（用於下拉選單）

### 認證與授權

設定於 `Program.cs`：
- ASP.NET Core Identity 搭配自訂 `AppUser`
- 密碼需求：6 個字元以上、需包含數字、不需大寫/特殊字元
- 登入路徑：`/Account/Login`
- 拒絕存取路徑：`/Account/AccessDenied`
- 認證中介軟體在授權之前啟用（`app.UseAuthentication()` 在 `app.UseAuthorization()` 之前）

**認證實作細節**：
- 使用 `UserManager<AppUser>` 管理使用者（註冊、密碼驗證）
- 使用 `SignInManager<AppUser>` 處理登入/登出（Cookie 認證）
- 密碼使用 PBKDF2 演算法雜湊，儲存於 `AspNetUsers` 資料表的 `PasswordHash` 欄位
- 註冊成功後自動呼叫 `SignInManager.SignInAsync()` 登入
- 登入驗證使用 `SignInManager.PasswordSignInAsync()` 比對密碼雜湊值
- Cookie 預設在瀏覽器關閉後失效，除非勾選「記住我」

### 路由設定

預設路由模式：`{controller=Home}/{action=Index}/{id?}`

## 關鍵模式與最佳實踐

1. **多專案解決方案架構**（v1.3.0 新增）：將應用程式分為 4 個獨立專案，確保關注點分離與模組化
2. **UnitOfWork 模式**（v1.3.0 新增）：統一管理所有 Repository，確保資料一致性與交易完整性
3. **Generic Repository 模式**（v1.3.0 新增）：提供通用的 CRUD 操作介面，減少重複程式碼
4. **三層式架構分離**：嚴格遵守 Controller → Service → Repository 的職責分離原則
5. **依賴注入（Dependency Injection）**：Service 層統一注入 `IUnitOfWork`，降低耦合度
6. **Repository 模式**：資料存取邏輯封裝於 Repository 層，提高可測試性
7. **IDbContextFactory 模式**：使用工廠模式建立 DbContext，避免並行問題
8. **AutoMapper 物件映射**（v1.3.0 新增）：自動化 Entity ↔ ViewModel 的轉換，減少手動映射程式碼
9. **預先載入（Eager Loading）**：使用 `.Include()` 載入關聯實體，避免 N+1 查詢問題
10. **Dictionary 快取**：在 OrderService 中使用字典快取已查詢的專輯，避免重複查詢資料庫
11. **UTC 時間**：統一使用 `DateTime.UtcNow` 避免時區問題
12. **資料註解（Data Annotations）**：使用 `[Required]`、`[StringLength]` 等進行模型驗證
13. **小數精度**：價格欄位使用 `[Column(TypeName = "decimal(10,2)")]`
14. **ViewBag 傳遞資料**：分類清單與搜尋詞透過 ViewBag 傳遞給檢視
15. **View Components**：可重複使用的 UI 元件（如購物車徽章）
16. **ValidationHelper 模式**（v1.1.1 新增）：集中管理所有驗證邏輯，避免重複程式碼
17. **OrderHelper 模式**（v1.2.1 新增）：集中管理訂單顯示邏輯，避免在 View 層撰寫業務邏輯
18. **業務邏輯分層**（v1.2.1 強化）：View 層僅負責顯示，業務邏輯統一放在 Service 或 Helper 層
19. **資料庫交易**（v1.2.0 新增）：訂單建立使用交易確保原子性（建立訂單、扣除庫存、清空購物車）
20. **樂觀並發控制**（v1.2.0 新增）：Album 模型使用 `[Timestamp]` RowVersion 防止並發更新問題
21. **輔助方法萃取**（v1.2.0 新增）：Controller 層萃取重複邏輯為私有方法，提升可讀性與維護性
22. **單一職責原則**（v1.2.0 新增）：Controller 僅負責流程控制，Service 層負責業務驗證與邏輯

## 已完成功能

### ✅ 購物車系統（CartController）
- 加入商品至購物車（含庫存驗證）
- 顯示購物車內容與總金額
- 更新商品數量（含庫存檢查）
- 移除購物車商品
- 清空購物車
- 使用 `[Authorize]` 保護，確保使用者已登入
- 購物車數量徽章（View Component）即時顯示
- 購物車 Modal 彈窗（Ajax 加入購物車）

### ✅ 訂單系統（OrderController）
- 結帳流程（從購物車建立訂單）
- 訂單建立時：
  - **使用資料庫交易確保原子性**（CreateOrderWithTransactionAsync）
  - 扣除專輯庫存（Album.Stock）
  - 建立 Order 記錄（訂單主檔）
  - 建立 OrderItem 記錄（訂單明細）
  - 清空使用者購物車
  - 使用 Dictionary 快取避免 N+1 查詢問題
  - **樂觀並發控制**（Album.RowVersion 防止超賣）
- 訂單查詢（僅能查看自己的訂單）
- 訂單詳細資訊
- 權限控制（無法查看他人訂單）

### ✅ 後台管理（AdminController）
- **專輯管理**：新增、編輯、刪除、列表
- **雙分類管理**：
  - 藝人分類（ArtistCategory）CRUD
  - 商品類型（ProductType）CRUD（支援階層式父子關係）
  - 級聯下拉選單（選擇父分類後動態載入子分類）
- **訂單管理**：查看所有訂單、訂單詳細資訊、更新訂單狀態
- **使用者管理**：查看所有使用者、切換管理員角色、防止自我移除管理員權限
- **統計資訊**：後台儀表板顯示商品數、訂單數、使用者數、總銷售額等統計
- 角色驗證 `[Authorize(Roles = "Admin")]`

### ✅ 使用者個人功能
- 個人帳號總覽（訂單統計、最近訂單）
- 個人資料編輯（姓名、電話、生日、性別）
- 訂單歷史查詢
- 訂單詳細資訊

### ✅ 資料庫初始化（DbInitializer）
- 自動建立系統角色（User、Admin）
- 從 `appsettings.json` 讀取並建立預設管理員帳戶
- 建立預設藝人分類（BOY GROUP、GIRL GROUP、SOLO）
- 建立預設商品類型（階層式：4 個父分類 + 10 個子分類）
- 建立範例商品資料

### ✅ UI/UX 優化（v1.1.1）
- 專輯列表頁面重新設計
  - 現代化卡片式搜尋區域（白色背景、圓角、陰影效果）
  - 搜尋輸入框加入圖示（Bootstrap Icons）
  - 優化的 focus 效果（紫色邊框與陰影）
  - 雙下拉式排序系統（預設排序、最新上架、價格由低到高、價格由高到低）
  - 分離式工具列設計（灰色背景區塊）
  - 商品數量即時顯示
  - 格狀/清單視圖切換按鈕
- 專屬樣式檔案（wwwroot/css/pages/album-index.css）
  - 380+ 行專業 CSS 設計
  - 漸層色彩效果（#b19cd9 主題色）
  - 完整響應式設計（@media 查詢：桌面、平板、手機）
  - 懸停效果與平滑轉場動畫
  - 下拉選單美化（圓角、陰影、hover 效果）

### ✅ 程式碼品質與安全性優化（v1.2.0）
- **資料庫交易保護**
  - 實作 `CreateOrderWithTransactionAsync` 方法
  - 確保訂單建立、庫存扣除、購物車清空的原子性
  - 防止並發訂單導致超賣問題

- **並發控制機制**
  - Album 模型新增 `[Timestamp]` RowVersion 欄位
  - 樂觀並發控制防止庫存更新衝突
  - 資料庫層級的 rowversion 自動檢測並發修改

- **程式碼重構與 DRY 原則**
  - 移除 Controller 層重複驗證邏輯（統一由 Service 層處理）
  - 萃取 `GetAuthorizedUserId()` 輔助方法（消除 8 處重複程式碼）
  - 萃取 `ReturnCheckoutViewWithDataAsync()` 錯誤處理方法
  - 優化購物車總計查詢（避免重複資料庫查詢）

- **UI 色彩優化**
  - 購物車與結帳頁面色彩調整
  - 移除刺眼的藍紫漸層，改用柔和紫色（#b19cd9）
  - 統一配色與導覽列、Footer 一致

## 待實作功能

### 🚧 進階功能
- 商品分頁顯示（目前一次顯示所有商品）
- 商品圖片上傳功能（目前使用靜態 URL）
- Email 通知（訂單確認、訂單狀態變更）
- 優惠券與折扣碼
- 商品評價與評論
- 多語系支援
- 深色模式

## 開發注意事項

### 架構與設計
- 嚴格遵守三層式架構：Controller 不直接呼叫 Repository，必須透過 Service 層
- 所有新功能都應建立對應的 Service 介面與實作
- Repository 只負責資料存取，不包含業務邏輯
- 使用 `IDbContextFactory<ApplicationDbContext>` 建立 DbContext 實例
- **使用 ValidationHelper 進行驗證**（v1.1.1 新增）：
  - 在 Service 層的方法開頭使用 ValidationHelper 進行參數驗證
  - 避免重複撰寫驗證邏輯，提升程式碼可維護性
  - 範例：`ValidationHelper.ValidateString(album.Title, "專輯標題", 200, nameof(album.Title));`
- **使用 OrderHelper 處理訂單顯示邏輯**（v1.2.1 新增）：
  - View 層不應包含複雜的業務邏輯（switch/case、if/else 判斷）
  - 訂單狀態文字、付款狀態判斷等邏輯應統一在 OrderHelper 中處理
  - 範例：`OrderHelper.GetPaymentStatusText(order)` 取代 View 中的 switch 判斷
  - 訂單流程控制（有效下一步狀態）也應在 Helper 層處理

### 資料庫與時間
- 統一使用 `DateTime.UtcNow` 而非 `DateTime.Now`，避免時區問題
- 避免 N+1 查詢問題，適當使用 `.Include()` 或快取機制
- 使用 `decimal(10,2)` 儲存價格欄位

### 安全性與權限
- 所有需要登入的功能都已加上 `[Authorize]` 屬性
- 後台管理功能使用 `[Authorize(Roles = "Admin")]`
- 操作購物車和訂單時，已驗證當前使用者權限（避免 A 使用者操作 B 使用者的資料）
- 使用 `[ValidateAntiForgeryToken]` 防止 CSRF 攻擊

### 專案設定
- 專案目標為 .NET 10.0
- HTTPS 重新導向在 `Program.cs` 中已被註解
- 程式碼中包含繁體中文註解
- 目前尚無測試專案
- 靜態檔案由 `wwwroot/` 提供

### 資料初始化
- 首次執行 `dotnet ef database update` 後，會自動執行 `DbInitializer`
- 預設管理員帳戶資訊從 `appsettings.json` 的 `AdminSettings` 區段讀取
- 範例設定檔為 `appsettings.example.json`

## 安全性注意事項

- **敏感資訊保護**：`appsettings.json` 包含資料庫連線字串等敏感資訊，已加入 `.gitignore`
- **範例設定**：使用 `appsettings.example.json` 提供設定範例，不包含真實資料
- **絕不提交**：確保不要將包含真實密碼、IP 位址的設定檔提交到版控系統
- **環境變數**：正式環境建議使用環境變數或 Azure Key Vault 管理敏感資訊

## 版本更新記錄

### v1.3.0 - 多專案架構重構 (2026-03-09)

**🏗️ 架構升級**
- 從單一專案重構為多專案解決方案（4 個獨立專案）
- 建立 `MusicShop.Data` 專案（資料存取層）
- 建立 `MusicShop.Service` 專案（商業邏輯層）
- 建立 `MusicShop.Library` 專案（共用工具庫）
- 保留 `MusicShop.Web` 專案（展示層）

**🔧 設計模式改進**
- 實作 **UnitOfWork 模式**：統一管理所有 Repository 的存取點
- 實作 **Generic Repository 模式**：提供通用的 CRUD 操作介面
- 整合 **AutoMapper**：自動化 Entity ↔ ViewModel 的物件映射
- Service 層統一改為注入 `IUnitOfWork`（原本注入多個 Repository）

**📦 命名空間重組**
- `MusicShop.Models` → `MusicShop.Data.Entities`
- `MusicShop.Services.Interface` → `MusicShop.Service.Services.Interfaces`
- `MusicShop.Services.Implementation` → `MusicShop.Service.Services.Implementation`
- `MusicShop.Repositories.Interface` → `MusicShop.Data.Repositories.Interfaces`
- `MusicShop.Repositories.Implementation` → `MusicShop.Data.Repositories.Implementation`
- `MusicShop.Helpers` → `MusicShop.Library.Helpers`
- `MusicShop.ViewModels` → `MusicShop.Service.ViewModels`

**✅ 重構成果**
- 所有專案編譯成功（0 錯誤）
- 應用程式正常啟動與運行
- 資料庫連線與初始化正常
- 所有核心功能測試通過（首頁、專輯列表、登入等）

**🎯 改進效益**
- **關注點分離**：每個專案職責明確，易於維護
- **可測試性提升**：UnitOfWork 和 Repository 模式便於單元測試
- **程式碼重用**：Generic Repository 減少重複的 CRUD 程式碼
- **依賴管理簡化**：Service 層統一注入 IUnitOfWork，降低耦合度
- **擴充性增強**：新增 Repository 或 Service 更加容易
- **團隊協作**：多專案架構便於分工開發
