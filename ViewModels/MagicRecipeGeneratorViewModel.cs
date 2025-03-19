using System;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Maui.Controls;
using Informatics.Appetite.Services;
using Informatics.Appetite.Interfaces;

namespace Informatics.Appetite.ViewModels
{
    public class MagicRecipeGeneratorViewModel : BindableObject
    {
        private string _recipeText;
        private readonly IMagicRecipeGeneratorService _magicRecipeGeneratorService;

        public MagicRecipeGeneratorViewModel(IMagicRecipeGeneratorService magicRecipeGeneratorService)
        {
            _magicRecipeGeneratorService = magicRecipeGeneratorService;
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
            RecipeText = await _magicRecipeGeneratorService.GenerateRecipeAsync();
            _isAnimating = false; // Stop the animation
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