# MusicShop - 線上音樂專輯商店

一個基於 ASP.NET Core MVC 開發的線上音樂專輯電子商務平台，提供完整的音樂專輯購物體驗。

## 專案目標

建立一個功能完整的音樂專輯電商網站，提供：
- 會員系統（註冊、登入、個人資料管理）
- 專輯展示與搜尋
- 購物車功能
- 訂單管理系統
- 後台管理功能

## 功能開發狀態

### ✅ 已完成功能（v1.1）

- **使用者認證與個人功能**
  - ✅ 使用者註冊（含表單驗證、角色自動分配）
  - ✅ 使用者登入/登出
  - ✅ ASP.NET Core Identity 整合
  - ✅ 密碼加密儲存（PBKDF2）
  - ✅ 存取權限控制
  - ✅ 導航欄位顯示登入狀態
  - ✅ 個人帳號總覽（訂單統計、最近訂單）
  - ✅ 個人資料編輯（姓名、電話、生日、性別）
  - ✅ 個人訂單歷史查詢

- **專輯展示系統**
  - ✅ 專輯列表瀏覽（響應式卡片設計）
  - ✅ 專輯詳細資訊頁面
  - ✅ **雙分類系統篩選**（藝人分類 + 商品類型）
  - ✅ **階層式商品類型**（父分類與子分類）
  - ✅ 關鍵字搜尋（標題、演出者）
  - ✅ 庫存顯示與庫存不足提示

- **購物車系統**
  - ✅ 加入購物車（含庫存驗證）
  - ✅ 購物車 Modal 彈窗（Ajax 即時加入）
  - ✅ 購物車數量徽章（View Component 即時更新）
  - ✅ 檢視購物車內容與總金額
  - ✅ 修改商品數量（含庫存檢查）
  - ✅ 移除購物車商品
  - ✅ 清空購物車

- **訂單管理系統**
  - ✅ 結帳流程（從購物車建立訂單）
  - ✅ 訂單建立與確認
  - ✅ 自動扣除庫存
  - ✅ 訂單歷史查詢（僅能查看自己的訂單）
  - ✅ 訂單詳細資訊檢視
  - ✅ 訂單狀態追蹤（待處理/已付款/已出貨/已完成/已取消）
  - ✅ 權限控制（無法查看他人訂單）

- **後台管理系統**
  - ✅ 後台儀表板（統計資料：商品數、訂單數、使用者數、總銷售額）
  - ✅ 專輯管理（新增、編輯、刪除、列表）
  - ✅ **雙分類管理**：
    - ✅ 藝人分類 CRUD（BOY GROUP、GIRL GROUP、SOLO）
    - ✅ 商品類型 CRUD（階層式架構，支援父子關係）
    - ✅ 級聯下拉選單（選擇父分類後動態載入子分類）
  - ✅ 訂單管理（查看所有訂單、更新訂單狀態）
  - ✅ 使用者管理（查看所有使用者、切換管理員角色）
  - ✅ 管理員選單元件（View Component）

- **首頁設計**
  - ✅ 幻燈片輪播（3 個輪播頁面）
  - ✅ 最新商品展示（顯示最新 2 個專輯）
  - ✅ 商品卡片（包含封面、價格、庫存資訊）
  - ✅ 加入購物車按鈕與 Modal
  - ✅ 響應式版面設計

- **架構與資料模型**
  - ✅ **完整三層式架構**（Controller → Service → Repository）
  - ✅ Repository 模式（介面與實作分離）
  - ✅ Service 層（商業邏輯封裝）
  - ✅ 使用者模型（AppUser，擴展欄位：生日、性別、電話）
  - ✅ 專輯模型（Album）
  - ✅ 雙分類模型（ArtistCategory + ProductType）
  - ✅ 訂單模型（Order、OrderItem）
  - ✅ 購物車模型（CartItem）
  - ✅ 完整的關聯設定（一對多、多對一、串聯刪除、限制刪除）
  - ✅ **DbInitializer**（自動初始化角色、管理員、分類、範例資料）

- **效能與品質優化**
  - ✅ 修正 N+1 查詢問題（使用 Dictionary 快取）
  - ✅ 統一使用 UTC 時間（DateTime.UtcNow）
  - ✅ IDbContextFactory 模式（更好的並行處理）
  - ✅ View Components 可重複使用元件

### 🚧 待開發功能

