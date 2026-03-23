using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using MusicShop.Data.Entities;

namespace MusicShop.Data;

public class ApplicationDbContext : IdentityDbContext<AppUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }
    public DbSet<Album> Albums { get; set; }
    public DbSet<Artist> Artists { get; set; }
    public DbSet<ArtistCategory> ArtistCategories { get; set; }
    public DbSet<ProductType> ProductTypes { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderItem> OrderItems { get; set; }
    public DbSet<CartItem> CartItems { get; set; }
    public DbSet<Banner> Banners { get; set; }
    public DbSet<WishlistItem> WishlistItems { get; set; }
    public DbSet<FeaturedArtist> FeaturedArtists { get; set; }
    public DbSet<Coupon> Coupons { get; set; }
    public DbSet<UserCoupon> UserCoupons { get; set; }
    public DbSet<PasswordHistory> PasswordHistories { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // ===== 軟刪除 Global Query Filter =====
        // 所有實作 ISoftDeletable 的實體，查詢時自動排除已軟刪除的資料。
        // 需要查詢已刪除資料時，使用 .IgnoreQueryFilters() 明確略過。
        builder.Entity<Album>().HasQueryFilter(e => !e.IsDeleted);
        builder.Entity<Artist>().HasQueryFilter(e => !e.IsDeleted);
        builder.Entity<ArtistCategory>().HasQueryFilter(e => !e.IsDeleted);
        builder.Entity<ProductType>().HasQueryFilter(e => !e.IsDeleted);
        builder.Entity<Order>().HasQueryFilter(e => !e.IsDeleted);
        builder.Entity<Banner>().HasQueryFilter(e => !e.IsDeleted);
        builder.Entity<FeaturedArtist>().HasQueryFilter(e => !e.IsDeleted);
        builder.Entity<Coupon>().HasQueryFilter(e => !e.IsDeleted);

        // 以下分類關聯皆使用 Restrict：刪除分類/藝人前必須先移除或重新指派所屬商品，
        // 避免意外串聯刪除大量專輯資料，由 Service 層在刪除前檢查並給予使用者明確提示。

        // Album 與 ProductType 關聯
        builder.Entity<Album>()
            .HasOne(a => a.ProductType)
            .WithMany(pt => pt.Albums)
            .HasForeignKey(a => a.ProductTypeId)
            .OnDelete(DeleteBehavior.Restrict);

        // Artist 與 ArtistCategory 關聯（一對多）
        builder.Entity<Artist>()
            .HasOne(a => a.ArtistCategory)
            .WithMany(ac => ac.Artists)
            .HasForeignKey(a => a.ArtistCategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        // Album 與 Artist 關聯（一對多）
        builder.Entity<Album>()
            .HasOne(a => a.Artist)
            .WithMany(ar => ar.Albums)
            .HasForeignKey(a => a.ArtistId)
            .OnDelete(DeleteBehavior.Restrict);

        // ProductType 階層式自我參照關聯
        builder.Entity<ProductType>()
            .HasOne(pt => pt.Parent)
            .WithMany(pt => pt.Children)
            .HasForeignKey(pt => pt.ParentId)
            .OnDelete(DeleteBehavior.Restrict);

        // Order 與 AppUser 關聯
        // 使用 Cascade：帳號刪除時一併清除其所有訂單，
        // 因為訂單與使用者強綁定，帳號刪除後訂單已無業務意義。
        builder.Entity<Order>()
            .HasOne(o => o.User)
            .WithMany(u => u.Orders)
            .HasForeignKey(o => o.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // OrderItem 與 Order 關聯
        builder.Entity<OrderItem>()
            .HasOne(oi => oi.Order)
            .WithMany(o => o.OrderItems)
            .HasForeignKey(oi => oi.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        // OrderItem 與 Album 關聯
        builder.Entity<OrderItem>()
            .HasOne(oi => oi.Album)
            .WithMany(a => a.OrderItems)
            .HasForeignKey(oi => oi.AlbumId)
            .OnDelete(DeleteBehavior.Restrict);

        // CartItem 與 AppUser 關聯
        builder.Entity<CartItem>()
            .HasOne(c => c.User)
            .WithMany(u => u.CartItems)
            .HasForeignKey(c => c.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // CartItem 與 Album 關聯
        builder.Entity<CartItem>()
            .HasOne(c => c.Album)
            .WithMany()
            .HasForeignKey(c => c.AlbumId)
            .OnDelete(DeleteBehavior.Restrict);

        // WishlistItem 與 AppUser 關聯
        builder.Entity<WishlistItem>()
            .HasOne(w => w.User)
            .WithMany(u => u.WishlistItems)
            .HasForeignKey(w => w.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // WishlistItem 與 Album 關聯
        // 使用 Cascade：收藏只是輕量級的「我喜歡」標記，商品下架後收藏已無意義，
        // 與 CartItem 的 Restrict 不同——購物車需要阻止刪除以保護使用者的購買意圖。
        builder.Entity<WishlistItem>()
            .HasOne(w => w.Album)
            .WithMany()
            .HasForeignKey(w => w.AlbumId)
            .OnDelete(DeleteBehavior.Cascade);

        // 每個使用者每件商品只能收藏一次
        builder.Entity<WishlistItem>()
            .HasIndex(w => new { w.UserId, w.AlbumId })
            .IsUnique();

        // Banner 與 Album 關聯
        // 使用 SetNull：幻燈片是獨立的行銷素材（含已上傳的圖片），
        // 商品下架後幻燈片仍可作為純展示用途保留，僅清除商品連結。
        builder.Entity<Banner>()
            .HasOne(b => b.Album)
            .WithMany()
            .HasForeignKey(b => b.AlbumId)
            .OnDelete(DeleteBehavior.SetNull);

        // FeaturedArtist 與 Artist 關聯
        // 使用 Cascade：精選藝人僅是「標記」，藝人刪除後精選也無意義
        builder.Entity<FeaturedArtist>()
            .HasOne(fa => fa.Artist)
            .WithMany()
            .HasForeignKey(fa => fa.ArtistId)
            .OnDelete(DeleteBehavior.Cascade);

        // Coupon Code 唯一索引（過濾已軟刪除的紀錄）
        builder.Entity<Coupon>()
            .HasIndex(c => c.Code)
            .IsUnique()
            .HasFilter("IsDeleted = 0");

        // UserCoupon 與 AppUser 關聯
        builder.Entity<UserCoupon>()
            .HasOne(uc => uc.User)
            .WithMany(u => u.UserCoupons)
            .HasForeignKey(uc => uc.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // UserCoupon 與 Coupon 關聯
        builder.Entity<UserCoupon>()
            .HasOne(uc => uc.Coupon)
            .WithMany(c => c.UserCoupons)
            .HasForeignKey(uc => uc.CouponId)
            .OnDelete(DeleteBehavior.Restrict);

        // UserCoupon 與 Order 關聯（可為 null）
        // 使用 NoAction 避免 SQL Server 循環串聯路徑（UserCoupon ↔ Order 雙向參照）
        builder.Entity<UserCoupon>()
            .HasOne(uc => uc.Order)
            .WithMany()
            .HasForeignKey(uc => uc.OrderId)
            .OnDelete(DeleteBehavior.NoAction);

        // Order 與 UserCoupon 關聯（訂單可選擇性使用優惠券）
        builder.Entity<Order>()
            .HasOne(o => o.UserCoupon)
            .WithMany()
            .HasForeignKey(o => o.UserCouponId)
            .OnDelete(DeleteBehavior.NoAction);

        // MerchantTradeNo 唯一索引（過濾 null 值，僅信用卡訂單會有值）
        builder.Entity<Order>()
            .HasIndex(o => o.MerchantTradeNo)
            .IsUnique()
            .HasFilter("MerchantTradeNo IS NOT NULL");

        // PasswordHistory 與 AppUser 關聯
        // 帳號刪除時一併刪除密碼歷史
        builder.Entity<PasswordHistory>()
            .HasOne(ph => ph.User)
            .WithMany()
            .HasForeignKey(ph => ph.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }

    /// <summary>
    /// 覆寫 SaveChangesAsync，攔截刪除操作並轉換為軟刪除。
    /// 當 EF Core ChangeTracker 偵測到 ISoftDeletable 實體被標記為 Deleted 時，
    /// 自動改為 Modified 狀態並設定 IsDeleted = true、DeletedAt = 當前時間。
    /// </summary>
    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        foreach (var entry in ChangeTracker.Entries<ISoftDeletable>())
        {
            if (entry.State == EntityState.Deleted)
            {
                // 將硬刪除轉換為軟刪除
                entry.State = EntityState.Modified;
                entry.Entity.IsDeleted = true;
                entry.Entity.DeletedAt = DateTime.UtcNow;
            }
        }

        return await base.SaveChangesAsync(cancellationToken);
    }
}
