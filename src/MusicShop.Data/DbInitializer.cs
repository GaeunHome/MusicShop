using Microsoft.AspNetCore.Identity;
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

        // 種子資料使用交易確保原子性
        await using var transaction = await context.Database.BeginTransactionAsync();
        try
        {
            await CreateDefaultCategoriesAsync(context);
            await CreateDefaultArtistsAsync(context);
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
        string[] roleNames = { "User", "Admin" };

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