- **進階功能**
  - ⏳ 商品分頁顯示
  - ⏳ 商品圖片上傳功能
  - ⏳ Email 通知（訂單確認、狀態變更）
  - ⏳ 優惠券與折扣碼
  - ⏳ 商品評價與評論系統
  - ⏳ 多語系支援
  - ⏳ 深色模式

## 技術堆疊

- **框架**：ASP.NET Core MVC (.NET 10.0)
- **架構模式**：三層式架構（Presentation → Business Logic → Data Access）
- **資料庫**：SQL Server
- **ORM**：Entity Framework Core 10.0.3
- **認證**：ASP.NET Core Identity
- **前端**：Razor Views、Bootstrap 5、jQuery、Ajax
- **設計模式**：Repository Pattern、Dependency Injection、Factory Pattern（IDbContextFactory）

## 系統需求

### 必要元件

- **.NET 10.0 SDK**
  - 下載：[https://dotnet.microsoft.com/download/dotnet/10.0](https://dotnet.microsoft.com/download/dotnet/10.0)
  - 支援 Windows、macOS (包含 Apple Silicon M1/M2/M3) 及 Linux

- **SQL Server**
  - 本機安裝或遠端連線皆可
  - 支援 SQL Server 2019 以上版本
  - 確保開啟 TCP/IP 連線（遠端連線需求）
  - 預設連接埠：1433

### 開發環境

- **作業系統**：Windows 10/11、macOS 11+ (含 Apple Silicon)、Linux
- **編輯器 / IDE**：
  - **Visual Studio Code**（建議，跨平台輕量化）
    - 必要擴充套件：C# Dev Kit、C# Extension
  - Visual Studio 2025（完整 IDE，僅 Windows/macOS）

### 本專案開發環境

- **硬體**：MacBook M1 Air
- **作業系統**：macOS
- **編輯器**：Visual Studio Code
- **資料庫**：遠端 SQL Server（透過 TCP/IP 連線）

### macOS (Apple Silicon) 開發注意事項

1. **安裝 .NET SDK**：
   ```bash
   # 使用 Homebrew 安裝（推薦）
   brew install dotnet

   # 驗證安裝
   dotnet --version
   ```

2. **遠端 SQL Server 連線**：
   - 確認 SQL Server 已啟用 TCP/IP 協定
   - 確認防火牆允許 1433 連接埠
   - 在 `appsettings.json` 中設定遠端伺服器位址（參考下方快速開始）

3. **VS Code 擴充套件**：
   - C# Dev Kit
   - C# Extension (由 Microsoft 提供)
   - NuGet Package Manager
   - SQL Server (mssql)（可選，用於資料庫管理）

4. **M1/M2/M3 相容性**：
   - .NET 6.0 以上版本原生支援 Apple Silicon
   - Entity Framework Core Tools 在 ARM64 架構下運作正常

## 快速開始

### 1. 複製專案

```bash
git clone <repository-url>
cd MusicShop
```

### 2. 設定資料庫連線

複製 `appsettings.example.json` 並重新命名為 `appsettings.json`，然後修改資料庫連線字串：

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=<你的伺服器>;Database=kalbum;User Id=<使用者名稱>;Password=<密碼>;Integrated Security=False;TrustServerCertificate=True;"
  }
}
```

**參數說明**：
- `Server`：SQL Server 位址（例如：localhost、192.168.x.x 或遠端網域）
- `Database`：資料庫名稱（預設為 kalbum）
- `User Id`：SQL Server 登入帳號
- `Password`：SQL Server 登入密碼
- `TrustServerCertificate=True`：允許自簽憑證（開發環境使用）

**注意**：`appsettings.json` 已加入 `.gitignore`，不會被提交到版控系統。

### 3. 還原套件

```bash
dotnet restore
```

### 4. 建立資料庫

```bash
dotnet ef database update
```

### 5. 執行應用程式

```bash
# 一般執行
dotnet run

# 或使用開發模式（自動重新載入）
dotnet watch run
```

應用程式將在 `http://localhost:5281` 啟動。

## 專案結構

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
│   │       └── album-index.css  # 專輯列表頁樣式（v1.1.1 新增）
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

## 資料庫架構

### 主要資料表

- **Albums**：專輯資訊（標題、演出者、價格、庫存、封面圖片 URL、藝人分類 ID、商品類型 ID）
- **ArtistCategories**：藝人分類（BOY GROUP、GIRL GROUP、SOLO）
- **ProductTypes**：商品類型（階層式架構，包含父子關係）
  - 父分類：K-ALBUM、K-MAGAZINE、K-MERCH、K-EVENT
  - 子分類：ALBUM、PHOTOBOOK、DVD、寫真雜誌、官方周邊、演唱會周邊等
