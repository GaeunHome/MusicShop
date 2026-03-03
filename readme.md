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

### ✅ 已完成功能

- **使用者認證系統**
  - ✅ 使用者註冊（含表單驗證）
  - ✅ 使用者登入/登出
  - ✅ ASP.NET Core Identity 整合
  - ✅ 密碼加密儲存（PBKDF2）
  - ✅ 存取權限控制
  - ✅ 導航欄位顯示登入狀態

- **專輯展示系統**
  - ✅ 專輯列表瀏覽（響應式卡片設計）
  - ✅ 專輯詳細資訊頁面
  - ✅ 分類篩選功能
  - ✅ 關鍵字搜尋（標題、演出者）
  - ✅ 專輯與分類關聯
  - ✅ 庫存顯示

- **首頁設計**
  - ✅ 幻燈片輪播（3 個輪播頁面）
  - ✅ 最新商品展示（顯示最新 2 個專輯）
  - ✅ 商品卡片（包含封面、價格、庫存資訊）
  - ✅ 加入購物車按鈕（連結到 CartController）
  - ✅ 響應式版面設計

- **頁面與導航**
  - ✅ 統一的頁面配置（_Layout.cshtml）
  - ✅ 簡潔的 Footer（含快速連結）
  - ✅ 頁面底部間距優化（防止內容被覆蓋）
  - ✅ 完整的隱私權政策頁面

- **資料模型**
  - ✅ 使用者模型（AppUser）
  - ✅ 專輯模型（Album）
  - ✅ 分類模型（Category）
  - ✅ 訂單模型（Order、OrderItem）
  - ✅ 購物車模型（CartItem）
  - ✅ 完整的關聯設定（一對多、多對一）

### 🚧 待開發功能

- **購物車系統**
  - ⏳ 加入購物車（CartController）
  - ⏳ 檢視購物車內容
  - ⏳ 修改商品數量
  - ⏳ 移除購物車商品
  - ⏳ 購物車商品數量即時顯示

- **訂單管理**
  - ⏳ 結帳流程（OrderController）
  - ⏳ 訂單建立與確認
  - ⏳ 訂單歷史查詢
  - ⏳ 訂單狀態追蹤（待處理/已付款/已出貨/已完成/已取消）
  - ⏳ 訂單詳細資訊檢視

- **後台管理**
  - ⏳ 專輯管理（新增、編輯、刪除）
  - ⏳ 分類管理
  - ⏳ 訂單管理
  - ⏳ 庫存管理

## 技術堆疊

- **框架**：ASP.NET Core MVC (.NET 10.0)
- **資料庫**：SQL Server
- **ORM**：Entity Framework Core 10.0.3
- **認證**：ASP.NET Core Identity
- **前端**：Razor Views、Bootstrap 5

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
├── Controllers/          # MVC 控制器
│   ├── AccountController.cs    # 帳戶管理（註冊、登入、登出）
│   ├── AlbumController.cs      # 專輯瀏覽與詳細頁面
│   ├── HomeController.cs       # 首頁（幻燈片、最新商品）
│   ├── CartController.cs       # 購物車（待實作）
│   └── OrderController.cs      # 訂單管理（待實作）
├── Models/              # 資料模型
│   ├── AppUser.cs       # 使用者模型（擴展 IdentityUser）
│   ├── Album.cs         # 專輯模型
│   ├── Category.cs      # 分類模型
│   ├── Order.cs         # 訂單模型
│   ├── OrderItem.cs     # 訂單項目
│   └── CartItem.cs      # 購物車項目
├── ViewMdoels/          # 檢視模型
│   ├── RegisterViewModel.cs
│   ├── LoginViewModel.cs
│   └── AlbumViewModel.cs
├── Views/               # Razor 檢視
│   ├── Home/            # 首頁檢視（Index、Privacy）
│   ├── Account/         # 帳戶檢視（Register、Login）
│   ├── Album/           # 專輯檢視（Index、Detail）
│   └── Shared/          # 共用檢視（_Layout、Footer）
├── Data/                # 資料庫上下文
│   └── ApplicationDbContext.cs
├── Migrations/          # EF Core 遷移檔案
├── wwwroot/            # 靜態檔案
│   ├── css/            # CSS 樣式
│   ├── js/             # JavaScript
│   └── lib/            # Bootstrap、jQuery
├── appsettings.example.json   # 設定檔範例
├── CLAUDE.md           # Claude Code 專案指引
└── readme.md           # 本文件
```

## 資料庫架構

### 主要資料表

- **Albums**：專輯資訊（標題、演出者、價格、庫存、封面圖片 URL 等）
- **Categories**：專輯分類（搖滾、流行、古典等）
- **Orders**：訂單主檔（訂單編號、使用者、日期、狀態、總金額）
- **OrderItems**：訂單明細（訂單 ID、專輯 ID、數量、單價）
- **CartItems**：購物車項目（使用者 ID、專輯 ID、數量、加入時間）
- **AspNetUsers**：使用者資料（Identity 系統自動建立）

### 關鍵關聯

- 專輯 ←→ 分類（多對一，限制刪除）
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
