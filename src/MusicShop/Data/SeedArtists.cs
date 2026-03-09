using Microsoft.EntityFrameworkCore;
using MusicShop.Models;

namespace MusicShop.Data;

/// <summary>
/// 手動插入藝人資料的工具類別
/// </summary>
public static class SeedArtists
{
    public static async Task SeedAsync(IDbContextFactory<ApplicationDbContext> contextFactory)
    {
        await using var context = await contextFactory.CreateDbContextAsync();

        // 檢查是否已有藝人資料
        var artistCount = await context.Artists.CountAsync();
        Console.WriteLine($"目前資料庫中有 {artistCount} 位藝人");

        if (artistCount > 0)
        {
            Console.WriteLine("資料庫中已有藝人資料，跳過插入。");
            return;
        }

        // 取得藝人分類
        var artistCategories = await context.ArtistCategories.ToListAsync();

        if (!artistCategories.Any())
        {
            Console.WriteLine("錯誤：找不到藝人分類資料，請先確保 ArtistCategories 已建立。");
            return;
        }

        var boyGroup = artistCategories.FirstOrDefault(c => c.Name == "BOY GROUP");
        var girlGroup = artistCategories.FirstOrDefault(c => c.Name == "GIRL GROUP");
        var solo = artistCategories.FirstOrDefault(c => c.Name == "SOLO");

        if (boyGroup == null || girlGroup == null || solo == null)
        {
            Console.WriteLine("錯誤：找不到必要的藝人分類（BOY GROUP、GIRL GROUP、SOLO）");
            return;
        }

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

        Console.WriteLine($"✓ 成功插入 {artists.Count} 位藝人資料");
        Console.WriteLine($"  - BOY GROUP: 26 位");
        Console.WriteLine($"  - GIRL GROUP: 16 位");
        Console.WriteLine($"  - SOLO: 6 位");
    }
}
