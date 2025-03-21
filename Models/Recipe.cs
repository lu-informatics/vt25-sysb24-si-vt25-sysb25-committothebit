using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using Microsoft.Maui.Graphics;

namespace Informatics.Appetite.Models
{
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
            public Color DifficultyColor
            {
                get
                {
                    return DifficultyLevel switch
                    {
                        "Easy" => Colors.Green,
                        "Medium" => Color.FromArgb("#DDAA00"),
                        "Hard" => Colors.Red,
                        _ => Colors.Gray // Default color if difficulty is unknown
                    };
                }
}
        // Existing logic: automatically determine the DietTag based on the recipe's ingredients
        [NotMapped]
        public string DietTag
        {
            get
            {
                // Check if any ingredient is Non-Vegetarian first
                if (RecipeIngredients.Any(ri => ri.Ingredient != null &&
                    ri.Ingredient.DietTag.Equals("Non-Vegetarian", StringComparison.OrdinalIgnoreCase)))
                {
                    return "Non-Vegetarian";
                }
                // Next, check for Pescatarian
                else if (RecipeIngredients.Any(ri => ri.Ingredient != null &&
                    ri.Ingredient.DietTag.Equals("Pescatarian", StringComparison.OrdinalIgnoreCase)))
                {
                    return "Pescatarian";
                }
                // Then, check for Vegetarian
                else if (RecipeIngredients.Any(ri => ri.Ingredient != null &&
                    ri.Ingredient.DietTag.Equals("Vegetarian", StringComparison.OrdinalIgnoreCase)))
                {
                    return "Vegetarian";
                }
                // Finally, if all ingredients are Vegan
                else if (RecipeIngredients.All(ri => ri.Ingredient != null &&
                    ri.Ingredient.DietTag.Equals("Vegan", StringComparison.OrdinalIgnoreCase)))
                {
                    return "Vegan";
                }
                // Fallback if no valid tags are found
                return "Unknown";
            }
        }


        // Navigation property
        public ICollection<RecipeIngredient> RecipeIngredients { get; set; } = new List<RecipeIngredient>();
        
    [NotMapped]
    public bool HasAllIngredients{ get; set;} = false;
    }

}
