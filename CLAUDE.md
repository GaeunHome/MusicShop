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

## 架構說明

### 三層式架構（Three-Tier Architecture）

本專案採用標準的三層式架構設計：

**展示層（Presentation Layer）** - `Controllers/`、`Views/`
- 負責處理使用者輸入與畫面顯示
- 不包含商業邏輯，僅負責資料傳遞與顯示

**商業邏輯層（Business Logic Layer）** - `Services/`
- 包含所有業務規則與驗證邏輯
- 協調資料存取層與展示層之間的互動
- 介面與實作分離（Interface/ 與 Implementation/）

**資料存取層（Data Access Layer）** - `Repositories/`、`Data/`
- 負責資料庫 CRUD 操作
- 使用 Repository 模式封裝資料存取
- 介面與實作分離（Interface/ 與 Implementation/）

### 核心元件

**資料層** (`Data/ApplicationDbContext.cs` + `Repositories/`)：
- `ApplicationDbContext`：繼承自 `IdentityDbContext<AppUser>`，整合 ASP.NET Core Identity
- DbSets：Albums、ArtistCategories、ProductTypes、Orders、OrderItems、CartItems
- 使用 `IDbContextFactory<ApplicationDbContext>` 提供更好的並行處理能力
- **Repository 介面與實作**：
  - `IAlbumRepository` / `AlbumRepository`：專輯資料存取
  - `ICartRepository` / `CartRepository`：購物車資料存取
  - `IOrderRepository` / `OrderRepository`：訂單資料存取
  - `IArtistCategoryRepository` / `ArtistCategoryRepository`：藝人分類資料存取
  - `IProductTypeRepository` / `ProductTypeRepository`：商品類型資料存取
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

**模型** (`Models/`)：
- `AppUser`：擴展 `IdentityUser`，包含 FullName、PhoneNumber、Birthday、Gender、RegisteredAt，以及 Orders 和 CartItems 導航屬性
- `Album`：Title、Artist、Description、Price（decimal）、CoverImageUrl、Stock、ArtistCategoryId、ProductTypeId
- `ArtistCategory`：藝人分類（BOY GROUP、GIRL GROUP、SOLO）
- `ProductType`：商品類型（階層式架構，包含 ParentId 實現父子關係）
  - 父分類：K-ALBUM、K-MAGAZINE、K-MERCH、K-EVENT
  - 子分類：ALBUM、PHOTOBOOK、DVD、寫真雜誌、官方周邊等
- `Order`：UserId、OrderDate、Status（列舉：Pending/Paid/Shipped/Completed/Cancelled）、TotalAmount、OrderItems 集合
- `OrderItem`：連結 Order 與 Album，包含 Quantity 和 UnitPrice
- `CartItem`：連結 User 與 Album，包含 Quantity 和 AddedAt 時間戳

**服務層** (`Services/`)：
- **介面** (`Interface/`)：
  - `IAlbumService`：專輯業務邏輯介面
  - `ICartService`：購物車業務邏輯介面
  - `IOrderService`：訂單業務邏輯介面
  - `IArtistCategoryService`：藝人分類業務邏輯介面
  - `IProductTypeService`：商品類型業務邏輯介面
  - `IStatisticsService`：統計業務邏輯介面
  - `IUserService`：使用者管理業務邏輯介面
- **實作** (`Implementation/`)：對應的服務實作類別

**檢視模型** (`ViewModels/`)：
- `RegisterViewModel`：註冊表單
- `LoginViewModel`：登入表單
- `AlbumViewModel`：專輯列表與詳細資訊
- `AccountIndexViewModel`：個人帳號總覽
- `EditProfileViewModel`：個人資料編輯
- `UserManagementViewModel`：後台使用者管理

**控制器** (`Controllers/`)：
- `AccountController`：帳號管理（註冊、登入、登出、個人資料編輯、訂單查詢）
- `AlbumController`：專輯瀏覽，支援搜尋與雙分類篩選
- `HomeController`：主要首頁與輪播
- `CartController`：購物車功能（✅ 已完成）
- `OrderController`：訂單管理（✅ 已完成）
- `AdminController`：後台管理（商品、分類、訂單、使用者管理）（✅ 已完成）

**View Components** (`ViewComponents/`)：
- `CartBadgeViewComponent`：購物車數量徽章元件，動態顯示購物車商品數量

**工具類別** (`Helpers/`)：
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

1. **三層式架構分離**：嚴格遵守 Controller → Service → Repository 的職責分離原則
2. **依賴注入（Dependency Injection）**：所有服務透過介面注入，降低耦合度
3. **Repository 模式**：資料存取邏輯封裝於 Repository 層，提高可測試性
4. **IDbContextFactory 模式**：使用工廠模式建立 DbContext，避免並行問題
5. **預先載入（Eager Loading）**：使用 `.Include()` 載入關聯實體，避免 N+1 查詢問題
6. **Dictionary 快取**：在 OrderService 中使用字典快取已查詢的專輯，避免重複查詢資料庫
7. **UTC 時間**：統一使用 `DateTime.UtcNow` 避免時區問題
8. **資料註解（Data Annotations）**：使用 `[Required]`、`[StringLength]` 等進行模型驗證
9. **小數精度**：價格欄位使用 `[Column(TypeName = "decimal(10,2)")]`
10. **ViewBag 傳遞資料**：分類清單與搜尋詞透過 ViewBag 傳遞給檢視
11. **View Components**：可重複使用的 UI 元件（如購物車徽章）
12. **ValidationHelper 模式**（v1.1.1 新增）：集中管理所有驗證邏輯，避免重複程式碼

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
  - 扣除專輯庫存（Album.Stock）
  - 建立 Order 記錄（訂單主檔）
  - 建立 OrderItem 記錄（訂單明細）
  - 清空使用者購物車
  - 使用 Dictionary 快取避免 N+1 查詢問題
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
