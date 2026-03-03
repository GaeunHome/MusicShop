using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using MusicShop.Models;

namespace MusicShop.Data;

public class ApplicationDbContext : IdentityDbContext<AppUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }
    public DbSet<Album> Albums { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderItem> OrderItems { get; set; }
    public DbSet<CartItem> CartItems { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Album 與 Category 關聯
        builder.Entity<Album>()
            .HasOne(a => a.Category)
            .WithMany(c => c.Albums)
            .HasForeignKey(a => a.CategoryId)
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
    }
}