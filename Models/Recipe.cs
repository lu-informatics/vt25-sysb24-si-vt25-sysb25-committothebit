using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Informatics.Appetite.Models;

[Table("Recipe")]
public class Recipe
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("recipeId")]
    public int Id { get; set; }

    [Required]
    [Column("name")]
    [MaxLength(255)]
    public string? Name { get; set; }

    [Column("data")]
    public string? Instructions { get; set; }

    [Column("cookingTime")]
    [Range(1, int.MaxValue)]
    public int CookingTime { get; set; }

    [Column("servings")]
    [Range(1, int.MaxValue)]
    public int Servings { get; set; }

    [Column("difficultyLevel")]
    [MaxLength(50)]
    public string? DifficultyLevel { get; set; }

    public ICollection<RecipeIngredient> RecipeIngredients { get; set; } = new List<RecipeIngredient>();
}
