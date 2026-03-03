using System.ComponentModel.DataAnnotations;

namespace MusicShop.Models
{
    public class Category
    {
        public int Id { get; set; }

        [Required, StringLength(100)]
        public string Name { get; set; } = string.Empty;

        public ICollection<Album> Albums { get; set; } = new List<Album>();
    }
}