- **Orders**：訂單主檔（訂單編號、使用者、日期、狀態、總金額）
- **OrderItems**：訂單明細（訂單 ID、專輯 ID、數量、單價）
- **CartItems**：購物車項目（使用者 ID、專輯 ID、數量、加入時間）
- **AspNetUsers**：使用者資料（Identity 系統自動建立，擴展欄位：姓名、電話、生日、性別、註冊時間）
- **AspNetRoles**：角色資料（User、Admin）

### 關鍵關聯

- 專輯 ←→ 藝人分類（多對一，限制刪除）
- 專輯 ←→ 商品類型（多對一，限制刪除）
- 商品類型 ←→ 商品類型（自我關聯：父分類與子分類）
- 訂單 ←→ 使用者（多對一，串聯刪除）
- 訂單 ←→ 訂單項目（一對多，串聯刪除）
- 訂單項目 ←→ 專輯（多對一，限制刪除）
- 購物車項目 ←→ 使用者（多對一，串聯刪除）
- 購物車項目 ←→ 專輯（多對一，限制刪除）

## 開發指令

### 建置專案

```bash
dotnet build
```

### 執行專案

```bash
# 一般執行
dotnet run

# 開發模式（檔案變更時自動重新載入）
dotnet watch run
```

### 資料庫遷移

```bash
# 新增遷移
dotnet ef migrations add <遷移名稱>

# 套用遷移
dotnet ef database update

# 移除最後一次遷移（需尚未套用到資料庫）
dotnet ef migrations remove

# 刪除資料庫
dotnet ef database drop
```

## 密碼規則

預設密碼需求：
- 最少 6 個字元
- 需包含至少一個數字
- 不需要大寫字母
- 不需要特殊符號

可在 `Program.cs` 的 Identity 設定中調整密碼強度要求。

## 訂單狀態

- `Pending`：待處理
- `Paid`：已付款
- `Shipped`：已出貨
- `Completed`：已完成
- `Cancelled`：已取消

## 使用者認證系統詳細說明

### 技術架構

本專案使用 **ASP.NET Core Identity** 作為認證與授權框架，提供完整的使用者管理功能。

#### Identity 設定（Program.cs）

```csharp
// 註冊 Identity 服務
builder.Services.AddIdentity<AppUser, IdentityRole>(options =>
{
    // 密碼強度設定
    options.Password.RequireDigit = true;           // 需要數字
    options.Password.RequiredLength = 6;            // 最少 6 個字元
    options.Password.RequireNonAlphanumeric = false; // 不需特殊符號
    options.Password.RequireUppercase = false;      // 不需大寫字母
})
.AddEntityFrameworkStores<ApplicationDbContext>()  // 使用 EF Core 儲存
.AddDefaultTokenProviders();                       // 加入預設 Token 提供者

// Cookie 認證設定
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";          // 登入頁面路徑
    options.LogoutPath = "/Account/Logout";        // 登出路徑
    options.AccessDeniedPath = "/Account/AccessDenied"; // 拒絕存取頁面
});
```

### 使用者模型（AppUser）

擴展 Identity 的 `IdentityUser` 類別，新增自訂欄位：

```csharp
public class AppUser : IdentityUser
{
    public string? FullName { get; set; }              // 使用者全名
    public DateTime RegisteredAt { get; set; }         // 註冊時間
    public ICollection<Order> Orders { get; set; }     // 訂單關聯
    public ICollection<CartItem> CartItems { get; set; } // 購物車關聯
}
```

繼承自 `IdentityUser` 後，自動擁有以下欄位：
- `Id`：使用者唯一識別碼
- `UserName`：使用者名稱（本專案使用 Email）
- `Email`：電子郵件
- `PasswordHash`：加密後的密碼
- `PhoneNumber`、`EmailConfirmed` 等其他欄位

### 註冊流程實作

```csharp
[HttpPost, ValidateAntiForgeryToken]
public async Task<IActionResult> Register(RegisterViewModel model)
{
    if (!ModelState.IsValid) return View(model);

    var user = new AppUser
    {
        UserName = model.Email,
        Email = model.Email,
        FullName = model.FullName,
        RegisteredAt = DateTime.Now
    };

    var result = await _userManager.CreateAsync(user, model.Password);

    if (result.Succeeded)
    {
        await _signInManager.SignInAsync(user, isPersistent: false);
        return RedirectToAction("Index", "Home");
    }

    foreach (var error in result.Errors)
        ModelState.AddModelError(string.Empty, error.Description);

    return View(model);
}
```

