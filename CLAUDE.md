# CLAUDE.md

此檔案為 Claude Code (claude.ai/code) 在此專案中工作時的指引文件。

MusicShop 是基於 ASP.NET Core MVC (.NET 10.0) 的線上音樂專輯商店。具備使用者認證、專輯瀏覽、購物車、訂單管理、後台管理與收藏清單功能。

**版本：v1.8.0**

## 建置指令

```bash
dotnet restore                          # 還原相依套件
dotnet build                            # 建置專案
dotnet run                              # 執行（http://localhost:5281）
dotnet build --configuration Release    # 正式版建置
```

### 資料庫（EF Core + SQL Server）

```bash
dotnet ef migrations add <MigrationName>
dotnet ef database update
dotnet ef migrations remove
dotnet ef database drop
```

連線字串設定於 `appsettings.json`（已 gitignore），參考 `appsettings.example.json`。

## 專案結構

四層多專案解決方案架構：

```
src/
├── MusicShop.Web/        # 展示層
│   ├── Controllers/      # 前台控制器（Home, Album, Cart, Order, Account, Wishlist）
│   ├── Areas/Admin/      # 後台管理 Area（Dashboard, Album, Artist, Category, Order, User, Banner, FeaturedArtist, Coupon）
│   ├── Views/            # 前台視圖
│   ├── Infrastructure/   # Web 層基礎設施
│   └── wwwroot/          # 靜態資源
├── MusicShop.Service/    # 商業邏輯層：Services/, ViewModels/, Mapper/
├── MusicShop.Data/       # 資料存取層：Entities/, Repositories/, UnitOfWork/
└── MusicShop.Library/    # 共用工具層：Helpers/, Enums/
```

> **`Infrastructure/`** 集中放置 Web 層基礎設施：圖片上傳服務、ViewComponent、全域例外中間件、設定模型、常數等。
> 判斷標準：不是業務邏輯、但 Web 層運作需要的東西，都放這裡。

> **`Areas/Admin/`** 後台管理使用 MVC Area 獨立拆分，每個功能模組一個 Controller。
> 路由格式：`/Admin/{Controller}/{Action}/{id?}`，例如 `/Admin/Album/Edit/5`。

相依方向：`Web → Service → Data`，`Library` 被 Service 和 Web 共用。禁止反向相依。

## 架構規範

### 分層規則

- **DO** 遵守 Controller → Service → Repository 的呼叫鏈。Controller 禁止直接存取 Repository。
- **DO** 為所有新功能建立對應的 Service 介面（`Interfaces/`）與實作（`Implementation/`）。
- **DO** Service 層統一注入 `IUnitOfWork` 存取所有 Repository，不要注入個別 Repository。
- **DO** Repository 只做資料存取，業務邏輯放 Service 層。
- **DO** 使用 `IDbContextFactory<ApplicationDbContext>` 建立 DbContext 實例。
- **DO** 顯示相關的數量常數放在 `DisplayConstants`（如 `RelatedAlbumsCount`、`OrderItemsPreviewCount`），不硬編碼 magic number。
- **DO** CSS Badge class 邏輯統一放在 `OrderHelper`（如 `GetPaymentBadgeClass`），不在 Service 層寫 CSS class 字串。
- **DO** Web 層相依邏輯（如 `IFormFile` 處理）放在 `MusicShop.Web/Services/`，不污染 Service 層。
- **DO** 後台管理功能放在 `Areas/Admin/` Area 中，每個模組一個 Controller，加上 `[Area("Admin")]` 屬性。
- **DON'T** 在 View 層撰寫業務邏輯。View 只負責顯示。

### DI 註冊

- **DO** Service 和 UnitOfWork 註冊為 `Scoped`（每個 Request 一個實例）。
- **DO** 無狀態的工具類註冊為 `Singleton`。
- **DON'T** 將 Repository 個別註冊到 DI，統一透過 `IUnitOfWork` 存取。

## 命名規範

