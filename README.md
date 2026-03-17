# MusicShop - 線上音樂專輯商店

[![.NET](https://img.shields.io/badge/.NET-10.0-512BD4)](https://dotnet.microsoft.com/)
[![ASP.NET Core](https://img.shields.io/badge/ASP.NET%20Core-MVC-512BD4)](https://docs.microsoft.com/aspnet/core/)
[![EF Core](https://img.shields.io/badge/EF%20Core-10.0-512BD4)](https://docs.microsoft.com/ef/core/)
[![SQL Server](https://img.shields.io/badge/SQL%20Server-2019+-CC2927)](https://www.microsoft.com/sql-server/)

一個基於 ASP.NET Core MVC 開發的線上音樂專輯電子商務平台，採用三層式架構設計，提供完整的購物與後台管理功能。

---

## 專案目標

建立一個功能完整的音樂專輯電商網站，包含：
- 會員系統（註冊、登入、個人資料管理）
- 專輯展示與搜尋（雙分類系統、分頁瀏覽）
- 購物車功能（含庫存驗證）
- 訂單管理系統（含交易保護與並發控制）
- 後台管理功能（商品、分類、訂單、使用者）
- SEO 優化（Open Graph + Schema.org）

---

## 技術堆疊

| 類別 | 技術 | 版本 |
|------|------|------|
| **框架** | ASP.NET Core MVC | .NET 10.0 |
| **架構模式** | 三層式架構 | Controller → Service → Repository |
| **資料庫** | SQL Server | 2019+ |
| **ORM** | Entity Framework Core | 10.0.3 |
| **認證** | ASP.NET Core Identity | Cookie 認證 |
| **快取** | IMemoryCache | 內建記憶體快取 |
| **前端** | Razor Views + Bootstrap 5 + jQuery | - |
| **設計模式** | Repository, UnitOfWork, DI, Factory, BaseController, Partial Controller | - |
| **物流金流** | 綠界 ECPay 物流 API（超商取貨） | logistics API |

---

## 核心功能

| 模組 | 功能 | 狀態 | 說明 |
|------|------|------|------|
| **使用者系統** | 註冊/登入/登出 | ✅ | ASP.NET Core Identity |
| | 個人資料管理 | ✅ | 姓名、電話、生日、性別 |
| | 訂單歷史查詢 | ✅ | 僅能查看自己的訂單 |
| **專輯展示** | 列表瀏覽 | ✅ | 響應式卡片設計 |
| | 雙分類篩選 | ✅ | 藝人分類 + 商品類型（階層式） |
| | 關鍵字搜尋 | ✅ | 標題、演出者 |
| | 排序功能 | ✅ | 預設/最新/價格（升/降） |
| | 分頁瀏覽 | ✅ | 伺服器端分頁（v1.6.1） |
| **購物車** | 加入商品 | ✅ | API Controller + AJAX（不跳轉）（v1.6.3） |
| | 數量管理 | ✅ | 含庫存驗證 |
| | 購物車徽章 | ✅ | API 即時更新 + 彈跳動畫（v1.6.3） |
| **訂單系統** | 建立訂單 | ✅ | 資料庫交易保護 |
| | 庫存扣除 | ✅ | 樂觀並發控制 |
| | 訂單追蹤 | ✅ | 5 種狀態（待處理→已完成） |
| | 訂單時間軸 | ✅ | 視覺化訂單進度（v1.6.1） |
| | 超商門市選取 | ✅ | 綠界 ECPay 物流 API |
| **後台管理** | 儀表板 | ✅ | 統計資訊（商品/訂單/銷售額） |
| | 商品管理 | ✅ | CRUD + 雙分類 |
| | 藝人管理 | ✅ | CRUD + 上架/下架 + 分頁 + 篩選（v1.6.2） |
| | 訂單管理 | ✅ | 查看/更新狀態 |
| | 使用者管理 | ✅ | 角色切換（Admin/User） |
| | 幻燈片管理 | ✅ | CRUD + 圖片上傳 + 商品聯動 |
| **收藏清單** | 加入最愛 | ✅ | AJAX 即時切換 |
| | 收藏清單頁 | ✅ | 顯示收藏商品，可直接移除 |
| **效能與品質** | MemoryCache 快取 | ✅ | 分類與商品類型快取（v1.6.1） |
| | 全域例外處理 | ✅ | GlobalExceptionMiddleware（v1.6.1） |
| | SEO 優化 | ✅ | Open Graph + Schema.org（v1.6.1） |
| **待開發** | Email 通知 | ⏳ | 待開發 |

---

## 系統架構

### 三層式架構

```
展示層 (MusicShop.Web)          Controllers/ + Views/ + Middleware/
        ↓ DI
商業邏輯層 (MusicShop.Service)  Services/ (Interface + Implementation)
        ↓ DI
資料存取層 (MusicShop.Data)     Repositories/ + UnitOfWork/ + Entities/
        ↓
SQL Server
```

### 核心設計模式

| 模式 | 實作位置 | 目的 |
|------|---------|------|
| **Repository Pattern** | `Repositories/` | 封裝資料存取邏輯 |
| **Generic Repository** | `GenericRepository<T>` | 通用 CRUD，減少重複程式碼 |
| **UnitOfWork** | `UnitOfWork/` | 統一管理 Repository 與交易 |
| **Service Layer** | `Services/` | 封裝商業邏輯 |
| **Dependency Injection** | `Program.cs` | 降低耦合度 |
| **Factory Pattern** | `IDbContextFactory<T>` | 避免 DbContext 並行問題 |
| **BaseController** | `BaseController` | 共用控制器邏輯萃取 |
| **Partial Controller** | `Controllers/Admin/` | 以 partial class 拆分大型控制器 |
| **MemoryCache** | `IMemoryCache` | 減少重複資料庫查詢 |
| **Middleware** | `GlobalExceptionMiddleware` | 集中處理未捕獲例外 |
| **View Components** | `ViewComponents/` | 可重複使用的 UI 元件 |
| **ValidationHelper** | `Helpers/` | 集中驗證邏輯 |

---

## 快速開始

### 系統需求

| 項目 | 需求 |
|------|------|
| **.NET SDK** | 10.0+ |
| **SQL Server** | 2019+ |
| **作業系統** | Windows / macOS (含 Apple Silicon) / Linux |
| **編輯器** | Visual Studio Code（推薦）或 Visual Studio 2025 |

### 安裝步驟

#### 1. 複製專案
```bash
git clone <repository-url>
cd MusicShop
```

#### 2. 設定資料庫連線
複製 `appsettings.example.json` 為 `appsettings.json`，並修改連線字串：

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=<伺服器>;Database=kalbum;User Id=<帳號>;Password=<密碼>;TrustServerCertificate=True;"
  },
  "Ecpay": {
    "MerchantID": "YOUR_MERCHANT_ID",
    "HashKey": "YOUR_HASH_KEY",
    "HashIV": "YOUR_HASH_IV",
    "IsTest": true
  }
}
```

> **ECPay 設定說明**：`IsTest: true` 使用綠界測試環境，`false` 切換正式環境。商家金鑰請至[綠界科技後台](https://www.ecpay.com.tw/)取得。

#### 3. 還原套件與建立資料庫
```bash
dotnet restore
dotnet ef database update
```

#### 4. 執行應用程式
```bash
dotnet run
# 或使用自動重新載入
dotnet watch run
```

應用程式將在 `http://localhost:5281` 啟動。

---

## 專案結構

本專案採用**多專案解決方案**架構（v1.3.0 起）：

```
MusicShop/
├── src/
│   ├── MusicShop.Data/                  # 資料存取層
│   │   ├── Entities/                    # 實體模型（AppUser, Album, Banner, Order...）
│   │   ├── Repositories/
│   │   │   ├── Interfaces/              # Repository 介面
│   │   │   └── Implementation/          # Repository 實作
│   │   ├── UnitOfWork/                  # IUnitOfWork / UnitOfWork
│   │   ├── ApplicationDbContext.cs
│   │   ├── DbInitializer.cs
│   │   └── Migrations/                  # EF Core 遷移檔案
│   │
│   ├── MusicShop.Service/               # 商業邏輯層
│   │   ├── Services/
│   │   │   ├── Interfaces/              # Service 介面
│   │   │   └── Implementation/          # Service 實作
│   │   ├── ViewModels/                  # ViewModel 定義
│   │   ├── Constants/                   # CacheKeys 等常數定義
│   │   └── Mapper/                      # AutoMapper 設定
│   │
│   ├── MusicShop.Library/               # 共用工具庫
│   │   ├── Helpers/                     # 工具類別與擴充方法（統一放置）
│   │   └── Enums/                       # 共用列舉定義
│   │
│   └── MusicShop.Web/                   # 展示層
│       ├── Controllers/                 # MVC 控制器
│       │   ├── Admin/                   # 後台管理 partial class 控制器
│       │   └── Api/                     # RESTful API 控制器
│       ├── Infrastructure/              # Web 基礎設施（圖片服務、ViewComponent、中間件、設定模型、常數）
│       ├── Views/
│       │   ├── Admin/                   # 後台管理 Views
│       │   ├── Shared/                  # 共用局部視圖（_AlbumCard 等）
│       │   └── Home/                    # 動態幻燈片首頁
│       └── wwwroot/
│           ├── css/                     # 全域與頁面專屬樣式
│           ├── js/                      # JavaScript 模組
│           └── images/                  # 專輯封面、幻燈片圖片
│
├── .vscode/                             # VS Code 設定（偵錯啟動設定）
├── CLAUDE.md                            # Claude Code 專案指引
├── README.md                            # 本文件
└── MusicShop.slnx                       # 解決方案檔案
```

---

## 資料庫架構

### 主要資料表

| 資料表 | 說明 | 關鍵欄位 |
|--------|------|---------|
| **Albums** | 專輯資訊 | Title, Price, Stock, RowVersion (並發控制) |
| **ArtistCategories** | 藝人分類 | Name（BOY GROUP/GIRL GROUP/SOLO） |
| **ProductTypes** | 商品類型（階層式） | Name, ParentId（支援父子關係） |
| **Orders** | 訂單主檔 | UserId, TotalAmount, Status |
| **OrderItems** | 訂單明細 | OrderId, AlbumId, Quantity, UnitPrice |
| **CartItems** | 購物車項目 | UserId, AlbumId, Quantity |
| **WishlistItems** | 收藏清單項目 | UserId, AlbumId, AddedAt（唯一索引） |
| **Banners** | 首頁幻燈片 | Title, ImageUrl, AlbumId, IsActive |
| **AspNetUsers** | 使用者資料 | Email, PasswordHash, FullName |
| **AspNetRoles** | 角色資料 | User, Admin |

### 關鍵關聯

| 關聯 | 刪除行為 |
|------|---------|
| Album → ArtistCategory (多對一) | Restrict |
| Album → ProductType (多對一) | Restrict |
| ProductType → ProductType (自我關聯) | Restrict |
| Order → User (多對一) | Cascade |
| Order → OrderItems (一對多) | Cascade |
| OrderItem → Album (多對一) | Restrict |
| CartItem → User / Album | Cascade / Restrict |
| Banner → Album (多對一) | SetNull |

---

## 版本歷史

### v1.6.3 (2026-03-17) - 購物車 API 化、前端程式碼審查與清理

- 新增 `CartApiController`（RESTful API）：`POST /api/cart/add`、`GET /api/cart/count`
- 加入購物車改為 AJAX（不再整頁跳轉），購物車 Badge 即時更新 + 彈跳動畫
- `AjaxHelper` 新增 `postJson()` 方法支援 JSON API 呼叫
- 全面前端程式碼審查與清理：
  - 修復 `detail.js` 引用錯誤 DOM ID 的 Bug（數量 +/- 按鈕未同步 AJAX 提交值）
  - 修復 `init.js` 呼叫不存在的 `.init()` 方法（改為正確的 `.initList()`）
  - 修復 `admin/artist.js` 從未載入也未初始化的問題
  - 刪除 `detail.js` 中被 `wishlist.js` 取代的死代碼
  - 刪除 `common.js` 中從未觸發的 `bindLogoutHandler`
  - 刪除未被引用的 `_SkeletonCard.cshtml` 局部視圖
  - 移除 `Admin/Album/Create.cshtml` 與 `Edit.cshtml` 重複載入的 `admin/album.js`
  - 移除 `Album/Index.cshtml` 與 `init.js` 重複的初始化呼叫
  - 清除 `carousel.js`、`admin/album.js`、`admin/artist.js`、`init.js` 中的除錯 `console.log`

### v1.6.2 (2026-03-17) - 藝人管理強化與 Bug 修復

- 藝人管理頁面重新設計：分頁瀏覽、分類與上架狀態篩選、商品數量顯示
- 藝人上架/下架功能（`IsActive` 欄位），下架藝人不在前台導覽列顯示
- 新增藝人時顯示目前最大排序值，方便決定排序位置
- 修復收藏功能（Wishlist）500 錯誤：修正 `ApplicationDbContext` 中 WishlistItem 關聯設定
- 修復訂單建立失敗：補建 `Order.UpdatedAt` 欄位的資料庫遷移
- 新增 `.vscode/launch.json` 支援 VS Code 一鍵啟動偵錯

### v1.6.1 (2026-03-17) - 分頁、快取與 SEO 優化

- 商品列表分頁瀏覽（伺服器端分頁，`PagedResult<T>` 模型）
- MemoryCache 快取機制（分類與商品類型資料，`CacheKeys` 常數集中管理）
- 全域例外處理中介軟體（`GlobalExceptionMiddleware`，統一錯誤頁面與日誌記錄）
- SEO 優化（Open Graph meta tags + Schema.org 結構化資料）
- 訂單時間軸視覺化（訂單詳情頁顯示狀態進度）
- Web 層重構：`Services/` 更名為 `Infrastructure/`，新增 `Middleware/`、`Constants/`、`Models/` 目錄
- 後台控制器改用 partial class 拆分至 `Controllers/Admin/`
- 控制器共用邏輯萃取至 `BaseController`

### v1.6.0 (2026-03-16) - 購物車登入提示優化

- 未登入點擊導覽列購物車圖示時，以 SweetAlert2 彈跳視窗提示「請先登入」
- 提供「立即登入」（帶 returnUrl）與「稍後再說」兩個選項
- 沿用現有 `data-require-auth` + `Common.bindAuthGuard()` 機制

### 歷史版本摘要（v1.0.0 ~ v1.5.2）

| 版本 | 日期 | 主要內容 |
|------|------|---------|
| **v1.5.2** | 2026-03-11 | 首頁商品卡片改用共用局部視圖；收藏頁補上樣式 |
| **v1.5.1** | 2026-03-11 | 未登入操作改以 SweetAlert2 彈窗提示；全域攔截機制 |
| **v1.5.0** | 2026-03-11 | 收藏清單功能（WishlistItem、AJAX 愛心切換） |
| **v1.4.1** | 2026-03-11 | 修正搜尋參數；整合綠界 ECPay 超商物流 |
| **v1.4.0** | 2026-03-11 | 動態首頁幻燈片（Banner CRUD、圖片上傳、商品聯動） |
| **v1.3.1** | 2026-03-09 | 修正結帳頁面表單條件驗證 |
| **v1.3.0** | 2026-03-09 | 多專案解決方案重構；UnitOfWork、Generic Repository、AutoMapper |
| **v1.2.1** | 2026-03-09 | OrderHelper 工具類別；訂單顯示邏輯集中管理 |
| **v1.2.0** | 2026-03-09 | 資料庫交易保護；樂觀並發控制（RowVersion）；DRY 重構 |
| **v1.1.1** | 2026-03-06 | ValidationHelper 工具類別；專輯列表 UI/UX 重新設計 |
| **v1.1.0** | 2026-03-04 | 三層式架構、購物車、訂單、後台管理、DbInitializer |
| **v1.0.0** | 2026-03-03 | 初始版本：使用者認證、專輯展示、首頁輪播 |

---

## 開發指令

### 建置與執行

| 指令 | 說明 |
|------|------|
| `dotnet restore` | 還原 NuGet 套件 |
| `dotnet build` | 建置專案 |
| `dotnet run` | 執行專案（`http://localhost:5281`） |
| `dotnet watch run` | 開發模式（自動重新載入） |
| `dotnet build --configuration Release` | 建置正式版本 |

### 資料庫遷移

| 指令 | 說明 |
|------|------|
| `dotnet ef migrations add <名稱>` | 新增遷移 |
| `dotnet ef database update` | 套用遷移到資料庫 |
| `dotnet ef migrations remove` | 移除最後一次遷移（需尚未套用） |
| `dotnet ef database drop` | 刪除資料庫 |

---

## 安全性特性

| 項目 | 實作 |
|------|------|
| **密碼加密** | PBKDF2 演算法 + 唯一 Salt |
| **CSRF 防護** | `[ValidateAntiForgeryToken]` |
| **SQL 注入防護** | EF Core 參數化查詢 |
| **XSS 防護** | Razor 自動編碼 |
| **敏感資訊保護** | `appsettings.json` 已加入 `.gitignore` |
| **權限控制** | `[Authorize]` + 角色驗證（Admin/User） |
| **並發控制** | 樂觀並發（`[Timestamp]` RowVersion） |
| **交易保護** | 資料庫交易確保原子性（ACID） |
| **全域例外處理** | Middleware 統一攔截未捕獲例外 |

---

## 作者

**Hou Wen Chia**

---

## 授權

此專案為學習與教育用途。

---

## 相關連結

- [CLAUDE.md](/CLAUDE.md) - Claude Code 專案指引
- [appsettings.example.json](/appsettings.example.json) - 設定檔範例
- [K-MONSTAR](https://www.k-monstar.com/) - 靈感來源（韓國音樂專輯線上商店）
