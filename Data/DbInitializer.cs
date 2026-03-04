using Microsoft.AspNetCore.Identity;
using MusicShop.Models;

namespace MusicShop.Data;

/// <summary>
/// 資料庫初始化器 - 負責建立預設角色和管理員帳戶
/// </summary>
public static class DbInitializer
{
    /// <summary>
    /// 初始化角色、管理員帳戶和預設分類資料
    /// </summary>
    public static async Task InitializeAsync(
        RoleManager<IdentityRole> roleManager,
        UserManager<AppUser> userManager,
        IConfiguration configuration,
        ApplicationDbContext context)
    {
        // 建立角色
        await CreateRolesAsync(roleManager);

        // 建立預設管理員（從設定檔讀取）
        await CreateDefaultAdminAsync(userManager, configuration);

        // 建立預設的藝人分類和商品類型
        await CreateDefaultCategoriesAsync(context);
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
                EmailConfirmed = true // 預設管理員帳戶已驗證
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

        // 建立範例商品
        if (!context.Albums.Any())
        {
            // 取得分類資料
            var boyGroup = context.ArtistCategories.First(c => c.Name == "BOY GROUP");
            var solo = context.ArtistCategories.First(c => c.Name == "SOLO");
            var albumType = context.ProductTypes.First(c => c.Name == "ALBUM");
            var currentEventType = context.ProductTypes.First(c => c.Name == "CURRENT EVENT");

            var sampleAlbums = new List<Album>
            {
                new Album
                {
                    Title = "SPIN OFF (5TH MINI ALBUM) 迷你五輯",
                    Artist = "ONF",
                    ArtistCategoryId = boyGroup.Id,
                    ProductTypeId = albumType.Id,
                    Description = @"商品內容：
外盒包裝：145mm x 123mm x 5mm
寫真書：104頁 (142mm x 123mm x 10mm)
CD：1張
迷你海報：1張 (折疊式)
小卡：隨機1張 (7種)
貼紙：1張",
                    Price = 690,
                    Stock = 10,
                    CoverImageUrl = "/images/albums/album2.jpg",
                    CreatedAt = DateTime.UtcNow
                },
                new Album
                {
                    Title = "[應募] 260315 Solar [Your Own Star] 專輯發行紀念簽名會 in TAIPEI",
                    Artist = "Solar",
                    ArtistCategoryId = solo.Id,
                    ProductTypeId = currentEventType.Id,
                    Description = @"‼️ 下單後請務必在應募期間內填寫Google應募表單 ‼️
⭐如有任何購買商品相關問題，請於應募期間來信至客服網系統或客服信箱

活動日期：2026年3月15日
活動地點：台北
活動內容：Solar專輯發行紀念簽名會",
                    Price = 630,
                    Stock = 1000,
                    CoverImageUrl = "/images/albums/album1.jpg",
                    CreatedAt = DateTime.UtcNow
                }
            };

            context.Albums.AddRange(sampleAlbums);
            await context.SaveChangesAsync();
            Console.WriteLine("已建立 2 件範例商品");
        }
    }
}
