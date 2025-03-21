using Informatics.Appetite.Interfaces;
using Informatics.Appetite.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Linq;
using System.Diagnostics;
using Microsoft.Maui.Controls;

namespace Informatics.Appetite.ViewModels;

public partial class RecipeDetailsViewModel : BaseViewModel
{
    private readonly IRecipeService _recipeService;
    private readonly IAppUserService _appUserService;
    private readonly IRecipeIngredientService _recipeIngredientService;
    private readonly IUserIngredientService _userIngredientService;
    private IEnumerable<UserIngredient> userIngredients;
    
    public bool IsInitialized { get; set; } = false;

    public ObservableCollection<RecipeIngredient> RecipeIngredients { get; } = new();
    public ObservableCollection<NumberedStep> NumberedStepsCollection { get; } = new();

    [ObservableProperty]
    private Recipe recipe;

    public RecipeDetailsViewModel(IRecipeService recipeService, IRecipeIngredientService recipeIngredientService, IUserIngredientService userIngredientService, IAppUserService appUserService)
    {
        Debug.WriteLine($"**DIAG** RecipeDetailsViewModel: Constructor started at {DateTime.Now:HH:mm:ss.fff}");
        _recipeService = recipeService;
        _recipeIngredientService = recipeIngredientService;
        _userIngredientService = userIngredientService;
        _appUserService = appUserService;
        SaveRecipeCommand = new AsyncRelayCommand(SaveRecipeAsync);
        DeleteRecipeCommand = new AsyncRelayCommand(DeleteRecipeAsync);
        Debug.WriteLine($"**DIAG** RecipeDetailsViewModel: Constructor completed at {DateTime.Now:HH:mm:ss.fff}");
    }

    public IAsyncRelayCommand SaveRecipeCommand { get; }
    public IAsyncRelayCommand DeleteRecipeCommand { get; }

