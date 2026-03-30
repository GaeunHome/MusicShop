using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using MusicShop.Data.Entities;

namespace MusicShop.Data;

/// <summary>
/// 資料庫初始化器 - 負責建立預設角色和管理員帳戶
/// </summary>
public static class DbInitializer
{
    /// <summary>
    /// 初始化角色、管理員帳戶和預設分類資料
    /// </summary>
    /// <remarks>
    /// 角色和管理員帳戶由 Identity 管理（自帶交易），不在 DbContext 交易範圍內。
    /// 分類和藝人的種子資料使用同一交易包裝，確保原子性：若任一步驟失敗，全部回滾。
    /// </remarks>
    public static async Task InitializeAsync(
        RoleManager<IdentityRole> roleManager,
        UserManager<AppUser> userManager,
        IConfiguration configuration,
        ApplicationDbContext context)
    {
        // Identity 操作（角色、管理員）自帶交易管理，不需要額外包裝
        await CreateRolesAsync(roleManager);
        await CreateDefaultAdminAsync(userManager, configuration);
        await CreateDefaultSuperAdminAsync(userManager, configuration);

        // 種子資料使用交易確保原子性
        await using var transaction = await context.Database.BeginTransactionAsync();
        try
        {
            await CreateDefaultCategoriesAsync(context);
            await CreateDefaultArtistsAsync(context);
            await CreateDefaultSystemSettingsAsync(context);
            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    /// <summary>
    /// 建立系統角色：User 和 Admin
    /// </summary>
    private static async Task CreateRolesAsync(RoleManager<IdentityRole> roleManager)
    {
        string[] roleNames = { "User", "Admin", "SuperAdmin" };

        foreach (var roleName in roleNames)
        {
            // 檢查角色是否已存在
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                var result = await roleManager.CreateAsync(new IdentityRole(roleName));

                if (result.Succeeded)
                {
                    Console.WriteLine($"角色 '{roleName}' 建立成功");
                }
                else
                {
                    Console.WriteLine($"角色 '{roleName}' 建立失敗：{string.Join(", ", result.Errors.Select(e => e.Description))}");
                }
            }
        }
    }

    /// <summary>
    /// 建立預設管理員帳戶（從 appsettings.json 讀取）
    /// </summary>
    private static async Task CreateDefaultAdminAsync(
        UserManager<AppUser> userManager,
        IConfiguration configuration)
    {
        // 從設定檔讀取管理員資訊
        var adminEmail = configuration["AdminSettings:Email"];
        var adminPassword = configuration["AdminSettings:Password"];
        var adminFullName = configuration["AdminSettings:FullName"] ?? "系統管理員";

        // 如果設定檔中沒有管理員資訊，就不建立
        if (string.IsNullOrEmpty(adminEmail) || string.IsNullOrEmpty(adminPassword))
        {
            Console.WriteLine("警告：未在設定檔中找到管理員資訊，跳過建立預設管理員");
            return;
        }

        // 檢查管理員帳戶是否已存在
        var adminUser = await userManager.FindByEmailAsync(adminEmail);

        if (adminUser == null)
        {
            // 建立管理員帳戶
            adminUser = new AppUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                FullName = adminFullName,
                // 預設管理員跳過 Email 驗證流程：系統初始化時尚無郵件服務，
                // 且管理員帳號由設定檔直接提供，可信度等同於環境設定。
                EmailConfirmed = true
            };

            var result = await userManager.CreateAsync(adminUser, adminPassword);

            if (result.Succeeded)
            {
                Console.WriteLine($"管理員帳戶 '{adminEmail}' 建立成功");

                // 指派 Admin 角色
                await userManager.AddToRoleAsync(adminUser, "Admin");
                Console.WriteLine($"已將 Admin 角色指派給 '{adminEmail}'");
            }
            else
            {
                Console.WriteLine($"管理員帳戶建立失敗：{string.Join(", ", result.Errors.Select(e => e.Description))}");
            }
        }
        else
        {
            // 確保現有管理員帳戶擁有 Admin 角色
            if (!await userManager.IsInRoleAsync(adminUser, "Admin"))
            {
                await userManager.AddToRoleAsync(adminUser, "Admin");
                Console.WriteLine($"已將 Admin 角色指派給現有帳戶 '{adminEmail}'");
            }
        }
    }

