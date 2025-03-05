using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Informatics.Appetite.Models;

[Table("UserIngredient")]
public class UserIngredient
{
    [Required]
    [Column("AppUserId")]
    public int AppUserId { get; set; }
    public AppUser? AppUser { get; set; }

    [Required]
    [Column("IngredientId")]
    public int IngredientId { get; set; }
    public Ingredient? Ingredient { get; set; }

    [Required]
    [Column("Amount", TypeName = "decimal(10,2)")]
    public double Amount { get; set; }
}
