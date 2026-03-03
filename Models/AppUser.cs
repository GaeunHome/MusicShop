using Microsoft.AspNetCore.Identity;

namespace MusicShop.Models
{
    public class AppUser : IdentityUser
    {
        public string? FullName { get; set; }

        public DateTime RegisteredAt { get; set; } = DateTime.Now;

        public ICollection<Order> Orders { get; set; } = new List<Order>();
        public ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();
    }
}