- Service：介面 `I{Feature}Service` → 實作 `{Feature}Service`（在 `Services/Interfaces/` 和 `Services/Implementation/`）
- Repository：介面 `I{Entity}Repository` → 實作 `{Entity}Repository`，泛型用 `IGenericRepository<T>`
- Controller：前台 `{Feature}Controller`、API `{Feature}ApiController`、後台同名但在 `Areas/Admin/` 下
- ViewModel：依用途加後綴 — `CardViewModel`（卡片）、`DetailViewModel`（詳情）、`FormViewModel`（表單）、`ListItemViewModel`（列表項）、`IndexViewModel`（索引頁）
- 其他：`{Purpose}Middleware`、`{Feature}ViewComponent`、`{Purpose}Helper`、`{Scope}Constants`、`{Type}Extensions`
- Entity 用單數名詞（`Album` 不是 `Albums`）
- Service 方法：`Get{Entity}DetailViewModelAsync`、`Create{Entity}Async`、`Update{Entity}Async`、`Delete{Entity}Async`
- Repository 方法：`Get{Entity}ByIdAsync`、`Get{Entities}PagedAsync`、`Add{Entity}Async`
- Controller Action：`Index`、`Detail`、`Create`（GET/POST）、`Edit`（GET/POST）、`Delete`
- 私有欄位 `_camelCase`，常數 `PascalCase`，命名空間與資料夾結構一致
- View `{Action}.cshtml`、Partial `_{Name}.cshtml`、CSS 元件 `components/{name}.css`、JS 功能 `features/{feature}/{name}.js`

## 資料庫

- **DO** 統一使用 `DateTime.UtcNow`。
- **DO** 價格欄位使用 `decimal(10,2)`。
- **DO** 使用 `.Include()` 預先載入關聯實體，避免 N+1 查詢。
- **DO** 唯讀查詢使用 `.AsNoTracking()` 降低記憶體開銷。
- **DO** 涉及多步驟資料修改時使用資料庫交易確保原子性。
- **DO** 利用 `[Timestamp]` RowVersion 做樂觀並發控制。
- **DO** 需要軟刪除的實體實作 `ISoftDeletable` 介面，`DbContext.SaveChangesAsync` 會自動攔截並轉為軟刪除。
- **DO** 查詢已刪除資料時使用 `.IgnoreQueryFilters()`。
- **DO** 父子強關聯用 `Cascade`，參考關聯用 `Restrict`（Service 層先檢查再刪除）。
- **DON'T** 對 CartItem、WishlistItem、OrderItem 使用軟刪除（暫存性質，硬刪即可）。

## 驗證與錯誤處理

### 四層驗證

1. **ViewModel**：Data Annotations（`[Required]`、`[StringLength]`、`[Range]`）
2. **Controller**：`ModelState.IsValid` 檢查，失敗返回 View
3. **Service**：方法開頭使用 `ValidationHelper` 進行參數驗證（非空、ID 正數、實體存在）
4. **Database**：FK、Unique、Check Constraint

### 錯誤處理

- **DO** 使用 `GlobalExceptionMiddleware` 攔截未處理例外，依類型導向對應狀態碼：`UnauthorizedAccessException` → 403、`KeyNotFoundException` → 404、`DbUpdateConcurrencyException` → 409、其他 → 500。
- **DO** Service 層透過 `ValidationHelper` 拋出具描述性的例外訊息。
- **DO** Controller 使用 `try-catch` 捕獲例外，設定 `TempData[TempDataKeys.Error]` 後返回 View。
- **DO** 訂單顯示邏輯（狀態文字、Badge class、流程控制）使用 `OrderHelper`，不在 View 中寫 switch/if。
- **DON'T** 在 Controller 中直接回傳例外訊息給使用者（可能洩漏系統資訊）。

## AutoMapper