### 登入流程實作

```csharp
[HttpPost, ValidateAntiForgeryToken]
public async Task<IActionResult> Login(LoginViewModel model)
{
    if (!ModelState.IsValid) return View(model);

    var result = await _signInManager.PasswordSignInAsync(
        model.Email,
        model.Password,
        model.RememberMe,
        lockoutOnFailure: false
    );

    if (result.Succeeded)
        return RedirectToAction("Index", "Home");

    ModelState.AddModelError(string.Empty, "帳號或密碼錯誤");
    return View(model);
}
```

### 密碼安全性

- 使用 **PBKDF2** 演算法進行密碼雜湊
- 每個密碼都有唯一的 Salt（鹽值）
- 即使資料庫外洩，攻擊者也無法反推原始密碼
- 密碼驗證時會重新計算雜湊值進行比對

## 核心業務流程

### 1. 使用者註冊與登入流程
```
訪客 → 註冊頁面 (/Account/Register)
    → 填寫資料（Email、密碼、姓名）
    → ModelState 驗證（欄位必填、Email 格式、密碼長度等）
    → UserManager.CreateAsync() 建立使用者（密碼自動加密）
    → SignInManager.SignInAsync() 自動登入（建立 Cookie）
    → 導向首頁
```

```
使用者 → 登入頁面 (/Account/Login)
    → 輸入 Email 和密碼
    → SignInManager.PasswordSignInAsync() 驗證
    → 驗證成功：建立認證 Cookie → 導向首頁
    → 驗證失敗：顯示錯誤訊息
```

### 2. 專輯瀏覽與搜尋流程
```
使用者 → 首頁 (/Home/Index)
    → 瀏覽幻燈片與最新商品
    → 點擊「查看所有專輯」→ 專輯列表 (/Album/Index)
    → 選擇分類或輸入關鍵字搜尋
    → 點擊專輯卡片 → 檢視專輯詳細資訊 (/Album/Detail/{id})
    → 查看價格、庫存、描述等資訊
```

### 3. 購物車流程（待開發）
```
使用者（已登入）→ 專輯詳細頁面
    → 點擊「加入購物車」
    → 查看購物車 (/Cart/Index)
    → 調整數量或移除商品
    → 前往結帳 (/Order/Checkout)
```

### 4. 訂單處理流程（待開發）
```
使用者 → 結帳頁面
    → 確認訂單資訊
    → 送出訂單
    → 訂單狀態：待處理 → 已付款 → 已出貨 → 已完成
    → 查看訂單歷史 (/Order/MyOrders)
```

## 資料庫設計重點

### 庫存管理
- Album 資料表的 `Stock` 欄位記錄庫存數量
- 加入購物車時需檢查庫存
- 訂單成立時需扣除庫存

### 購物車設計
- CartItem 與使用者綁定（UserId）
- 每個使用者可有多個購物車項目
- 購物車項目包含專輯 ID、數量、加入時間

### 訂單設計
- Order：訂單主檔（訂單編號、使用者、總金額、狀態）
- OrderItem：訂單明細（訂購的專輯、數量、單價）

## 安全性考量

- ✅ 密碼使用 ASP.NET Core Identity 加密儲存（PBKDF2）
- ✅ 防止 CSRF 攻擊（ValidateAntiForgeryToken）
- ✅ SQL 注入防護（使用 EF Core 參數化查詢）
- ✅ XSS 防護（Razor 自動編碼）
- ✅ 敏感資訊保護（appsettings.json 已加入 .gitignore）
- ⏳ 購物車操作需登入驗證（[Authorize]）
- ⏳ 訂單查看權限控制（僅能查看自己的訂單）

## 隱私權政策

本專案包含完整的隱私權政策頁面 (`/Home/Privacy`)，涵蓋：
- 資料收集範圍
- 資料使用方式
- 資料保護措施
- Cookie 使用說明
- 使用者權利
- 聯絡方式

## 效能優化建議

- ✅ 使用 `.Include()` 預先載入關聯資料（避免 N+1 查詢）
- ⏳ 商品列表分頁顯示（目前未實作）
- ⏳ 購物車數量快取（減少資料庫查詢）
- ⏳ 圖片使用 CDN 或優化壓縮
- ⏳ 資料庫索引優化（CategoryId、UserId 等外鍵欄位）

## 測試建議

