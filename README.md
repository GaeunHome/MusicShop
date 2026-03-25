# MusicShop - 線上音樂專輯商店

[![.NET](https://img.shields.io/badge/.NET-10.0-512BD4)](https://dotnet.microsoft.com/)
[![ASP.NET Core](https://img.shields.io/badge/ASP.NET%20Core-MVC-512BD4)](https://docs.microsoft.com/aspnet/core/)
[![EF Core](https://img.shields.io/badge/EF%20Core-10.0-512BD4)](https://docs.microsoft.com/ef/core/)
[![SQL Server](https://img.shields.io/badge/SQL%20Server-2022-CC2927)](https://www.microsoft.com/sql-server/)

一個基於 ASP.NET Core MVC 開發的線上音樂專輯電子商務平台，採用三層式架構設計，提供完整的購物與後台管理功能。

## 專案目標

建立一個功能完整的音樂專輯電商網站，包含：
- 會員系統（註冊、登入、個人資料管理）
- 專輯展示與搜尋（雙分類系統、分頁瀏覽）
- 購物車功能（含庫存驗證）
- 訂單管理系統（含交易保護與並發控制）
- 後台管理功能（商品、分類、訂單、使用者）
- SEO 優化（Open Graph + Schema.org）

## 技術堆疊

| 類別 | 技術 | 版本 |
|------|------|------|
| **框架** | ASP.NET Core MVC | .NET 10.0 |
| **架構模式** | 三層式架構 | Controller → Service → Repository |
| **資料庫** | SQL Server | 2022 |
| **ORM** | Entity Framework Core | 10.0.3 |
| **認證** | ASP.NET Core Identity | Cookie 認證 |
| **快取** | IMemoryCache | 內建記憶體快取 |
| **前端** | Razor Views + Bootstrap 5 + jQuery | - |
| **設計模式** | Repository, UnitOfWork, DI, Factory, BaseController, MVC Area | - |
| **社群登入** | Google OAuth 2.0 + LINE Login | OAuth 2.0 |
| **物流金流** | 綠界 ECPay（超商取貨 + 信用卡付款） | All-in-One API |

## 核心功能

| 模組 | 功能 | 狀態 | 說明 |
|------|------|------|------|
| **使用者系統** | 註冊/登入/登出 | ✅ | ASP.NET Core Identity |
| | 社群登入（Google / LINE） | ✅ | OAuth 2.0 第三方登入 |
| | 個人資料管理 | ✅ | 姓名、電話、生日、性別 |
| | 兩步驟驗證（2FA） | ✅ | TOTP 驗證器應用程式 |
| | 訂單歷史查詢 | ✅ | 僅能查看自己的訂單 |
| **專輯展示** | 列表瀏覽 | ✅ | 響應式卡片設計 |
| | 雙分類篩選 | ✅ | 藝人分類 + 商品類型（階層式） |
| | 關鍵字搜尋 | ✅ | 標題、演出者 |
| | 排序功能 | ✅ | 預設/最新/價格（升/降） |
| | 分頁瀏覽 | ✅ | 伺服器端分頁 |
| **購物車** | 加入商品 | ✅ | API Controller + AJAX（不跳轉） |
| | 數量管理 | ✅ | 含庫存驗證 |
| | 購物車徽章 | ✅ | API 即時更新 + 彈跳動畫 |
| **訂單系統** | 建立訂單 | ✅ | 資料庫交易保護 |
| | 庫存扣除 | ✅ | 樂觀並發控制 |
| | 訂單追蹤 | ✅ | 5 種狀態（待處理→已完成） |
| | 訂單時間軸 | ✅ | 視覺化訂單進度 |
| | 超商門市選取 | ✅ | 綠界 ECPay 物流 API |
| | 信用卡付款 | ✅ | 綠界 ECPay All-in-One 金流 |
| | 付款失敗回滾 | ✅ | 取消訂單 + 恢復庫存 + 退還優惠券 |
| | 未完成付款自動取消 | ✅ | 信用卡訂單 15 分鐘逾時自動取消 |
| **優惠券系統** | 後台優惠券 CRUD | ✅ | 固定金額 / 百分比折扣 |
| | 兌換碼兌換 | ✅ | 使用者輸入兌換碼領取 |
| | 統一發放 / 壽星發放 | ✅ | 管理員一鍵發放給全部使用者或當月壽星 |
| | 結帳套用優惠券 | ✅ | AJAX 即時計算折扣，訂單顯示折扣後金額 |
| | 取消訂單自動退券 | ✅ | 前台/後台取消均退還優惠券 |
| **後台管理** | 儀表板 | ✅ | 統計資訊（商品/訂單/銷售額） |
| | 商品管理 | ✅ | CRUD + 雙分類 |
| | 藝人管理 | ✅ | CRUD + 上架/下架 + 分頁 + 篩選 |
| | 訂單管理 | ✅ | 完整訂單詳情（收件人、配送、付款、發票、優惠券、備註） |
| | 使用者管理 | ✅ | 角色切換（Admin/User） |
| | 幻燈片管理 | ✅ | CRUD + 圖片上傳 + 商品聯動 |
| | 精選藝人管理 | ✅ | CRUD + 首頁展示 |
| | 優惠券管理 | ✅ | CRUD + 統一發放 + 壽星發放 |
| **收藏清單** | 加入最愛 | ✅ | AJAX 即時切換 |
| | 收藏清單頁 | ✅ | 顯示收藏商品，可直接移除 |
| **效能與品質** | MemoryCache 快取 | ✅ | 分類與商品類型快取 |
| | 全域例外處理 | ✅ | GlobalExceptionMiddleware |
| | SEO 優化 | ✅ | Open Graph + Schema.org |
| | 速率限制 | ✅ | 分級限流（一般 100 / API 30 / Auth 10 req/min） |
| | 安全標頭 | ✅ | CSP、X-Frame-Options、X-Content-Type-Options 等 |

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
| **MVC Area** | `Areas/Admin/` | 後台管理功能獨立 Area 拆分 |
| **Soft Delete** | `ISoftDeletable` + Global Query Filter | 軟刪除保留資料完整性 |
| **MemoryCache** | `IMemoryCache` | 減少重複資料庫查詢 |
| **Middleware** | `GlobalExceptionMiddleware` | 集中處理未捕獲例外 |
| **View Components** | `ViewComponents/` | 可重複使用的 UI 元件 |
| **ValidationHelper** | `Helpers/` | 集中驗證邏輯 |

## 快速開始

### 系統需求

| 項目 | 需求 |
|------|------|
| **.NET SDK** | 10.0+ |
| **SQL Server** | 2019+ |
| **作業系統** | Windows / macOS (含 Apple Silicon) / Linux |
| **編輯器** | Visual Studio Code 或 Visual Studio 2025 |

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
  "EcpayPayment": {
    "MerchantID": "YOUR_PAYMENT_MERCHANT_ID",
    "HashKey": "YOUR_PAYMENT_HASH_KEY",
    "HashIV": "YOUR_PAYMENT_HASH_IV",
    "IsTest": true
  },
  "EcpayLogistics": {
    "MerchantID": "YOUR_LOGISTICS_MERCHANT_ID",
    "HashKey": "YOUR_LOGISTICS_HASH_KEY",
    "HashIV": "YOUR_LOGISTICS_HASH_IV",
    "IsTest": true
  },
  "Authentication": {
    "Google": {
      "ClientId": "YOUR_GOOGLE_CLIENT_ID",
      "ClientSecret": "YOUR_GOOGLE_CLIENT_SECRET"
    },
    "LINE": {
      "ChannelId": "YOUR_LINE_CHANNEL_ID",
      "ChannelSecret": "YOUR_LINE_CHANNEL_SECRET"
    }
  }
}
```

> **ECPay 設定說明**：金流與物流使用不同的 MerchantID。`IsTest: true` 使用綠界測試環境，`false` 切換正式環境。商家金鑰請至[綠界科技後台](https://www.ecpay.com.tw/)取得。
>
> **社群登入設定**：Google OAuth 2.0 至 [Google Cloud Console](https://console.cloud.google.com/) 設定；LINE Login 至 [LINE Developers Console](https://developers.line.biz/) 設定。

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

## 專案結構

本專案採用**多專案解決方案**架構：

```
MusicShop/
├── src/
│   ├── MusicShop.Data/                  # 資料存取層
│   │   ├── Entities/                    # 實體模型（AppUser, Album, Banner, Order...）
│   │   │   └── ISoftDeletable.cs        # 軟刪除介面
│   │   ├── Repositories/
│   │   │   ├── Interfaces/              # Repository 介面
│   │   │   └── Implementation/          # Repository 實作
│   │   ├── UnitOfWork/                  # IUnitOfWork / UnitOfWork
│   │   ├── ApplicationDbContext.cs      # DbContext（含軟刪除攔截與 Global Query Filter）
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
│       ├── Controllers/                 # 前台 MVC 控制器
│       │   └── Api/                     # RESTful API 控制器
│       ├── Areas/Admin/                 # 後台管理 Area
│       │   ├── Controllers/             # Dashboard, Album, Artist, Category, Order, User, Banner
│       │   └── Views/                   # 後台管理 Views
│       ├── Infrastructure/              # Web 基礎設施（圖片服務、ViewComponent、中間件、設定模型、常數）
│       ├── Views/
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
| **FeaturedArtists** | 精選藝人 | ArtistId, DisplayOrder, IsActive |
| **Coupons** | 優惠券模板 | Code, DiscountType, DiscountValue, ValidDays |
| **UserCoupons** | 使用者持有的優惠券 | UserId, CouponId, ExpiresAt, IsUsed, OrderId |
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
| FeaturedArtist → Artist (多對一) | Cascade |
| UserCoupon → User (多對一) | Cascade |
| UserCoupon → Coupon (多對一) | Restrict |
| UserCoupon ↔ Order (雙向可選) | NoAction |

## 版本歷史

| 版本 | 日期 | 主要內容 |
|------|------|---------|
| **v1.9.0** | 2026-03-24 | 全面程式碼審查、架構修正、效能優化與社群登入改善 |
| **v1.8.0** | 2026-03-23 | ECPay 信用卡金流串接、社群登入（Google / LINE）、付款失敗自動回滾、程式碼品質審查 |
| **v1.7.2** | 2026-03-18 | 程式碼品質審查、管理員 Email 確認、安全強化（SecurityHeaders、密碼歷史、SMTP） |
| **v1.7.1** | 2026-03-18 | 優惠券系統修復與統一發放、訂單詳情強化、程式碼品質審查 |
| **v1.7.0** | 2026-03-18 | Cookie 安全強化、軟刪除機制（ISoftDeletable）、MVC Area 拆分 |
| **v1.6.3** | 2026-03-17 | 購物車 API 化（AJAX 不跳轉）、Badge 即時更新、前端程式碼審查與清理 |
| **v1.6.2** | 2026-03-17 | 藝人管理強化（分頁、篩選、上下架）、修復 Wishlist 與訂單 Bug |
| **v1.6.1** | 2026-03-17 | 商品分頁瀏覽、MemoryCache 快取、全域例外處理、SEO 優化、訂單時間軸 |
| **v1.6.0** | 2026-03-16 | 購物車圖示加入登入提示彈跳視窗（SweetAlert2） |
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

## 安全性特性

| 項目 | 實作 |
|------|------|
| **密碼加密** | PBKDF2 演算法 + 唯一 Salt |
| **Cookie 安全** | HttpOnly + Secure + SameSite=Lax + 自訂名稱 + 2 小時過期 |
| **帳號鎖定** | 連續 3 次登入失敗鎖定 5 分鐘，防止暴力破解 |
| **CSRF 防護** | `[ValidateAntiForgeryToken]` + `SameSite=Lax` |
| **SQL 注入防護** | EF Core 參數化查詢 |
| **XSS 防護** | Razor 自動編碼 + Cookie HttpOnly |
| **速率限制** | 分級限流：一般 100 / API 30 / Auth 10 req/min |
| **安全標頭** | CSP、X-Frame-Options、X-Content-Type-Options、Referrer-Policy |
| **敏感資訊保護** | `appsettings.json` 已加入 `.gitignore` |
| **權限控制** | `[Authorize]` + 角色驗證（Admin/User） |
| **金流安全** | CheckMacValue SHA-256 驗證 + FixedTimeEquals 防 Timing Attack |
| **並發控制** | 樂觀並發（`[Timestamp]` RowVersion） |
| **交易保護** | 資料庫交易確保原子性（ACID） |
| **軟刪除** | `ISoftDeletable` + Global Query Filter，保留資料完整性 |
| **全域例外處理** | Middleware 統一攔截未捕獲例外 |

## 作者

**Hou Wen Chia**

## 授權

此專案為本人學習用途，無任何商業用途。

## 相關連結

- [CLAUDE.md](/CLAUDE.md) - Claude Code 專案指引
- [appsettings.example.json](/appsettings.example.json) - 設定檔範例
- [K-MONSTAR](https://www.k-monstar.com/)[微樂客](https://www.willmusic.com.tw/) - 靈感來源與商品圖片（韓國音樂專輯線上商店）