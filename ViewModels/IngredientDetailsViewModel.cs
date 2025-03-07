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
    private readonly IUserIngredientService _userIngredientService;
    
    public ObservableCollection<Ingredient> Ingredients { get; } = new();
    public ObservableCollection<Recipe> Recipes { get; } = new();

    [ObservableProperty]
    private Ingredient _selectedIngredient;

    [ObservableProperty]
    private double _amount;


    public IngredientDetailsViewModel(IIngredientService ingredientService, IRecipeIngredientService recipeIngredientService, IUserIngredientService userIngredientService)
    {
        _ingredientService = ingredientService;
        _recipeIngredientService = recipeIngredientService;
        _userIngredientService = userIngredientService;
        Ingredients = new ObservableCollection<Ingredient>();
        Recipes = new ObservableCollection<Recipe>();

        RefreshCommand = new AsyncRelayCommand(LoadIngredientsAsync);
        SaveIngredientCommand = new AsyncRelayCommand(SaveUserIngredientAsync);
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

    private async Task SaveUserIngredientAsync()
    {
        if (IsBusy) return;

        try
        {
            IsBusy = true;
            var userIngredient = new UserIngredient
            {
                AppUserId = 1, // Hardcoded user id
                IngredientId = SelectedIngredient.Id,
                Amount = Amount
            };
            await _userIngredientService.SaveUserIngredientAsync(userIngredient);
            await Shell.Current.GoToAsync(".."); // Navigate back
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error saving user ingredient: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }
}