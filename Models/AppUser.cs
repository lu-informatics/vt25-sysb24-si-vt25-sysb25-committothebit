using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Informatics.Appetite.Models;

[Table ("AppUser")]
public class AppUser
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("AppUserID")]
    public int Id { get; set; }

    [Required]
    [Column("Username")]
    [MaxLength(50)]
    public string? Username { get; set; }

    public ICollection<UserIngredient> UserIngredients { get; set; } = new List<UserIngredient>();


}