    public async Task LoadRecipeAsync(int recipeId)
    {
        if (IsBusy) 
        {
            Debug.WriteLine($"**DIAG** LoadRecipeAsync: Already busy, skipping at {DateTime.Now:HH:mm:ss.fff}");
            return;
        }

        Debug.WriteLine($"**DIAG** LoadRecipeAsync: Started for recipeId={recipeId} at {DateTime.Now:HH:mm:ss.fff}");
        var methodStartTime = DateTime.Now;

        try
        {
            IsBusy = true;
            
            if (recipeId == -1)
            {
                Debug.WriteLine("**DIAG** LoadRecipeAsync: Creating new recipe");
                Recipe = new Recipe();
            }
            else
            {
                Debug.WriteLine($"**DIAG** LoadRecipeAsync: Loading existing recipe with ID {recipeId}");
                
                // Load the recipe data
                var recipeStartTime = DateTime.Now;
                Recipe = await _recipeService.GetRecipeByIdAsync(recipeId) ?? new Recipe();
                Debug.WriteLine($"**DIAG** LoadRecipeAsync: Loaded recipe in {(DateTime.Now - recipeStartTime).TotalMilliseconds:F1}ms");
                
                // Load recipe ingredients
                var ingredientsStartTime = DateTime.Now;
                var recipeIngredients = await _recipeIngredientService.GetRecipeIngredientsByRecipeIdAsync(recipeId);
                Debug.WriteLine($"**DIAG** LoadRecipeAsync: Loaded {recipeIngredients?.Count() ?? 0} recipe ingredients in {(DateTime.Now - ingredientsStartTime).TotalMilliseconds:F1}ms");

                try
                {        
                    // Load user's ingredients to check availability
                    var userStartTime = DateTime.Now;
                    AppUser appUser = await _appUserService.GetCurrentUserAsync();
                    int appUserId = appUser.Id;
                    Debug.WriteLine($"**DIAG** LoadRecipeAsync: Got user with ID {appUserId} in {(DateTime.Now - userStartTime).TotalMilliseconds:F1}ms");
                    
                    var userIngredientsStartTime = DateTime.Now;
                    userIngredients = await _userIngredientService.GetUserIngredientsByUserIdAsync(appUserId);
                    Debug.WriteLine($"**DIAG** LoadRecipeAsync: Got {userIngredients?.Count() ?? 0} user ingredients in {(DateTime.Now - userIngredientsStartTime).TotalMilliseconds:F1}ms");
                    
                    // Prepare data before UI updates
                    var prepStartTime = DateTime.Now;
                    var tempIngredients = new List<RecipeIngredient>();
                    
                    foreach (var recipeIngredient in recipeIngredients)
                    {
                        recipeIngredient.IsAvailable = userIngredients.Any(ui => 
                            ui.IngredientId == recipeIngredient.Ingredient.Id && ui.Amount >= recipeIngredient.Amount);
                        tempIngredients.Add(recipeIngredient);
                    }
                    Debug.WriteLine($"**DIAG** LoadRecipeAsync: Prepared {tempIngredients.Count} ingredients in {(DateTime.Now - prepStartTime).TotalMilliseconds:F1}ms");
                    
                    // Prepare steps data
                    var stepsStartTime = DateTime.Now;
                    var tempSteps = new List<NumberedStep>();
                    
                    if (Recipe?.ParsedData?.steps != null)
                    {
                        tempSteps = Recipe.ParsedData.steps.Select((step, index) => new NumberedStep
                        {
                            StepNumber = $"{index + 1}.",
                            StepText = step
                        }).ToList();
                    }
                    Debug.WriteLine($"**DIAG** LoadRecipeAsync: Prepared {tempSteps.Count} steps in {(DateTime.Now - stepsStartTime).TotalMilliseconds:F1}ms");
                    
                    // Batch update the UI
                    var uiUpdateStartTime = DateTime.Now;
                    
                    if (Application.Current != null)
                    {
                        Application.Current.Dispatcher.Dispatch(() => 
                        {
                            // Update ingredients
                            RecipeIngredients.Clear();
                            foreach (var ingredient in tempIngredients)
                            {
                                RecipeIngredients.Add(ingredient);
                            }
                            
                            // Update steps
                            NumberedStepsCollection.Clear();
                            foreach (var step in tempSteps)
                            {
                                NumberedStepsCollection.Add(step);
                            }
                            
                            // Notify UI that data has changed
                            OnPropertyChanged(nameof(NumberedSteps));
                        });
                    }
                    else
                    {
                        // Fallback if Application.Current is null
                        RecipeIngredients.Clear();
                        foreach (var ingredient in tempIngredients)
                        {
                            RecipeIngredients.Add(ingredient);
                        }
                        
                        NumberedStepsCollection.Clear();
                        foreach (var step in tempSteps)
                        {
                            NumberedStepsCollection.Add(step);
                        }
                        
                        // Notify UI that data has changed
                        OnPropertyChanged(nameof(NumberedSteps));
                    }
                    
                    Debug.WriteLine($"**DIAG** LoadRecipeAsync: Updated UI in {(DateTime.Now - uiUpdateStartTime).TotalMilliseconds:F1}ms");
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"**DIAG** ERROR loading user ingredients: {ex.Message}");
                    Debug.WriteLine($"**DIAG** Stack trace: {ex.StackTrace}");
                }
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"**DIAG** ERROR loading recipe: {ex.Message}");
            Debug.WriteLine($"**DIAG** Stack trace: {ex.StackTrace}");
        }
        finally
        {
            IsBusy = false;
            Debug.WriteLine($"**DIAG** LoadRecipeAsync: Completed at {DateTime.Now:HH:mm:ss.fff}, total time: {(DateTime.Now - methodStartTime).TotalMilliseconds:F1}ms");
        }
    }

    // Keep the computed property for binding
    public ObservableCollection<NumberedStep> NumberedSteps => NumberedStepsCollection;

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
            Debug.WriteLine($"**DIAG** ERROR saving recipe: {ex.Message}");
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
            Debug.WriteLine($"**DIAG** ERROR deleting recipe: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }
}