建議為以下功能撰寫單元測試和整合測試：
- 使用者註冊驗證邏輯
- 購物車加入/移除/更新數量
- 訂單建立流程
- 庫存扣除邏輯
- 搜尋與篩選功能

## 部署注意事項

### 正式環境檢查清單
- [ ] 修改 `appsettings.json` 中的資料庫連線字串
- [ ] 啟用 HTTPS 重新導向（取消註解 `Program.cs` 第 43 行）
- [ ] 設定正式環境的密碼強度要求
- [ ] 設定 SMTP 服務（若需發送 Email）
- [ ] 檢查並移除敏感資訊（密碼、連線字串）
- [ ] 設定適當的 CORS 政策
- [ ] 啟用日誌記錄（Logging）

## 常見問題

### 資料庫連線失敗
確認 SQL Server 服務是否啟動，以及 `appsettings.json` 中的連線字串是否正確。

### 遷移失敗
```bash
# 刪除 Migrations 資料夾中的所有檔案
# 重新建立遷移
dotnet ef migrations add InitialCreate
dotnet ef database update
```

### 無法登入
檢查密碼是否符合規則（至少 6 個字元且包含數字）。

### 頁面底部內容被 Footer 覆蓋
已在 `_Layout.cshtml` 和 `site.css` 中調整底部間距，確保所有頁面都有足夠的留白空間。

## 作者

Hou Wen Chia

## 版本歷史

### v1.1.1 (2026-03-06)
程式碼品質與 UI/UX 優化版本：

**程式碼品質改進**
- ✅ 新增 ValidationHelper 工具類別（Helpers/ValidationHelper.cs）
  - 9 個靜態驗證方法：ValidateNotEmpty、ValidateMaxLength、ValidateString、ValidatePositive（decimal & int）、ValidateId、ValidateEntityExists、ValidateCondition、ValidateCollectionNotEmpty
  - 集中管理所有驗證邏輯，提升程式碼可維護性
- ✅ 重構 8 個 Service 類別使用 ValidationHelper
  - AlbumService、ArtistService、ArtistCategoryService、ProductTypeService
  - CartService、OrderService、StatisticsService、UserService
  - 驗證程式碼減少 67%（約 150 行 → 50 行）
  - 消除重複程式碼，提升一致性

**UI/UX 改善**
- ✅ 重新設計專輯列表搜尋介面（Views/Album/Index.cshtml）
  - 現代化卡片式搜尋區域（白色背景、圓角、陰影效果）
  - 搜尋輸入框加入圖示與優化的 focus 效果
  - 雙下拉式排序系統（預設排序 + 價格排序）
  - 分離式工具列（排序、篩選、商品數量、視圖切換）
  - 商品數量即時顯示
  - 格狀/清單視圖切換按鈕
- ✅ 新增專屬樣式檔案（wwwroot/css/pages/album-index.css）
  - 380+ 行專業 CSS 設計
  - 漸層色彩與動畫效果
  - 完整響應式設計（桌面/平板/手機）
  - 懸停效果與轉場動畫

**專案結構更新**
- ✅ 新增 Helpers/ 目錄（共用工具類別）
- ✅ 擴充 wwwroot/css/pages/ 目錄（頁面專屬樣式）

**建置狀態**
- ✅ 編譯成功（0 個錯誤、0 個警告）
- ✅ 架構完整性驗證通過（100% 符合三層式架構）

### v1.1.0 (2026-03-04)
重構與功能擴充版本，完成三層式架構與核心功能：

**架構改進**
- ✅ 實作完整三層式架構（Controller → Service → Repository）
- ✅ Repository 模式（介面與實作分離）
- ✅ Service 層封裝所有商業邏輯
- ✅ 使用 IDbContextFactory 模式提升並行處理能力
- ✅ 修正 ViewMdoels → ViewModels 目錄拼寫錯誤

**分類系統重構**
- ✅ 從單一 Category 改為雙分類系統
  - ArtistCategory（藝人分類）：BOY GROUP、GIRL GROUP、SOLO
  - ProductType（商品類型）：階層式架構，支援父子關係
- ✅ 4 個父分類 + 10 個子分類
- ✅ 級聯下拉選單（選擇父分類後動態載入子分類）

**購物車系統**（✅ 完整實作）
- ✅ 加入購物車（含庫存驗證）
- ✅ 購物車 Modal 彈窗（Ajax 即時加入）
- ✅ 購物車數量徽章（View Component 即時更新）
- ✅ 檢視購物車內容與總金額
- ✅ 修改商品數量（含庫存檢查）
- ✅ 移除購物車商品
- ✅ 清空購物車

