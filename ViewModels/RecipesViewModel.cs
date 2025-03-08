using Informatics.Appetite.Interfaces;
using Informatics.Appetite.Models;
using Informatics.Appetite.Pages;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Linq;


namespace Informatics.Appetite.ViewModels;

public partial class RecipesViewModel : BaseViewModel
{
    private readonly IRecipeService _recipeService;
    private string _searchText;

    public ObservableCollection<Recipe> Recipes { get; }
    public ObservableCollection<Recipe> FilteredRecipes { get; }

    public ObservableCollection<string> Difficulties { get; } = new ObservableCollection<string>();
    public ObservableCollection<string> CookingTimes { get; } = new ObservableCollection<string>();

    private string _selectedDifficulty = "Difficulty";
    public string SelectedDifficulty
    {
        get => _selectedDifficulty;
        set
        {
            if (SetProperty(ref _selectedDifficulty, value))
            {
                FilterRecipes();
            }
        }
    }

    private string _selectedCookingTime = "Cooking time";
    public string SelectedCookingTime
    {
        get => _selectedCookingTime;
        set
        {
            if (SetProperty(ref _selectedCookingTime, value))
            {
                FilterRecipes();
            }
        }
    }

    public IAsyncRelayCommand RefreshCommand { get; }
    public IRelayCommand<int> OpenRecipeDetailsCommand { get; }
    public IRelayCommand OpenAddRecipeCommand { get; }

    public RecipesViewModel(IRecipeService recipeService)
    {
        _recipeService = recipeService;
        Recipes = new ObservableCollection<Recipe>();
        FilteredRecipes = new ObservableCollection<Recipe>();
        Title = "Recipes";

        RefreshCommand = new AsyncRelayCommand(LoadRecipesAsync);
        OpenRecipeDetailsCommand = new RelayCommand<int>(OpenRecipeDetails);
        OpenAddRecipeCommand = new RelayCommand(OpenAddRecipe);
        
        _ = LoadDifficultyLevelsAsync();
        _ = LoadCookingTimesAsync();
    }

    private async Task LoadRecipesAsync()
    {
        if (IsBusy) return;
        try
        {
            IsBusy = true;
            var recipes = await _recipeService.GetRecipesAsync();
            Recipes.Clear();
            foreach (var recipe in recipes)
            {
                Recipes.Add(recipe);
            }
            FilterRecipes();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading recipes: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    private void FilterRecipes()
    {
        if (string.IsNullOrWhiteSpace(SearchText) &&
            (string.IsNullOrWhiteSpace(SelectedDifficulty) || SelectedDifficulty == "Difficulty") &&
            (string.IsNullOrWhiteSpace(SelectedCookingTime) || SelectedCookingTime == "Cooking time"))
        {
            FilteredRecipes.Clear();
            foreach (var recipe in Recipes)
            {
                FilteredRecipes.Add(recipe);
            }
            return;
        }

        var filtered = Recipes.Where(r =>
             (string.IsNullOrWhiteSpace(SearchText) || r.Name.ToLower().Contains(SearchText.ToLower())) &&
             (string.IsNullOrWhiteSpace(SelectedDifficulty) || SelectedDifficulty == "Difficulty" ||
                 (r.DifficultyLevel != null && r.DifficultyLevel.Equals(SelectedDifficulty, StringComparison.OrdinalIgnoreCase))) &&
             (string.IsNullOrWhiteSpace(SelectedCookingTime) || SelectedCookingTime == "Cooking time" ||
                 r.CookingTime.ToString() == SelectedCookingTime)
        ).ToList();

        FilteredRecipes.Clear();
        foreach (var recipe in filtered)
        {
            FilteredRecipes.Add(recipe);
        }
    }

    private async Task LoadDifficultyLevelsAsync()
    {
        var difficultyLevels = await _recipeService.GetDifficultyLevelsAsync();
        Difficulties.Clear();
        Difficulties.Add("Difficulty");
        foreach (var level in difficultyLevels)
        {
            Difficulties.Add(level);
        }
        SelectedDifficulty = "Difficulty";
    }

    private async Task LoadCookingTimesAsync()
    {
        var cookingTimes = await _recipeService.GetCookingTimesAsync();
        CookingTimes.Clear();
        CookingTimes.Add("Cooking time");
        foreach (var time in cookingTimes)
        {
            CookingTimes.Add(time);
        }
        SelectedCookingTime = "Cooking time";
    }
    public string SearchText
    {
        get => _searchText;
        set
        {
            SetProperty(ref _searchText, value);
            FilterRecipes();
        }
    }

    private void OpenAddRecipe() => OpenRecipeDetails(-1);

    private async void OpenRecipeDetails(int id)
    {
        await Shell.Current.GoToAsync($"{nameof(RecipeDetailsPage)}?recipeId={id}");
    }
}