    /// <summary>
    /// 建立預設超級管理員帳戶（從 appsettings.json 讀取）
    /// SuperAdmin 同時擁有 Admin + SuperAdmin 兩個角色，可存取所有後台功能
    /// </summary>
    private static async Task CreateDefaultSuperAdminAsync(
        UserManager<AppUser> userManager,
        IConfiguration configuration)
    {
        var superAdminEmail = configuration["SuperAdminSettings:Email"];
        var superAdminPassword = configuration["SuperAdminSettings:Password"];
        var superAdminFullName = configuration["SuperAdminSettings:FullName"] ?? "超級管理員";

        if (string.IsNullOrEmpty(superAdminEmail) || string.IsNullOrEmpty(superAdminPassword))
        {
            Console.WriteLine("警告：未在設定檔中找到超級管理員資訊，跳過建立預設超級管理員");
            return;
        }

        var superAdminUser = await userManager.FindByEmailAsync(superAdminEmail);

        if (superAdminUser == null)
        {
            superAdminUser = new AppUser
            {
                UserName = superAdminEmail,
                Email = superAdminEmail,
                FullName = superAdminFullName,
                EmailConfirmed = true
            };

            var result = await userManager.CreateAsync(superAdminUser, superAdminPassword);

            if (result.Succeeded)
            {
                Console.WriteLine($"超級管理員帳戶 '{superAdminEmail}' 建立成功");

                // 同時指派 Admin 和 SuperAdmin 角色
                await userManager.AddToRoleAsync(superAdminUser, "Admin");
                await userManager.AddToRoleAsync(superAdminUser, "SuperAdmin");
                Console.WriteLine($"已將 Admin + SuperAdmin 角色指派給 '{superAdminEmail}'");
            }
            else
            {
                Console.WriteLine($"超級管理員帳戶建立失敗：{string.Join(", ", result.Errors.Select(e => e.Description))}");
            }
        }
        else
        {
            // 確保現有帳戶擁有 Admin 和 SuperAdmin 角色
            if (!await userManager.IsInRoleAsync(superAdminUser, "Admin"))
            {
                await userManager.AddToRoleAsync(superAdminUser, "Admin");
            }
            if (!await userManager.IsInRoleAsync(superAdminUser, "SuperAdmin"))
            {
                await userManager.AddToRoleAsync(superAdminUser, "SuperAdmin");
                Console.WriteLine($"已將 SuperAdmin 角色指派給現有帳戶 '{superAdminEmail}'");
            }
        }
    }

