using Informatics.Appetite.Interfaces;
using Informatics.Appetite.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Linq;

namespace Informatics.Appetite.ViewModels;

public partial class RecipeDetailsViewModel : BaseViewModel
{
    private readonly IRecipeService _recipeService;
    private readonly IAppUserService _appUserService;
    private readonly IRecipeIngredientService _recipeIngredientService;
    private readonly IUserIngredientService _userIngredientService;
    private IEnumerable<UserIngredient> userIngredients;

    public ObservableCollection<RecipeIngredient> RecipeIngredients { get; } = new();

    [ObservableProperty]
    private Recipe recipe;

    public RecipeDetailsViewModel(IRecipeService recipeService, IRecipeIngredientService recipeIngredientService, IUserIngredientService userIngredientService, IAppUserService appUserService)
    {
        _recipeService = recipeService;
        _recipeIngredientService = recipeIngredientService;
        _userIngredientService = userIngredientService;
        _appUserService = appUserService;
        SaveRecipeCommand = new AsyncRelayCommand(SaveRecipeAsync);
        DeleteRecipeCommand = new AsyncRelayCommand(DeleteRecipeAsync);
    }

    public IAsyncRelayCommand SaveRecipeCommand { get; }
    public IAsyncRelayCommand DeleteRecipeCommand { get; }

    public async Task LoadRecipeAsync(int recipeId)
    {
        if (IsBusy) return;

        try
        {
            IsBusy = true;
            if (recipeId == -1)
            {
                Recipe = new Recipe();
            }
            else
            {
                Recipe = await _recipeService.GetRecipeByIdAsync(recipeId) ?? new Recipe();
                var recipeIngredients = await _recipeIngredientService.GetRecipeIngredientsByRecipeIdAsync(recipeId);

                try
                {        
                    AppUser appUser = await _appUserService.GetCurrentUserAsync();
                    int appUserId = appUser.Id;
                    IEnumerable<UserIngredient> userIngredients = await _userIngredientService.GetUserIngredientsByUserIdAsync(appUserId);
                    RecipeIngredients.Clear();
                    foreach (var recipeIngredient in recipeIngredients)
                    {
                        recipeIngredient.IsAvailable = userIngredients.Any(ui => 
                            ui.IngredientId == recipeIngredient.Ingredient.Id && ui.Amount>= recipeIngredient.Amount);
                        RecipeIngredients.Add(recipeIngredient);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error loading user ingredients: {ex.Message}");
                    // Handle the error appropriately
                }

                // Notify UI that NumberedSteps has changed
                OnPropertyChanged(nameof(NumberedSteps));
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading recipe: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    public ObservableCollection<NumberedStep> NumberedSteps
    {
        get
        {
            if (Recipe?.ParsedData?.steps == null)
                return new ObservableCollection<NumberedStep>();

            return new ObservableCollection<NumberedStep>(
                Recipe.ParsedData.steps.Select((step, index) => new NumberedStep
                {
                    StepNumber = $"{index + 1}.",  // Keep only the number
                    StepText = step               // The actual step text
                }));
        }
    }

    private async Task SaveRecipeAsync()
    {
        if (IsBusy) return;

        try
        {
            IsBusy = true;
            await _recipeService.SaveRecipeAsync(Recipe);
            await Shell.Current.GoToAsync(".."); // Navigate back
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error saving recipe: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task DeleteRecipeAsync()
    {
        if (IsBusy) return;

        try
        {
            IsBusy = true;
            await _recipeService.DeleteRecipeByIdAsync(Recipe.Id);
            await Shell.Current.GoToAsync(".."); // Navigate back
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error deleting recipe: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }
}