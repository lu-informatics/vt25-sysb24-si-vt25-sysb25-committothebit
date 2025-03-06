using Informatics.Appetite.Interfaces;
using Informatics.Appetite.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;

namespace Informatics.Appetite.ViewModels;

public partial class IngredientDetailsViewModel : BaseViewModel
{
    private readonly IIngredientService _ingredientService;
    private readonly IRecipeIngredientService _recipeIngredientService;
    
    public ObservableCollection<Ingredient> Ingredients { get; } = new();
    public ObservableCollection<Recipe> Recipes { get; } = new();


    public IngredientDetailsViewModel(IIngredientService ingredientService, IRecipeIngredientService recipeIngredientService)
    {
        _ingredientService = ingredientService;
        _recipeIngredientService = recipeIngredientService;
        Ingredients = new ObservableCollection<Ingredient>();
        Recipes = new ObservableCollection<Recipe>();

        RefreshCommand = new AsyncRelayCommand(LoadIngredientsAsync);
        //SaveIngredientCommand = new AsyncRelayCommand(SaveIngredientAsync);
    }

    public IAsyncRelayCommand RefreshCommand { get; }
    public IAsyncRelayCommand SaveIngredientCommand { get; }
    public IAsyncRelayCommand DeleteIngredientCommand { get; }

    public async Task LoadIngredientsAsync()
    {
        if (IsBusy) return;

        try
        {
            IsBusy = true;
            var ingredients = await _ingredientService.GetIngredientsAsync();
            Ingredients.Clear();
            foreach (var ingredient in ingredients)
            {
                Ingredients.Add(ingredient);
            }
        } 
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading ingredients: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    // private async Task SaveIngredientAsync(Ingredient ingredient)
    // {
    //     if (IsBusy) return;

    //     try
    //     {
    //         IsBusy = true;
    //         await _ingredientService.SaveIngredientAsync(ingredient);
    //         await Shell.Current.GoToAsync(".."); // Navigate back
    //     }
    //     catch (Exception ex)
    //     {
    //         Console.WriteLine($"Error saving ingredient: {ex.Message}");
    //     }
    //     finally
    //     {
    //         IsBusy = false;
    //     }
    // }
}