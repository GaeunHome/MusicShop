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

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

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

        // Banner 與 Album 關聯（刪除商品時，幻燈片保留但連結設為 null）
        builder.Entity<Banner>()
            .HasOne(b => b.Album)
            .WithMany()
            .HasForeignKey(b => b.AlbumId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}