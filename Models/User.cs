using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MusicShop.Models;

[Table("User")]
public class User
{
    [Key]
    public int Id { get; set; }
    public string user_name { get; set; } = string.Empty;
}