- **DO** 使用單一 `MapperProfile.cs` 管理所有映射規則。
- **DO** 使用 AutoMapper 處理 Entity ↔ ViewModel 轉換，避免手動 `.Select()` 映射。
- **DO** 導航屬性使用 null-safe 映射（`s.Artist != null ? s.Artist.Name : null`）。
- **DO** 需要額外查詢才能取得的屬性使用 `.ForMember(d => d.Xxx, o => o.Ignore())`，在 Service 中手動賦值。
- **DO** `ReverseMap()` 時排除 `Id`、`RowVersion`、`CreatedAt` 等唯讀欄位。

## 程式碼風格

- **DO** 使用繁體中文撰寫註解。
- **DO** 使用 `async/await` 處理所有非同步操作，方法名稱加 `Async` 後綴。
- **DO** Controller 中萃取重複邏輯為私有輔助方法。
- **DO** 使用 Serilog 結構化日誌，以 `{PropertyName}` 佔位符傳遞參數（如 `_logger.LogInformation("商品已新增：{AlbumId}", id)`）。
- **DO** 日誌等級：Debug（開發除錯）、Info（業務事件）、Warning（可恢復異常）、Error（系統錯誤）。
- **DON'T** 在 Controller 中重複撰寫驗證邏輯，統一由 Service 層處理。

## 前端規範

### 靜態資源結構

```
wwwroot/
├── css/
│   ├── site.css              # 全域樣式
│   ├── variables.css         # CSS 自訂屬性（色彩、間距、字型）
│   ├── components/           # 可複用元件樣式（pagination.css, skeleton.css 等）
│   └── pages/                # 頁面專屬樣式（admin.css, account.css 等）
├── js/
│   ├── core/                 # 全域初始化與工具（init.js, site.js, notifications.js）
│   ├── features/             # 功能模組，按 feature 分資料夾（cart/, search/, album/ 等）
│   ├── admin/                # 後台專用腳本
│   └── utils/                # 通用工具函式
└── images/
```

### View 模式

- **Layout**：`_Layout.cshtml`（前台）、`_AdminLayout.cshtml`（後台）
- **Partial View**：以 `_` 開頭（`_AlbumCard.cshtml`、`_Pagination.cshtml`），用於可複用 UI 片段
- **ViewComponent**：需要後端邏輯的可複用 UI 元件（如 `CartBadgeViewComponent`）
- **Section**：`@section Styles { }` 和 `@section Scripts { }` 載入頁面專屬 CSS/JS
- **TempData 通知**：透過 `_TempDataNotifications.cshtml` Partial 統一顯示成功/錯誤訊息

### RWD

- 使用 Bootstrap 5 Grid System
- 表單寬度：`col-md-6 col-lg-5`（平板半寬、桌面窄化）
- 側邊選單：桌面 `d-none d-lg-block`，手機改用 Dropdown

## 安全性

- `appsettings.json` 已加入 `.gitignore`，絕不提交含真實密碼/連線字串的設定檔
- 需登入的功能加 `[Authorize]`，後台加 `[Authorize(Roles = "Admin")]`
- 操作購物車/訂單時驗證當前使用者權限，防止越權存取
- POST 請求使用 `[ValidateAntiForgeryToken]` 防止 CSRF
- Cookie 驗證已設定 HttpOnly、Secure、SameSite、自訂名稱與過期時間
- 帳號鎖定機制：連續 3 次登入失敗鎖定 5 分鐘，防止暴力破解
- 速率限制：一般頁面 100 req/min（Sliding Window）、API 30 req/min（Sliding Window）、Auth 10 req/min（Fixed Window）
- Security Headers：`X-Content-Type-Options: nosniff`、`X-Frame-Options: DENY`、CSP
- 正式環境建議使用環境變數或 Azure Key Vault 管理敏感資訊

## 開發注意事項

- 前台路由：`{controller=Home}/{action=Index}/{id?}`
- 後台路由：`{area:exists}/{controller=Dashboard}/{action=Index}/{id?}`
- 首次 `dotnet ef database update` 後會自動執行 `DbInitializer`（建立角色、管理員帳戶、種子資料）
- 管理員帳戶設定從 `appsettings.json` 的 `AdminSettings` 區段讀取
- 目前無測試專案
- HTTPS 重新導向已在 `Program.cs` 中註解
