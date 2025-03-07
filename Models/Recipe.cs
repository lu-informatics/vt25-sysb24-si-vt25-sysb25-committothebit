using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;

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
    public string? Data { get; set; }

    [Column("cookingTime")]
    [Range(1, int.MaxValue)]
    public int CookingTime { get; set; }

    [Column("servings")]
    [Range(1, int.MaxValue)]
    public int Servings { get; set; }

    [Column("difficultyLevel")]
    [MaxLength(50)]
    public string? DifficultyLevel { get; set; }

    [NotMapped]
    public RecipeData? ParsedData
    {
        get
        {
            var parsed = RecipeData.FromJson(Data);
            if (parsed == null)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to parse Data: {Data}");
            }
            return parsed;
        }
    }

    [NotMapped]
    public string Description => $"Cook Time: {CookingTime} mins • Serves: {Servings} • Difficulty: {DifficultyLevel}";

    [NotMapped]
    public string StepsDescription => ParsedData != null && ParsedData.steps.Any() ? 
        string.Join("\n", ParsedData.steps.Select((step, index) => $"{index + 1}. {step}")) : 
        "No steps available";

    public ICollection<RecipeIngredient> RecipeIngredients { get; set; } = new List<RecipeIngredient>();
}