    /// <summary>
    /// 建立預設系統參數
    /// </summary>
    private static async Task CreateDefaultSystemSettingsAsync(ApplicationDbContext context)
    {
        // 檢查資料表是否已存在（Migration 可能尚未套用）
        var tableExists = await context.Database
            .SqlQueryRaw<int>("SELECT CASE WHEN EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'SystemSettings') THEN 1 ELSE 0 END AS [Value]")
            .OrderBy(x => x)
            .FirstOrDefaultAsync();

        if (tableExists == 0)
        {
            Console.WriteLine("SystemSettings 資料表尚未建立，跳過系統參數種子資料（請執行 dotnet ef database update）");
            return;
        }

        // 預設系統參數清單（每次啟動會自動補缺，不會覆蓋已存在的值）
        var defaultSettings = new List<SystemSetting>
        {
            // ===== 網站設定（對應原 appsettings.json SiteSettings）=====
            new SystemSetting { Key = "site.name", Value = "MusicShop", Description = "網站名稱", Group = "網站設定", ValueType = "string" },
            new SystemSetting { Key = "site.customer_service_phone", Value = "02-2732-9768", Description = "客服電話", Group = "網站設定", ValueType = "string" },
            new SystemSetting { Key = "site.customer_service_hours", Value = "09:00-19:00", Description = "客服時間", Group = "網站設定", ValueType = "string" },
            new SystemSetting { Key = "site.customer_service_email", Value = "help@musicshop.com", Description = "客服信箱", Group = "網站設定", ValueType = "string" },
            new SystemSetting { Key = "site.company_tax_id", Value = "12345678", Description = "公司統一編號", Group = "網站設定", ValueType = "string" },
            new SystemSetting { Key = "site.membership_id", Value = "MS202601", Description = "會員編號（頂部資訊列顯示）", Group = "網站設定", ValueType = "string" },

            new SystemSetting { Key = "site.maintenance_mode", Value = "false", Description = "是否進入維護模式", Group = "網站設定", ValueType = "bool" },
            new SystemSetting { Key = "site.maintenance_message", Value = "網站維護中，請稍後再試", Description = "維護模式顯示訊息", Group = "網站設定", ValueType = "string" },
            new SystemSetting { Key = "site.announcement", Value = "", Description = "全站公告（頂部橫幅，空值不顯示）", Group = "網站設定", ValueType = "string" },

            // ===== 社群媒體（對應原 appsettings.json SiteSettings.SocialMedia）=====
            new SystemSetting { Key = "site.social.facebook", Value = "https://facebook.com", Description = "Facebook 粉絲專頁連結", Group = "社群媒體", ValueType = "string" },
            new SystemSetting { Key = "site.social.instagram", Value = "https://instagram.com", Description = "Instagram 帳號連結", Group = "社群媒體", ValueType = "string" },
            new SystemSetting { Key = "site.social.line", Value = "https://line.me", Description = "LINE 官方帳號連結", Group = "社群媒體", ValueType = "string" },
            new SystemSetting { Key = "site.social.youtube", Value = "", Description = "YouTube 頻道連結", Group = "社群媒體", ValueType = "string" },
            new SystemSetting { Key = "site.social.twitter", Value = "", Description = "X (Twitter) 連結", Group = "社群媒體", ValueType = "string" },

            // ===== 頁尾連結（對應原 appsettings.json SiteSettings.FooterLinks）=====
            new SystemSetting { Key = "site.footer.about", Value = "#", Description = "關於我們頁面連結", Group = "頁尾連結", ValueType = "string" },
            new SystemSetting { Key = "site.footer.refund_policy", Value = "#", Description = "退款政策頁面連結", Group = "頁尾連結", ValueType = "string" },
            new SystemSetting { Key = "site.footer.terms", Value = "#", Description = "服務條款頁面連結", Group = "頁尾連結", ValueType = "string" },
            new SystemSetting { Key = "site.footer.privacy", Value = "#", Description = "隱私權政策頁面連結", Group = "頁尾連結", ValueType = "string" },
            new SystemSetting { Key = "site.footer.faq", Value = "#", Description = "常見問題頁面連結", Group = "頁尾連結", ValueType = "string" },

            // ===== SMTP 郵件設定（對應原 appsettings.json SmtpSettings，不含密碼）=====
            new SystemSetting { Key = "smtp.from_name", Value = "MusicShop", Description = "寄件者名稱", Group = "郵件設定", ValueType = "string" },

            // ===== 訂單設定 =====
            new SystemSetting { Key = "order.shipping_fee", Value = "60", Description = "訂單運費（元）", Group = "訂單設定", ValueType = "decimal" },
            new SystemSetting { Key = "order.free_shipping_threshold", Value = "1000", Description = "免運門檻金額（元）", Group = "訂單設定", ValueType = "decimal" },
            new SystemSetting { Key = "order.auto_cancel_hours", Value = "24", Description = "未付款訂單自動取消時數", Group = "訂單設定", ValueType = "int" },
            new SystemSetting { Key = "order.credit_card_timeout_minutes", Value = "15", Description = "信用卡訂單未付款自動取消時間（分鐘）", Group = "訂單設定", ValueType = "int" },
            new SystemSetting { Key = "order.high_pending_threshold", Value = "10", Description = "待處理訂單數量警示門檻", Group = "訂單設定", ValueType = "int" },
            new SystemSetting { Key = "order.min_order_amount", Value = "0", Description = "最低訂購金額（元）", Group = "訂單設定", ValueType = "decimal" },

            // ===== 優惠券設定 =====
            new SystemSetting { Key = "coupon.max_per_order", Value = "1", Description = "每筆訂單最多使用幾張優惠券", Group = "優惠券設定", ValueType = "int" },
            new SystemSetting { Key = "coupon.new_user_enabled", Value = "true", Description = "是否啟用新會員優惠券", Group = "優惠券設定", ValueType = "bool" },
            new SystemSetting { Key = "coupon.new_user_discount", Value = "50", Description = "新會員優惠券折抵金額", Group = "優惠券設定", ValueType = "decimal" },

            // ===== 顯示設定（對應 DisplayConstants 中可調整的數值）=====
            new SystemSetting { Key = "display.albums_per_page", Value = "12", Description = "前台商品列表每頁顯示筆數", Group = "顯示設定", ValueType = "int" },
            new SystemSetting { Key = "display.related_albums_count", Value = "8", Description = "商品詳情頁相關商品顯示數量", Group = "顯示設定", ValueType = "int" },
            new SystemSetting { Key = "display.featured_artist_albums_count", Value = "4", Description = "精選藝人區塊每位藝人顯示的專輯數", Group = "顯示設定", ValueType = "int" },
            new SystemSetting { Key = "display.recent_orders_count", Value = "5", Description = "最近訂單預設顯示筆數", Group = "顯示設定", ValueType = "int" },
            new SystemSetting { Key = "display.admin_artist_page_size", Value = "10", Description = "後台藝人列表每頁顯示筆數", Group = "顯示設定", ValueType = "int" },
            new SystemSetting { Key = "display.admin_order_page_size", Value = "20", Description = "後台訂單列表每頁顯示筆數", Group = "顯示設定", ValueType = "int" },
            new SystemSetting { Key = "display.new_arrivals_count", Value = "8", Description = "首頁新品上架顯示數量", Group = "顯示設定", ValueType = "int" },
            new SystemSetting { Key = "display.home_banner_interval", Value = "5000", Description = "首頁 Banner 輪播間隔（毫秒）", Group = "顯示設定", ValueType = "int" },

            // ===== SEO 設定 =====
            new SystemSetting { Key = "seo.meta_description", Value = "線上音樂專輯商店", Description = "首頁 meta description", Group = "SEO 設定", ValueType = "string" },
            new SystemSetting { Key = "seo.meta_keywords", Value = "音樂,專輯,唱片,CD", Description = "首頁 meta keywords", Group = "SEO 設定", ValueType = "string" },
            new SystemSetting { Key = "seo.og_image", Value = "", Description = "Open Graph 預設圖片 URL", Group = "SEO 設定", ValueType = "string" },

            // ===== 安全設定 =====
            new SystemSetting { Key = "security.max_failed_attempts", Value = "3", Description = "登入失敗鎖定次數", Group = "安全設定", ValueType = "int" },
            new SystemSetting { Key = "security.lockout_minutes", Value = "5", Description = "帳號鎖定時間（分鐘）", Group = "安全設定", ValueType = "int" },
            new SystemSetting { Key = "security.password_min_length", Value = "6", Description = "密碼最小長度", Group = "安全設定", ValueType = "int" },

            // ===== 金流設定（僅測試模式旗標，不含金鑰）=====
            new SystemSetting { Key = "ecpay.is_test", Value = "true", Description = "ECPay 是否為測試模式", Group = "金流設定", ValueType = "bool" }
        };

        // 補缺模式：只新增資料庫中尚未存在的 Key，不覆蓋已修改的值
        var existingKeys = context.SystemSettings
            .Select(s => s.Key)
            .ToHashSet();

        var missingSettings = defaultSettings
            .Where(s => !existingKeys.Contains(s.Key))
            .ToList();

        if (missingSettings.Any())
        {
            context.SystemSettings.AddRange(missingSettings);
            await context.SaveChangesAsync();
            Console.WriteLine($"已補建 {missingSettings.Count} 組系統參數：{string.Join(", ", missingSettings.Select(s => s.Key))}");
        }
    }

