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
            RecipeText = "Generating recipe...";
            RecipeText = await _magicRecipeGeneratorService.GenerateRecipeAsync();
        }
    }
}