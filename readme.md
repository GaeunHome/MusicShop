# MusicShop - 線上音樂專輯商店

[![.NET](https://img.shields.io/badge/.NET-10.0-512BD4)](https://dotnet.microsoft.com/)
[![ASP.NET Core](https://img.shields.io/badge/ASP.NET%20Core-MVC-512BD4)](https://docs.microsoft.com/aspnet/core/)
[![EF Core](https://img.shields.io/badge/EF%20Core-10.0-512BD4)](https://docs.microsoft.com/ef/core/)
[![SQL Server](https://img.shields.io/badge/SQL%20Server-2019+-CC2927)](https://www.microsoft.com/sql-server/)

一個基於 ASP.NET Core MVC 開發的線上音樂專輯電子商務平台，採用三層式架構設計，提供完整的購物與後台管理功能。

---

## 📋 目錄

- [專案目標](#-專案目標)
- [技術堆疊](#-技術堆疊)
- [核心功能](#-核心功能)
- [系統架構](#-系統架構)
- [快速開始](#-快速開始)
- [專案結構](#-專案結構)
- [資料庫架構](#-資料庫架構)
- [版本歷史](#-版本歷史)
- [開發指令](#-開發指令)

---

## 🎯 專案目標

建立一個功能完整的音樂專輯電商網站，包含：
- ✅ 會員系統（註冊、登入、個人資料管理）
- ✅ 專輯展示與搜尋（雙分類系統）
- ✅ 購物車功能（含庫存驗證）
- ✅ 訂單管理系統（含交易保護）
- ✅ 後台管理功能（商品、分類、訂單、使用者）

---

## 🛠️ 技術堆疊

| 類別 | 技術 | 版本 |
|------|------|------|
| **框架** | ASP.NET Core MVC | .NET 10.0 |
| **架構模式** | 三層式架構 | Controller → Service → Repository |
| **資料庫** | SQL Server | 2019+ |
| **ORM** | Entity Framework Core | 10.0.3 |
| **認證** | ASP.NET Core Identity | Cookie 認證 |
| **前端** | Razor Views + Bootstrap 5 + jQuery | - |
| **設計模式** | Repository、Dependency Injection、Factory | - |

---

## ✨ 核心功能

### 功能概覽

| 模組 | 功能 | 狀態 | 說明 |
|------|------|------|------|
| **使用者系統** | 註冊/登入/登出 | ✅ | ASP.NET Core Identity |
| | 個人資料管理 | ✅ | 姓名、電話、生日、性別 |
| | 訂單歷史查詢 | ✅ | 僅能查看自己的訂單 |
| **專輯展示** | 列表瀏覽 | ✅ | 響應式卡片設計 |
| | 雙分類篩選 | ✅ | 藝人分類 + 商品類型（階層式） |
| | 關鍵字搜尋 | ✅ | 標題、演出者 |
| | 排序功能 | ✅ | 預設/最新/價格（升/降） |
| **購物車** | 加入商品 | ✅ | Ajax + Modal 彈窗 |
| | 數量管理 | ✅ | 含庫存驗證 |
| | 購物車徽章 | ✅ | View Component 即時更新 |
| **訂單系統** | 建立訂單 | ✅ | **資料庫交易保護** |
| | 庫存扣除 | ✅ | **樂觀並發控制** |
| | 訂單追蹤 | ✅ | 5 種狀態（待處理→已完成） |
| **後台管理** | 儀表板 | ✅ | 統計資訊（商品/訂單/銷售額） |
| | 商品管理 | ✅ | CRUD + 雙分類 |
| | 訂單管理 | ✅ | 查看/更新狀態 |
| | 使用者管理 | ✅ | 角色切換（Admin/User） |
| **進階功能** | 商品分頁 | ⏳ | 待開發 |
| | 圖片上傳 | ⏳ | 待開發 |
| | Email 通知 | ⏳ | 待開發 |

---

## 🏗️ 系統架構

### 三層式架構

```
┌─────────────────────────────────────────────────────────┐
│  展示層 (Presentation Layer)                              │
│  Controllers/ + Views/                                   │
│  - 處理 HTTP 請求與回應                                    │
│  - 不包含商業邏輯                                          │
└─────────────────────────────────────────────────────────┘
                         ▼
┌─────────────────────────────────────────────────────────┐
│  商業邏輯層 (Business Logic Layer)                         │
│  Services/ (Interface + Implementation)                 │
│  - 業務規則與驗證                                          │
│  - 協調展示層與資料層                                       │
└─────────────────────────────────────────────────────────┘
                         ▼
┌─────────────────────────────────────────────────────────┐
│  資料存取層 (Data Access Layer)                            │
│  Repositories/ + Data/                                   │
│  - 資料庫 CRUD 操作                                        │
│  - Repository 模式                                        │
└─────────────────────────────────────────────────────────┘
```

### 核心設計模式

| 模式 | 實作位置 | 目的 |
|------|---------|------|
| **Repository Pattern** | `Repositories/` | 封裝資料存取邏輯 |
| **Service Layer** | `Services/` | 封裝商業邏輯 |
| **Dependency Injection** | `Program.cs` | 降低耦合度 |
| **Factory Pattern** | `IDbContextFactory<T>` | 避免並行問題 |
| **View Components** | `ViewComponents/` | 可重複使用的 UI 元件 |

---

## 🚀 快速開始

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
  }
}
```

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

## 📁 專案結構

```
MusicShop/
├── Controllers/          # 展示層 - MVC 控制器
│   ├── AccountController.cs    # 帳戶管理（註冊、登入、個人資料）
│   ├── AdminController.cs      # 後台管理（商品、分類、訂單、使用者）
│   ├── AlbumController.cs      # 專輯瀏覽與詳細頁面
│   ├── CartController.cs       # 購物車（✅ 已完成）
│   ├── HomeController.cs       # 首頁（幻燈片、最新商品）
│   └── OrderController.cs      # 訂單管理（✅ 已完成）
├── Helpers/             # 共用工具類別
│   └── ValidationHelper.cs     # 驗證工具（v1.1.1 新增）
├── Services/            # 商業邏輯層
│   ├── Interface/       # 服務介面
│   │   ├── IAlbumService.cs
│   │   ├── ICartService.cs
│   │   ├── IOrderService.cs
│   │   ├── IArtistCategoryService.cs
│   │   ├── IProductTypeService.cs
│   │   ├── IStatisticsService.cs
│   │   └── IUserService.cs
│   └── Implementation/  # 服務實作
│       ├── AlbumService.cs
│       ├── CartService.cs
│       ├── OrderService.cs
│       ├── ArtistCategoryService.cs
│       ├── ProductTypeService.cs
│       ├── StatisticsService.cs
│       └── UserService.cs
├── Repositories/        # 資料存取層
│   ├── Interface/       # Repository 介面
│   │   ├── IAlbumRepository.cs
│   │   ├── ICartRepository.cs
│   │   ├── IOrderRepository.cs
│   │   ├── IArtistCategoryRepository.cs
│   │   ├── IProductTypeRepository.cs
│   │   └── IStatisticsRepository.cs
│   └── Implementation/  # Repository 實作
│       ├── AlbumRepository.cs
│       ├── CartRepository.cs
│       ├── OrderRepository.cs
│       ├── ArtistCategoryRepository.cs
│       ├── ProductTypeRepository.cs
│       └── StatisticsRepository.cs
├── Models/              # 資料模型
│   ├── AppUser.cs       # 使用者模型（擴展 IdentityUser）
│   ├── Album.cs         # 專輯模型
│   ├── ArtistCategory.cs # 藝人分類模型
│   ├── ProductType.cs   # 商品類型模型（階層式）
│   ├── Order.cs         # 訂單模型
│   ├── OrderItem.cs     # 訂單項目
│   └── CartItem.cs      # 購物車項目
├── ViewModels/          # 檢視模型
│   ├── RegisterViewModel.cs
│   ├── LoginViewModel.cs
│   ├── AlbumViewModel.cs
│   ├── AccountIndexViewModel.cs
│   ├── EditProfileViewModel.cs
│   └── UserManagementViewModel.cs
├── ViewComponents/      # View Components
│   └── CartBadgeViewComponent.cs  # 購物車數量徽章
├── Views/               # Razor 檢視
│   ├── Home/            # 首頁檢視（Index、Privacy）
│   ├── Account/         # 帳戶檢視（Register、Login、Index、Edit）
│   ├── Album/           # 專輯檢視（Index、Detail）
│   ├── Cart/            # 購物車檢視（Index）
│   ├── Order/           # 訂單檢視（Index、Detail）
│   ├── Admin/           # 後台管理檢視
│   ├── Shared/          # 共用檢視（_Layout、_AddToCartModal）
│   └── Components/      # View Component 檢視
├── Data/                # 資料庫上下文與初始化
│   ├── ApplicationDbContext.cs
│   └── DbInitializer.cs # 資料庫初始化（角色、管理員、範例資料）
├── Migrations/          # EF Core 遷移檔案
├── wwwroot/            # 靜態檔案
│   ├── css/            # CSS 樣式
│   │   ├── site.css    # 全域樣式
│   │   └── pages/      # 頁面專屬樣式
│   │       ├── album-index.css  # 專輯列表頁樣式（v1.1.1 新增）
│   │       └── cart.css         # 購物車頁面樣式（v1.2.0 優化）
│   ├── js/             # JavaScript
│   │   ├── add-to-cart-modal.js
│   │   └── pages/      # 頁面專屬 JS
│   ├── images/         # 圖片資源
│   │   ├── albums/     # 專輯封面
│   │   └── banners/    # 輪播圖片
│   └── lib/            # Bootstrap、jQuery
├── appsettings.example.json   # 設定檔範例
├── CLAUDE.md           # Claude Code 專案指引
└── README.md           # 本文件
```

---

## 🎓 開發指引與最佳實踐

### 三層式架構規範

**Controller 層（展示層）**
- ✅ 僅負責處理 HTTP 請求與回應
- ✅ 不包含商業邏輯，僅負責資料傳遞與顯示
- ✅ 透過依賴注入使用 Service 層服務
- ❌ **禁止**直接呼叫 Repository 層

**Service 層（商業邏輯層）**
- ✅ 包含所有業務規則與驗證邏輯
- ✅ 協調展示層與資料層之間的互動
- ✅ 介面與實作分離（Interface/ 與 Implementation/）
- ✅ 使用 ValidationHelper 進行參數驗證
- ❌ **禁止**直接處理 HTTP 相關邏輯

**Repository 層（資料存取層）**
- ✅ 負責資料庫 CRUD 操作
- ✅ 使用 Repository 模式封裝資料存取
- ✅ 介面與實作分離（Interface/ 與 Implementation/）
- ✅ 使用 `IDbContextFactory<ApplicationDbContext>` 建立 DbContext
- ❌ **禁止**包含業務邏輯

### 關鍵設計模式實作

| 模式 | 說明 | 範例 |
|------|------|------|
| **Repository Pattern** | 封裝資料存取邏輯 | `IAlbumRepository` → `AlbumRepository` |
| **Service Layer** | 封裝商業邏輯 | `IOrderService` → `OrderService` |
| **Dependency Injection** | 降低耦合度 | `Program.cs` 註冊所有服務 |
| **Factory Pattern** | 避免並行問題 | `IDbContextFactory<ApplicationDbContext>` |
| **View Components** | 可重複使用的 UI 元件 | `CartBadgeViewComponent` |
| **ValidationHelper** | 集中驗證邏輯 | 9 個靜態驗證方法 |

### 資料庫操作最佳實踐

| 實踐 | 說明 | 範例 |
|------|------|------|
| **預先載入（Eager Loading）** | 避免 N+1 查詢 | `.Include(o => o.OrderItems)` |
| **UTC 時間** | 避免時區問題 | `DateTime.UtcNow` |
| **交易保護** | 確保原子性 | `CreateOrderWithTransactionAsync` |
| **樂觀並發控制** | 防止並發衝突 | `[Timestamp]` RowVersion |
| **Dictionary 快取** | 避免重複查詢 | OrderService 中的專輯快取 |
| **IDbContextFactory** | 並行處理 | 每次操作建立新的 DbContext |

### 程式碼品質規範

**驗證邏輯**
```csharp
// ✅ 正確：使用 ValidationHelper
ValidationHelper.ValidateString(album.Title, "專輯標題", 200, nameof(album.Title));

// ❌ 錯誤：重複撰寫驗證邏輯
if (string.IsNullOrEmpty(album.Title))
    throw new ArgumentException("專輯標題不能為空");
```

**DRY 原則（Don't Repeat Yourself）**
```csharp
// ✅ 正確：萃取為輔助方法
private string GetAuthorizedUserId()
{
    var userId = _userManager.GetUserId(User);
    if (string.IsNullOrEmpty(userId))
        throw new UnauthorizedAccessException("使用者未登入");
    return userId;
}

// ❌ 錯誤：重複程式碼
var userId = _userManager.GetUserId(User);
if (string.IsNullOrEmpty(userId))
    return Unauthorized();
```

**交易處理**
```csharp
// ✅ 正確：使用交易確保原子性
await _orderRepository.CreateOrderWithTransactionAsync(order, userId);

// ❌ 錯誤：分離操作（可能導致不一致）
await _orderRepository.CreateOrderAsync(order);
await _albumRepository.DeductStockAsync(orderItems);
await _cartRepository.ClearCartAsync(userId);
```

---

## 🗄️ 資料庫架構

### 主要資料表

| 資料表 | 說明 | 關鍵欄位 |
|--------|------|---------|
| **Albums** | 專輯資訊 | Title, Price, Stock, **RowVersion** (並發控制) |
| **ArtistCategories** | 藝人分類 | Name（BOY GROUP/GIRL GROUP/SOLO） |
| **ProductTypes** | 商品類型（階層式） | Name, ParentId（支援父子關係） |
| **Orders** | 訂單主檔 | UserId, TotalAmount, Status |
| **OrderItems** | 訂單明細 | OrderId, AlbumId, Quantity, UnitPrice |
| **CartItems** | 購物車項目 | UserId, AlbumId, Quantity |
| **AspNetUsers** | 使用者資料 | Email, PasswordHash, FullName, PhoneNumber |
| **AspNetRoles** | 角色資料 | User, Admin |

### 關鍵關聯

| 關聯類型 | 說明 | 刪除行為 |
|---------|------|---------|
| Album → ArtistCategory | 多對一 | Restrict |
| Album → ProductType | 多對一 | Restrict |
| ProductType → ProductType | 自我關聯（父子） | Restrict |
| Order → User | 多對一 | Cascade |
| Order → OrderItems | 一對多 | Cascade |
| OrderItem → Album | 多對一 | Restrict |
| CartItem → User | 多對一 | Cascade |
| CartItem → Album | 多對一 | Restrict |

---

## 🔄 核心業務流程

### 購物車到訂單流程

```
使用者登入
    ↓
瀏覽專輯 → 加入購物車（庫存驗證）
    ↓
檢視購物車 → 調整數量（庫存檢查）
    ↓
前往結帳 → 填寫收件資訊
    ↓
送出訂單 → CreateOrderWithTransactionAsync
    ├── 1. 建立訂單記錄
    ├── 2. 扣除專輯庫存（樂觀並發控制）
    ├── 3. 清空購物車
    └── 4. 提交交易（ACID）
    ↓
訂單成立 → 訂單詳細頁面
    ↓
訂單追蹤（Pending → Paid → Shipped → Completed）
```

### 資料庫交易保護機制

**問題場景**：兩個使用者同時下單同一商品（庫存僅剩 1 件）

**未使用交易（❌ 錯誤）**：
```
時間軸    使用者 A                     使用者 B
T1       讀取庫存（1 件）
T2                                   讀取庫存（1 件）
T3       建立訂單
T4                                   建立訂單
T5       扣除庫存（0 件）
T6                                   扣除庫存（-1 件）❌ 超賣！
```

**使用交易（✅ 正確）**：
```
時間軸    使用者 A                     使用者 B
T1       開始交易
T2       鎖定庫存（1 件）
T3       建立訂單                     開始交易（等待 A 釋放鎖）
T4       扣除庫存（0 件）
T5       提交交易
T6                                   檢查庫存（0 件）
T7                                   ❌ 拋出例外：庫存不足
T8                                   回滾交易
```

### 樂觀並發控制（Optimistic Concurrency）

**Album 模型並發保護**：
```csharp
public class Album
{
    public int Id { get; set; }
    public int? Stock { get; set; }

    [Timestamp]
    public byte[]? RowVersion { get; set; }  // 自動檢測並發修改
}
```

**運作機制**：
1. 讀取專輯時，同時讀取 RowVersion
2. 更新庫存時，檢查 RowVersion 是否改變
3. 若 RowVersion 不同 → 拋出 `DbUpdateConcurrencyException`
4. 交易回滾，防止資料不一致

---

## ⚙️ 開發注意事項

### 架構規範

| 規範 | 說明 |
|------|------|
| **嚴格遵守三層式架構** | Controller 不直接呼叫 Repository，必須透過 Service 層 |
| **所有新功能建立 Service** | 建立對應的 Service 介面與實作 |
| **Repository 只負責資料存取** | 不包含業務邏輯 |
| **使用 IDbContextFactory** | 建立 DbContext 實例，避免並行問題 |
| **使用 ValidationHelper** | Service 層方法開頭進行參數驗證 |

### 資料庫規範

| 規範 | 說明 |
|------|------|
| **統一使用 DateTime.UtcNow** | 避免時區問題 |
| **避免 N+1 查詢** | 適當使用 `.Include()` 或快取機制 |
| **價格欄位使用 decimal(10,2)** | 確保小數精度 |
| **關鍵操作使用交易** | 確保 ACID 特性 |
| **高並發操作加並發控制** | 使用 `[Timestamp]` 防止衝突 |

### 安全性規範

| 規範 | 說明 |
|------|------|
| **需登入功能加 [Authorize]** | 保護需要認證的功能 |
| **後台功能加角色驗證** | `[Authorize(Roles = "Admin")]` |
| **驗證使用者權限** | 避免 A 使用者操作 B 使用者的資料 |
| **使用 [ValidateAntiForgeryToken]** | 防止 CSRF 攻擊 |
| **密碼使用 Identity 加密** | PBKDF2 + 唯一 Salt |

### 程式碼品質規範

| 規範 | 說明 |
|------|------|
| **DRY 原則** | 避免重複程式碼，萃取為方法或工具類別 |
| **單一職責原則** | 每個類別/方法只負責一件事 |
| **介面與實作分離** | Service 和 Repository 都有介面 |
| **依賴注入** | 所有服務透過介面注入 |
| **繁體中文註解** | 程式碼包含清楚的註解說明 |

---

## 📜 版本歷史

### v1.2.0 (2026-03-09) - 程式碼品質與安全性優化

**🔒 資料庫交易保護**
- ✅ 實作 `CreateOrderWithTransactionAsync` 方法
- ✅ 確保訂單建立、庫存扣除、購物車清空的原子性（ACID）
- ✅ 防止並發訂單導致超賣問題

**⚡ 並發控制機制**
- ✅ Album 模型新增 `[Timestamp]` RowVersion 欄位
- ✅ 樂觀並發控制防止庫存更新衝突
- ✅ 資料庫層級的 rowversion 自動檢測並發修改

**🧹 程式碼重構（DRY 原則）**
- ✅ 移除 Controller 層重複驗證邏輯（統一由 Service 層處理）
- ✅ 萃取 `GetAuthorizedUserId()` 輔助方法（消除 8 處重複）
- ✅ 萃取 `ReturnCheckoutViewWithDataAsync()` 錯誤處理方法
- ✅ 優化購物車總計查詢（避免重複資料庫查詢）

**🎨 UI 色彩優化**
- ✅ 購物車與結帳頁面色彩調整（移除刺眼藍紫漸層）
- ✅ 統一使用柔和紫色（#b19cd9）與導覽列、Footer 一致

---

### v1.1.1 (2026-03-06) - ValidationHelper 與 UI/UX 優化

**程式碼品質**
- ✅ 新增 `ValidationHelper` 工具類別（9 個靜態驗證方法）
- ✅ 重構 8 個 Service 類別使用 ValidationHelper
- ✅ 驗證程式碼減少 67%（~150 行 → ~50 行）

**UI/UX 改善**
- ✅ 重新設計專輯列表搜尋介面（現代化卡片式設計）
- ✅ 新增專屬樣式檔案 `album-index.css`（380+ 行）
- ✅ 雙下拉式排序系統 + 格狀/清單視圖切換

---

### v1.1.0 (2026-03-04) - 三層式架構與核心功能

**架構重構**
- ✅ 實作完整三層式架構（Controller → Service → Repository）
- ✅ 雙分類系統（ArtistCategory + 階層式 ProductType）

**功能實作**
- ✅ 購物車系統（完整 CRUD + 庫存驗證）
- ✅ 訂單系統（結帳、狀態追蹤、權限控制）
- ✅ 後台管理系統（儀表板、商品、訂單、使用者管理）
- ✅ 個人功能（帳號總覽、資料編輯、訂單歷史）
- ✅ DbInitializer（自動初始化角色、管理員、範例資料）

---

### v1.0.0 (2026-03-03) - 初始版本

**核心功能**
- ✅ 使用者認證系統（ASP.NET Core Identity）
- ✅ 專輯展示與搜尋
- ✅ 首頁設計（幻燈片輪播 + 最新商品）
- ✅ 隱私權政策頁面

---

## 💻 開發指令

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

## 🔐 安全性特性

| 項目 | 實作 |
|------|------|
| **密碼加密** | PBKDF2 演算法 + 唯一 Salt |
| **CSRF 防護** | `[ValidateAntiForgeryToken]` |
| **SQL 注入防護** | EF Core 參數化查詢 |
| **XSS 防護** | Razor 自動編碼 |
| **敏感資訊保護** | `appsettings.json` 已加入 `.gitignore` |
| **權限控制** | `[Authorize]` + 角色驗證（Admin/User） |
| **並發控制** | 樂觀並發（`[Timestamp]` RowVersion） |
| **交易保護** | 資料庫交易確保原子性 |

---

## 📝 訂單狀態說明

| 狀態 | 說明 |
|------|------|
| **Pending** | 待處理 |
| **Paid** | 已付款 |
| **Shipped** | 已出貨 |
| **Completed** | 已完成 |
| **Cancelled** | 已取消 |

---

## 👤 作者

**Hou Wen Chia**

---

## 📄 授權

此專案為學習與教育用途。

---

## 🔗 相關連結

### 專案文件
- [CLAUDE.md](/CLAUDE.md) - Claude Code 專案指引
- [appsettings.example.json](/appsettings.example.json) - 設定檔範例

### 參考資源
- [K-MONSTAR](https://www.k-monstar.com/) - 韓國音樂專輯線上商店
  - 本專案的靈感來源與功能參考
  - 專輯分類、商品展示、購物流程等設計參考
