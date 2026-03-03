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

### 核心元件

**資料層** (`Data/ApplicationDbContext.cs`)：
- 繼承自 `IdentityDbContext<AppUser>` 以整合 ASP.NET Core Identity
- DbSets：Albums、Categories、Orders、OrderItems、CartItems
- 實體關聯設定於 `OnModelCreating`：
  - Album-Category：一對多關聯，限制刪除（Restrict）
  - Order-AppUser：一對多關聯，串聯刪除（Cascade）
  - OrderItem-Order：一對多關聯，串聯刪除（Cascade）
  - OrderItem-Album：一對多關聯，限制刪除（Restrict）
  - CartItem-AppUser：一對多關聯，串聯刪除（Cascade）
  - CartItem-Album：一對多關聯，限制刪除（Restrict）

**模型** (`Models/`)：
- `AppUser`：擴展 `IdentityUser`，包含 FullName、RegisteredAt，以及 Orders 和 CartItems 導航屬性
- `Album`：Title、Artist、Description、Price（decimal）、CoverImageUrl、Stock、CategoryId
- `Category`：Name 以及 Albums 集合
- `Order`：UserId、OrderDate、Status（列舉：Pending/Paid/Shipped/Completed/Cancelled）、TotalAmount、OrderItems 集合
- `OrderItem`：連結 Order 與 Album，包含 Quantity 和 Price
- `CartItem`：連結 User 與 Album，包含 Quantity 和 AddedAt 時間戳

**檢視模型** (`ViewMdoels/`)：
注意：目錄名稱在程式碼中拼寫為 "ViewMdoels"（少了一個 l）。
- `RegisterViewModel`、`LoginViewModel`、`AlbumViewModel`

**控制器** (`Controllers/`)：
- `AccountController`：使用 ASP.NET Core Identity 處理使用者註冊、登入、登出
- `AlbumController`：專輯瀏覽，支援搜尋與分類篩選
- `HomeController`：主要首頁
- `CartController`：購物車功能（尚未實作）
- `OrderController`：訂單管理（尚未實作）

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

## 關鍵模式

1. **預先載入（Eager Loading）**：控制器使用 `.Include()` 載入關聯實體（例如：`Albums.Include(a => a.Category)`）
2. **查詢建構**：搜尋與篩選邏輯在控制器中使用 IQueryable 模式
3. **ViewBag 傳遞篩選資料**：分類清單與搜尋詞透過 ViewBag 傳遞
4. **資料註解（Data Annotations）**：使用 `[Required]`、`[StringLength]` 等進行模型驗證
5. **小數精度**：價格欄位使用 `[Column(TypeName = "decimal(10,2)")]`

## 待實作功能

### 購物車系統（CartController）
- 加入商品至購物車（需驗證庫存）
- 顯示購物車內容（查詢 CartItems 資料表）
- 更新商品數量
- 移除購物車商品
- 清空購物車
- 需使用 `[Authorize]` 保護，確保使用者已登入
- 使用 `User.FindFirstValue(ClaimTypes.NameIdentifier)` 取得目前登入使用者 ID

### 訂單系統（OrderController）
- 結帳流程（從購物車建立訂單）
- 訂單建立時需：
  - 扣除專輯庫存（Album.Stock）
  - 建立 Order 記錄（訂單主檔）
  - 建立 OrderItem 記錄（訂單明細）
  - 清空使用者購物車
- 訂單查詢（僅能查看自己的訂單）
- 訂單詳細資訊
- 訂單狀態更新（後台功能）

### 後台管理（AdminController）
- 專輯 CRUD（新增、編輯、刪除、列表）
- 分類管理
- 訂單管理（查看所有訂單、更新狀態）
- 需要角色驗證 `[Authorize(Roles = "Admin")]`

## 開發注意事項

- 專案目標為 .NET 10.0（預覽/早期版本）
- HTTPS 重新導向在 `Program.cs` 中已被註解（第 43 行）
- 程式碼中包含繁體中文註解
- 目前尚無測試專案
- 靜態檔案由 `wwwroot/` 提供
- 購物車和訂單功能需要實作 CartController 和 OrderController
- 所有需要登入的功能都應加上 `[Authorize]` 屬性
- 操作購物車和訂單時，務必驗證當前使用者權限（避免 A 使用者操作 B 使用者的資料）

## 安全性注意事項

- **敏感資訊保護**：`appsettings.json` 包含資料庫連線字串等敏感資訊，已加入 `.gitignore`
- **範例設定**：使用 `appsettings.example.json` 提供設定範例，不包含真實資料
- **絕不提交**：確保不要將包含真實密碼、IP 位址的設定檔提交到版控系統
- **環境變數**：正式環境建議使用環境變數或 Azure Key Vault 管理敏感資訊
