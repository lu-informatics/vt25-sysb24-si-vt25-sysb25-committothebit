using System;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Maui.Controls;
using Informatics.Appetite.Services;
using Informatics.Appetite.Interfaces;
using Informatics.Appetite.Models;

namespace Informatics.Appetite.ViewModels
{
    public class MagicRecipeGeneratorViewModel : BindableObject
    {
        private string _recipeText;
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

        public string RecipeText
        {
            get => _recipeText;
            set
            {
                _recipeText = value;
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
    }
}