    /// <summary>
    /// 建立預設的藝人分類和商品類型
    /// </summary>
    private static async Task CreateDefaultCategoriesAsync(ApplicationDbContext context)
    {
        // 建立預設藝人分類
        if (!context.ArtistCategories.Any())
        {
            var artistCategories = new List<ArtistCategory>
            {
                new ArtistCategory { Name = "BOY GROUP", Description = "男子偶像團體", DisplayOrder = 1 },
                new ArtistCategory { Name = "GIRL GROUP", Description = "女子偶像團體", DisplayOrder = 2 },
                new ArtistCategory { Name = "SOLO", Description = "個人歌手", DisplayOrder = 3 }
            };

            context.ArtistCategories.AddRange(artistCategories);
            await context.SaveChangesAsync();
            Console.WriteLine("已建立預設藝人分類");
        }

        // 建立預設商品類型（階層式：父分類 + 子分類）
        if (!context.ProductTypes.Any())
        {
            // 第一步：建立父分類
            var kAlbum = new ProductType
            {
                Name = "K-ALBUM",
                Description = "韓國音樂專輯相關商品",
                DisplayOrder = 1,
                ParentId = null
            };
            var kMagazine = new ProductType
            {
                Name = "K-MAGAZINE",
                Description = "韓國雜誌與刊物",
                DisplayOrder = 2,
                ParentId = null
            };
            var kMerch = new ProductType
            {
                Name = "K-MERCH",
                Description = "韓國周邊商品",
                DisplayOrder = 3,
                ParentId = null
            };
            var kEvent = new ProductType
            {
                Name = "K-EVENT",
                Description = "演唱會與活動",
                DisplayOrder = 4,
                ParentId = null
            };

            // 先儲存父分類以取得 ID
            context.ProductTypes.AddRange(new[] { kAlbum, kMagazine, kMerch, kEvent });
            await context.SaveChangesAsync();
            Console.WriteLine("已建立 4 個父分類");

            // 第二步：建立子分類
            var childCategories = new List<ProductType>
            {
                // K-ALBUM 的子分類
                new ProductType { Name = "ALBUM", Description = "正規專輯、迷你專輯", DisplayOrder = 1, ParentId = kAlbum.Id },
                new ProductType { Name = "PHOTOBOOK", Description = "寫真書、影像集", DisplayOrder = 2, ParentId = kAlbum.Id },
                new ProductType { Name = "DVD", Description = "演唱會影像", DisplayOrder = 3, ParentId = kAlbum.Id },

                // K-MAGAZINE 的子分類
                new ProductType { Name = "寫真雜誌", Description = "明星寫真雜誌", DisplayOrder = 1, ParentId = kMagazine.Id },
                new ProductType { Name = "時尚雜誌", Description = "流行時尚雜誌", DisplayOrder = 2, ParentId = kMagazine.Id },

                // K-MERCH 的子分類
                new ProductType { Name = "官方周邊", Description = "官方授權周邊", DisplayOrder = 1, ParentId = kMerch.Id },
                new ProductType { Name = "應援手燈", Description = "演唱會應援手燈", DisplayOrder = 2, ParentId = kMerch.Id },
                new ProductType { Name = "服飾配件", Description = "T恤、帽子等", DisplayOrder = 3, ParentId = kMerch.Id },

                // K-EVENT 的子分類
                new ProductType { Name = "CURRENT EVENT", Description = "進行中的活動", DisplayOrder = 1, ParentId = kEvent.Id },
                new ProductType { Name = "演唱會周邊", Description = "演唱會官方周邊", DisplayOrder = 2, ParentId = kEvent.Id }
            };

            context.ProductTypes.AddRange(childCategories);
            await context.SaveChangesAsync();
            Console.WriteLine($"已建立 {childCategories.Count} 個子分類");
        }

        // 注意：範例商品建立已移至資料庫遷移後，使用新的 Artist 外鍵關聯
        // 如需建立範例商品，請在執行資料庫遷移後，使用後台管理介面手動建立
    }

