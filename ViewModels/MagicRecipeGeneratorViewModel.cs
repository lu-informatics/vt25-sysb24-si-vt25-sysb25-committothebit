using System;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Maui.Controls;
using Informatics.Appetite.Services;
using Informatics.Appetite.Interfaces;
using Informatics.Appetite.Models;
using System.Text.Json;
using System.Diagnostics;

namespace Informatics.Appetite.ViewModels
{
    public class MagicRecipeGeneratorViewModel : BindableObject
    {
        private string _recipeText;
        private string _recipeName;
        private string _recipeDescription;
        private string _recipeDifficultyLevel;
        private int _recipeCookingTime;
        private int _recipeServings;
        private string _recipeIngredients;
        private string _numberedSteps;

        private Recipe _recipe;
        private readonly IMagicRecipeGeneratorService _magicRecipeGeneratorService;
        private readonly IUserIngredientService _userIngredientService;
        private readonly IAppUserService _appUserService;

        public MagicRecipeGeneratorViewModel(IMagicRecipeGeneratorService magicRecipeGeneratorService, IUserIngredientService userIngredientService, IAppUserService appUserService)
        {
            _magicRecipeGeneratorService = magicRecipeGeneratorService;
            _userIngredientService = userIngredientService;
            _appUserService = appUserService;
            GenerateRecipeCommand = new Command(async () => await GenerateRecipeAsync());
        }

        public Recipe Recipe
        {
            get => _recipe;
            set
            {
                _recipe = value;
                OnPropertyChanged();
            }
        }

        public string RecipeText
        {
            get => _recipeText;
            set
            {
                _recipeText = value;
                OnPropertyChanged();
            }
        }

        public string RecipeName
        {
            get => _recipeName;
            set
            {
                _recipeName = value;
                OnPropertyChanged();
            }
        }

        public string RecipeDescription
        {
            get => _recipeDescription;
            set
            {
                _recipeDescription = value;
                OnPropertyChanged();
            }
        }

        public string RecipeDifficultyLevel
        {
            get => _recipeDifficultyLevel;
            set
            {
                _recipeDifficultyLevel = value;
                OnPropertyChanged();
            }
        }

        public int RecipeCookingTime
        {
            get => _recipeCookingTime;
            set
            {
                _recipeCookingTime = value;
                OnPropertyChanged();
            }
        }

        public int RecipeServings
        {
            get => _recipeServings;
            set
            {
                _recipeServings = value;
                OnPropertyChanged();
            }
        }

        public string RecipeIngredients
        {
            get => _recipeIngredients;
            set
            {
                _recipeIngredients = value;
                OnPropertyChanged();
            }
        }

        public string NumberedSteps
        {
            get => _numberedSteps;
            set
            {
                _numberedSteps = value;
                OnPropertyChanged();
            }
        }

        public ICommand GenerateRecipeCommand { get; }

        private async Task GenerateRecipeAsync()
        {
            var animationTask = AnimateRecipeTextAsync();

            // Fetch user ingredients
            AppUser appUser = await _appUserService.GetCurrentUserAsync();
            var appUserId = appUser.Id;

            IEnumerable<UserIngredient> userIngredients = await _userIngredientService.GetUserIngredientsByUserIdAsync(appUserId);

            //Convert user ingredients to a string
            string ingredientsList = string.Join(", ", userIngredients.Select(ui => ui.Ingredient?.Name));

            // Call the MagicRecipeGeneratorService to generate a recipe
            RecipeText = await _magicRecipeGeneratorService.GenerateRecipeAsync(ingredientsList);

            Debug.WriteLine($"Hello");
            Debug.WriteLine(RecipeText);

            // Parse API response to Recipe object
            Recipe recipe = ParseRecipe(RecipeText);

            // Populate UI elements with recipe data
            RecipeName = recipe.Name;
            RecipeDescription = recipe.ParsedData?.description;
            RecipeDifficultyLevel = recipe.DifficultyLevel;
            RecipeCookingTime = recipe.CookingTime;
            RecipeServings = recipe.Servings;
            RecipeIngredients = "Testing...";
            NumberedSteps = "Testing...";
            


            // Stop the animation
            _isAnimating = false;
        }

        private bool _isAnimating;

        private async Task AnimateRecipeTextAsync()
        {
            _isAnimating = true;
            string baseText = "Generating recipe ";
            int dotCount = 0;

            while (_isAnimating)
            {
                dotCount = (dotCount % 3) + 1;
                RecipeText = baseText + new string('.', dotCount);
                await Task.Delay(500); // Adjust the delay as needed
            }
        }

        // Method for parsing the API response to a Recipe object
        public Recipe ParseRecipe(string apiResponse)
        {
            try
            {
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                var recipe = JsonSerializer.Deserialize<Recipe>(apiResponse, options);

                if (recipe != null)
                {
                    Debug.WriteLine($"Recipe Name: {recipe.Name}");
                    Debug.WriteLine($"Recipe Data: {recipe.Data}");
                    Debug.WriteLine($"Recipe Difficulty Level: {recipe.DifficultyLevel}");
                    Debug.WriteLine($"Recipe Cooking Time: {recipe.CookingTime}");
                    Debug.WriteLine($"Recipe Servings: {recipe.Servings}");
                    Debug.WriteLine($"Recipe Description: {recipe.ParsedData?.description}");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("Parsed recipe is null.");
                }

                return recipe ?? new Recipe();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"JSON Parse Error: {ex.Message}");
                return new Recipe();
            }
        }
    }
}