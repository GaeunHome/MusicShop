# CLAUDE.md

此檔案為 Claude Code (claude.ai/code) 在此專案中工作時的指引文件。

MusicShop 是基於 ASP.NET Core MVC (.NET 10.0) 的線上音樂專輯商店。具備使用者認證、專輯瀏覽、購物車、訂單管理、後台管理與收藏清單功能。

**版本：v1.7.2**

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

相依方向：`Web → Service → Data`，`Library` 被 Service 和 Web 共用。

## 架構規範

### 分層規則

- **DO** 遵守 Controller → Service → Repository 的呼叫鏈。Controller 禁止直接存取 Repository。
- **DO** 為所有新功能建立對應的 Service 介面（`Interfaces/`）與實作（`Implementation/`）。
- **DO** Service 層統一注入 `IUnitOfWork` 存取所有 Repository，不要注入個別 Repository。
- **DO** Repository 只做資料存取，業務邏輯放 Service 層。
- **DO** 使用 `IDbContextFactory<ApplicationDbContext>` 建立 DbContext 實例。
- **DO** 使用 AutoMapper（`Mapper/MapperProfile.cs`）處理 Entity ↔ ViewModel 轉換，避免手動 `.Select()` 映射。
- **DO** 顯示相關的數量常數放在 `DisplayConstants`（如 `RelatedAlbumsCount`、`OrderItemsPreviewCount`），不硬編碼 magic number。
- **DO** CSS Badge class 邏輯統一放在 `OrderHelper`（如 `GetPaymentBadgeClass`），不在 Service 層寫 CSS class 字串。
- **DO** Web 層相依邏輯（如 `IFormFile` 處理）放在 `MusicShop.Web/Services/`，不污染 Service 層。
- **DO** 後台管理功能放在 `Areas/Admin/` Area 中，每個模組一個 Controller，加上 `[Area("Admin")]` 屬性。

### 驗證與 Helper

- **DO** 在 Service 層方法開頭使用 `ValidationHelper` 進行參數驗證。
- **DO** 訂單顯示邏輯（狀態文字、Badge class、流程控制）使用 `OrderHelper`，不在 View 中寫 switch/if。
- **DON'T** 在 View 層撰寫業務邏輯。View 只負責顯示。

### 資料庫

- **DO** 統一使用 `DateTime.UtcNow`。
- **DO** 價格欄位使用 `decimal(10,2)`。
- **DO** 使用 `.Include()` 預先載入關聯實體，避免 N+1 查詢。
- **DO** 涉及多步驟資料修改時使用資料庫交易確保原子性。
- **DO** 利用 `[Timestamp]` RowVersion 做樂觀並發控制。
- **DO** 需要軟刪除的實體實作 `ISoftDeletable` 介面，`DbContext.SaveChangesAsync` 會自動攔截並轉為軟刪除。
- **DO** 查詢已刪除資料時使用 `.IgnoreQueryFilters()`。
- **DON'T** 對 CartItem、WishlistItem、OrderItem 使用軟刪除（暫存性質，硬刪即可）。

### 程式碼風格

- **DO** 使用繁體中文撰寫註解。
- **DO** Controller 中萃取重複邏輯為私有輔助方法。
- **DON'T** 在 Controller 中重複撰寫驗證邏輯，統一由 Service 層處理。

## 開發注意事項

- 前台路由：`{controller=Home}/{action=Index}/{id?}`
- 後台路由：`{area:exists}/{controller=Dashboard}/{action=Index}/{id?}`
- 首次 `dotnet ef database update` 後會自動執行 `DbInitializer`（建立角色、管理員帳戶、種子資料）
- 管理員帳戶設定從 `appsettings.json` 的 `AdminSettings` 區段讀取
- 目前無測試專案
- HTTPS 重新導向已在 `Program.cs` 中註解

## 安全性

- `appsettings.json` 已加入 `.gitignore`，絕不提交含真實密碼/連線字串的設定檔
- 需登入的功能加 `[Authorize]`，後台加 `[Authorize(Roles = "Admin")]`
- 操作購物車/訂單時驗證當前使用者權限，防止越權存取
- POST 請求使用 `[ValidateAntiForgeryToken]` 防止 CSRF
- Cookie 驗證已設定 HttpOnly、Secure、SameSite、自訂名稱與過期時間
- 帳號鎖定機制：連續 3 次登入失敗鎖定 5 分鐘，防止暴力破解
- 正式環境建議使用環境變數或 Azure Key Vault 管理敏感資訊