    /// <summary>
    /// 建立預設的藝人/團體資料（三層架構：K-ARTIST → BOY GROUP/GIRL GROUP/SOLO → 具體團體）
    /// </summary>
    private static async Task CreateDefaultArtistsAsync(ApplicationDbContext context)
    {
        if (!context.Artists.Any())
        {
            var artistCategories = context.ArtistCategories.ToList();
            var boyGroup = artistCategories.FirstOrDefault(c => c.Name == "BOY GROUP")
                ?? throw new InvalidOperationException("找不到藝人分類 'BOY GROUP'，請先建立預設分類");
            var girlGroup = artistCategories.FirstOrDefault(c => c.Name == "GIRL GROUP")
                ?? throw new InvalidOperationException("找不到藝人分類 'GIRL GROUP'，請先建立預設分類");
            var solo = artistCategories.FirstOrDefault(c => c.Name == "SOLO")
                ?? throw new InvalidOperationException("找不到藝人分類 'SOLO'，請先建立預設分類");

            var artists = new List<Artist>
            {
                // BOY GROUP
                new Artist { Name = "2PM", ArtistCategoryId = boyGroup.Id, DisplayOrder = 1 },
                new Artist { Name = "ASTRO", ArtistCategoryId = boyGroup.Id, DisplayOrder = 2 },
                new Artist { Name = "ATEEZ", ArtistCategoryId = boyGroup.Id, DisplayOrder = 3 },
                new Artist { Name = "BIGBANG", ArtistCategoryId = boyGroup.Id, DisplayOrder = 4 },
                new Artist { Name = "BTOB", ArtistCategoryId = boyGroup.Id, DisplayOrder = 5 },
                new Artist { Name = "BTS", ArtistCategoryId = boyGroup.Id, DisplayOrder = 6 },
                new Artist { Name = "CRAVITY", ArtistCategoryId = boyGroup.Id, DisplayOrder = 7 },
                new Artist { Name = "DAY6", ArtistCategoryId = boyGroup.Id, DisplayOrder = 8 },
                new Artist { Name = "DKZ", ArtistCategoryId = boyGroup.Id, DisplayOrder = 9 },
                new Artist { Name = "ENHYPEN", ArtistCategoryId = boyGroup.Id, DisplayOrder = 10 },
                new Artist { Name = "EXO", ArtistCategoryId = boyGroup.Id, DisplayOrder = 11 },
                new Artist { Name = "GOT7", ArtistCategoryId = boyGroup.Id, DisplayOrder = 12 },
                new Artist { Name = "MONSTA X", ArtistCategoryId = boyGroup.Id, DisplayOrder = 13 },
                new Artist { Name = "NCT", ArtistCategoryId = boyGroup.Id, DisplayOrder = 14 },
                new Artist { Name = "NU'EST", ArtistCategoryId = boyGroup.Id, DisplayOrder = 15 },
                new Artist { Name = "ONF", ArtistCategoryId = boyGroup.Id, DisplayOrder = 16 },
                new Artist { Name = "ONEUS", ArtistCategoryId = boyGroup.Id, DisplayOrder = 17 },
                new Artist { Name = "PENTAGON", ArtistCategoryId = boyGroup.Id, DisplayOrder = 18 },
                new Artist { Name = "SEVENTEEN", ArtistCategoryId = boyGroup.Id, DisplayOrder = 19 },
                new Artist { Name = "SHINee", ArtistCategoryId = boyGroup.Id, DisplayOrder = 20 },
                new Artist { Name = "Stray Kids", ArtistCategoryId = boyGroup.Id, DisplayOrder = 21 },
                new Artist { Name = "SUPER JUNIOR", ArtistCategoryId = boyGroup.Id, DisplayOrder = 22 },
                new Artist { Name = "TREASURE", ArtistCategoryId = boyGroup.Id, DisplayOrder = 23 },
                new Artist { Name = "TXT", ArtistCategoryId = boyGroup.Id, DisplayOrder = 24 },
                new Artist { Name = "VICTON", ArtistCategoryId = boyGroup.Id, DisplayOrder = 25 },
                new Artist { Name = "WINNER", ArtistCategoryId = boyGroup.Id, DisplayOrder = 26 },

                // GIRL GROUP
                new Artist { Name = "BLACKPINK", ArtistCategoryId = girlGroup.Id, DisplayOrder = 1 },
                new Artist { Name = "TWICE", ArtistCategoryId = girlGroup.Id, DisplayOrder = 2 },
                new Artist { Name = "RED VELVET", ArtistCategoryId = girlGroup.Id, DisplayOrder = 3 },
                new Artist { Name = "MAMAMOO", ArtistCategoryId = girlGroup.Id, DisplayOrder = 4 },
                new Artist { Name = "ITZY", ArtistCategoryId = girlGroup.Id, DisplayOrder = 5 },
                new Artist { Name = "aespa", ArtistCategoryId = girlGroup.Id, DisplayOrder = 6 },
                new Artist { Name = "(G)I-DLE", ArtistCategoryId = girlGroup.Id, DisplayOrder = 7 },
                new Artist { Name = "fromis_9", ArtistCategoryId = girlGroup.Id, DisplayOrder = 8 },
                new Artist { Name = "VIVIZ", ArtistCategoryId = girlGroup.Id, DisplayOrder = 9 },
                new Artist { Name = "IVE", ArtistCategoryId = girlGroup.Id, DisplayOrder = 10 },
                new Artist { Name = "LE SSERAFIM", ArtistCategoryId = girlGroup.Id, DisplayOrder = 11 },
                new Artist { Name = "NewJeans", ArtistCategoryId = girlGroup.Id, DisplayOrder = 12 },
                new Artist { Name = "NMIXX", ArtistCategoryId = girlGroup.Id, DisplayOrder = 13 },
                new Artist { Name = "Oh My Girl", ArtistCategoryId = girlGroup.Id, DisplayOrder = 14 },
                new Artist { Name = "STAYC", ArtistCategoryId = girlGroup.Id, DisplayOrder = 15 },
                new Artist { Name = "Kep1er", ArtistCategoryId = girlGroup.Id, DisplayOrder = 16 },

                // SOLO
                new Artist { Name = "IU", ArtistCategoryId = solo.Id, DisplayOrder = 1 },
                new Artist { Name = "Solar", ArtistCategoryId = solo.Id, DisplayOrder = 2 },
                new Artist { Name = "Taeyeon", ArtistCategoryId = solo.Id, DisplayOrder = 3 },
                new Artist { Name = "Hwasa", ArtistCategoryId = solo.Id, DisplayOrder = 4 },
                new Artist { Name = "Sunmi", ArtistCategoryId = solo.Id, DisplayOrder = 5 },
                new Artist { Name = "Chungha", ArtistCategoryId = solo.Id, DisplayOrder = 6 }
            };

            context.Artists.AddRange(artists);
            await context.SaveChangesAsync();
            Console.WriteLine($"已建立 {artists.Count} 位藝人資料");
        }
    }
}
