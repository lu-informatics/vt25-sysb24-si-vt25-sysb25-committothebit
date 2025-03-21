using System;
using System.Collections.ObjectModel;
using System.Linq;
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
        private string _apiResponse;
        private string _generatingAnimation;
        private string _recipeName;
        private string _recipeDescription;
        private string _recipeDifficultyLevel;
        private int _recipeCookingTime;
        private int _recipeServings;

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

        public string GeneratingAnimation
        {
            get => _generatingAnimation;
            set
            {
                _generatingAnimation = value;
                OnPropertyChanged();
            }
        }

        public string ApiResponse
        {
            get => _apiResponse;
            set
            {
                _apiResponse = value;
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

        public ObservableCollection<NumberedStep> NumberedStepsCollection { get; } = new();

        public ObservableCollection<string> RecipeIngredients { get; } = new();

        public ICommand GenerateRecipeCommand { get; }

    private async Task GenerateRecipeAsync()
    {
        var animationTask = AnimateRecipeTextAsync();

        // Fetch user ingredients
        AppUser appUser = await _appUserService.GetCurrentUserAsync();
        var appUserId = appUser.Id;

        IEnumerable<UserIngredient> userIngredients = await _userIngredientService.GetUserIngredientsByUserIdAsync(appUserId);

        // Convert user ingredients to a string
        string ingredientsList = string.Join(", ", userIngredients.Select(ui => ui.Ingredient?.Name));

        // Call the MagicRecipeGeneratorService to generate a recipe
        ApiResponse = await _magicRecipeGeneratorService.GenerateRecipeAsync(ingredientsList);

        Debug.WriteLine($"Hello");
        Debug.WriteLine(ApiResponse);

        // Parse API response to Recipe object
        Recipe recipe = ParseRecipe(ApiResponse);

        // Populate UI elements with recipe data
        PopulateRecipeData(recipe);

        // Parse ingredient names from the JSON response
        var ingredientNames = ParseIngredientNames(ApiResponse);

        // Set RecipeIngredients
        RecipeIngredients.Clear();
        foreach (var ingredient in ingredientNames)
        {
            RecipeIngredients.Add(ingredient);
        }
        OnPropertyChanged(nameof(RecipeIngredients));
        Debug.WriteLine($"RecipeIngredients: {RecipeIngredients.Count}");

        // Prepare steps data
        var tempSteps = new List<NumberedStep>();
        if (recipe?.ParsedData?.steps != null)
        {
            tempSteps = recipe.ParsedData.steps.Select((step, index) => new NumberedStep
            {
                StepNumber = $"{index + 1}.",
                StepText = step
            }).ToList();
        }

        // Update the NumberedStepsCollection
        NumberedStepsCollection.Clear();
        foreach (var step in tempSteps)
        {
            NumberedStepsCollection.Add(step);
        }

        // Notify UI that data has changed
        OnPropertyChanged(nameof(NumberedStepsCollection));
        Debug.WriteLine($"NumberedSteps: {NumberedStepsCollection.Count}");

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
                GeneratingAnimation = baseText + new string('.', dotCount);
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
                    Debug.WriteLine("Parsed recipe is null.");
                }

                return recipe ?? new Recipe();
            }
            catch (JsonException ex)
            {
                Debug.WriteLine($"JSON Parse Error: {ex.Message}");
                return new Recipe();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Unexpected Error: {ex.Message}");
                return new Recipe();
            }
        }

        private List<string> ParseIngredientNames(string apiResponse)
        {
            var jsonDocument = JsonDocument.Parse(apiResponse);
            return jsonDocument.RootElement.GetProperty("ingredientNames").EnumerateArray().Select(x => x.GetString()).ToList();
        }

        private void PopulateRecipeData(Recipe recipe)
        {
            RecipeName = recipe.Name;
            RecipeDescription = recipe.ParsedData?.description;
            RecipeDifficultyLevel = recipe.DifficultyLevel;
            RecipeCookingTime = recipe.CookingTime;
            RecipeServings = recipe.Servings;
        }
    }
}