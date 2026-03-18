using Microsoft.AspNetCore.Identity;
using MusicShop.Library.Enums;

namespace MusicShop.Data.Entities
{
    public class AppUser : IdentityUser
    {
        public string? FullName { get; set; }

        public DateTime RegisteredAt { get; set; } = DateTime.UtcNow;

        public DateTime? Birthday { get; set; }

        public Gender? Gender { get; set; }

        public ICollection<Order> Orders { get; set; } = new List<Order>();
        public ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();
        public ICollection<WishlistItem> WishlistItems { get; set; } = new List<WishlistItem>();
        public ICollection<UserCoupon> UserCoupons { get; set; } = new List<UserCoupon>();
    }
}