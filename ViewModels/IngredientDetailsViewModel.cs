using Informatics.Appetite.Interfaces;
using Informatics.Appetite.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace Informatics.Appetite.ViewModels;

public partial class IngredientDetailsViewModel : BaseViewModel
{
    private readonly IIngredientService _ingredientService;
    private readonly IRecipeIngredientService _recipeIngredientService;
    private readonly IUserIngredientService _userIngredientService;
    private readonly IAppUserService _appUserService;
    public ObservableCollection<Ingredient> Ingredients { get; } = new();
    public ObservableCollection<Recipe> Recipes { get; } = new();

    [ObservableProperty]
    private Ingredient _selectedIngredient;

    [ObservableProperty]
    private string _amount;


    public IngredientDetailsViewModel(IIngredientService ingredientService, IRecipeIngredientService recipeIngredientService, IUserIngredientService userIngredientService, IAppUserService appUserService)
    {
        _ingredientService = ingredientService;
        _recipeIngredientService = recipeIngredientService;
        _userIngredientService = userIngredientService;
        _appUserService = appUserService;
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

        AppUser user = await _appUserService.GetCurrentUserAsync();

        try
        {
            IsBusy = true;

            if (!double.TryParse(Amount, out double parsedAmount))
            {
                await Application.Current.MainPage.DisplayAlert("Error", "Please enter a number", "OK");
                return;
            }
            else if (parsedAmount < 0)
            {
                await Application.Current.MainPage.DisplayAlert("Error", "Please enter a positive number", "OK");
                return;
            }
            else if (parsedAmount == 0)
            {
                bool confirm = await Application.Current.MainPage.DisplayAlert("Confirm", "Are you sure you want to set the amount to 0? This will delete the ingredient from My Ingredients.", "OK", "Cancel");
                if (confirm)
                {
                    var result = await _userIngredientService.DeleteUserIngredientAsync(user.Id, SelectedIngredient.Id); // Hardcoded user id
                    if (result)
                    {
                        await Shell.Current.GoToAsync(".."); // Navigate back
                    }
                    else
                    {
                        await Application.Current.MainPage.DisplayAlert("Error", "Failed to delete ingredient.", "OK");
                    }
                }
                return;
            }

            var userIngredient = new UserIngredient
            {
                AppUserId = user.Id,
                IngredientId = SelectedIngredient.Id,
                Amount = parsedAmount
            };
            Debug.WriteLine("HEEEELLLOO");
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