**訂單系統**（✅ 完整實作）
- ✅ 結帳流程（從購物車建立訂單）
- ✅ 自動扣除庫存
- ✅ 訂單歷史查詢（僅能查看自己的訂單）
- ✅ 訂單詳細資訊檢視
- ✅ 訂單狀態追蹤
- ✅ 權限控制（無法查看他人訂單）

**後台管理系統**（✅ 完整實作）
- ✅ 後台儀表板（統計資料：商品數、訂單數、使用者數、總銷售額、待處理訂單數）
- ✅ 專輯管理（新增、編輯、刪除、列表）
- ✅ 雙分類管理（藝人分類 + 階層式商品類型 CRUD）
- ✅ 訂單管理（查看所有訂單、更新訂單狀態）
- ✅ 使用者管理（查看所有使用者、切換管理員角色、防止自我移除管理員權限）
- ✅ 管理員選單元件（View Component）

**使用者個人功能**
- ✅ 個人帳號總覽（訂單統計、最近訂單）
- ✅ 個人資料編輯（姓名、電話、生日、性別）
- ✅ 個人訂單歷史查詢
- ✅ 擴展使用者欄位（Birthday、Gender、PhoneNumber）

**資料庫初始化**
- ✅ DbInitializer 統一管理資料庫初始化
- ✅ 自動建立系統角色（User、Admin）
- ✅ 從 appsettings.json 讀取並建立預設管理員帳戶
- ✅ 建立預設藝人分類與商品類型（階層式）
- ✅ 建立範例商品資料

**效能與品質優化**
- ✅ 修正 OrderService N+1 查詢問題（使用 Dictionary 快取）
- ✅ 統一使用 DateTime.UtcNow 避免時區問題
- ✅ 修正 nullable 警告（Views/Album/Index.cshtml）
- ✅ 建置成功（0 個警告、0 個錯誤）

**UI/UX 改善**
- ✅ 購物車 Modal 彈窗
- ✅ View Components（購物車徽章、管理員選單）
- ✅ 帳號選單與後台選單
- ✅ 頁面專屬樣式與 JavaScript 模組化

**程式碼品質**
- ✅ 完整的繁體中文註解
- ✅ 三層式架構清晰分離
- ✅ 依賴注入設計
- ✅ 安全性驗證（權限控制、庫存檢查）

### v1.0.0 (2026-03-03)
初始版本發布，包含以下功能：

**核心功能**
- ✅ 使用者認證系統（ASP.NET Core Identity）
  - 使用者註冊（含表單驗證）
  - 使用者登入/登出
  - 密碼 PBKDF2 加密儲存
  - Cookie 認證機制
  - 導航欄位顯示登入狀態

- ✅ 專輯展示與瀏覽
  - 專輯列表頁面（響應式卡片設計）
  - 專輯詳細資訊頁面
  - 分類篩選功能
  - 關鍵字搜尋（支援標題、演出者）
  - 庫存數量顯示

- ✅ 首頁設計
  - 幻燈片輪播系統（3 個輪播頁面）
  - 最新商品展示區（顯示最新 2 個專輯）
  - 商品卡片包含封面、價格、庫存、加入購物車按鈕
  - 響應式設計（支援桌面與行動裝置）

- ✅ 頁面與版面配置
  - 統一的頁面配置（_Layout.cshtml）
  - 導航欄位（首頁、專輯、隱私權政策、登入/註冊）
  - 簡潔的 Footer（含快速連結）
  - 頁面底部間距優化（防止內容被覆蓋）
  - 完整的隱私權政策頁面

**資料庫**
- ✅ Entity Framework Core 整合
- ✅ 完整的資料模型設計（AppUser、Album、Category、Order、OrderItem、CartItem）
- ✅ 資料表關聯設定（一對多、多對一、串聯刪除、限制刪除）
- ✅ Code First Migrations

**安全性**
- ✅ 密碼 PBKDF2 雜湊加密
- ✅ CSRF 防護（ValidateAntiForgeryToken）
- ✅ SQL 注入防護（EF Core 參數化查詢）
- ✅ XSS 防護（Razor 自動編碼）
- ✅ 敏感資訊保護（appsettings.json 已加入 .gitignore）

**已知限制**
- ⏳ 購物車功能尚未實作
- ⏳ 訂單管理功能尚未實作
- ⏳ 後台管理功能尚未實作
- ⏳ 分頁功能尚未實作

## 授權

此專案為學習與教育用途。
