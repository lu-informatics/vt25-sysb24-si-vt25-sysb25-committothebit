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

    public ObservableCollection<string> DietTags { get; } = new ObservableCollection<string>();

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

    private string _selectedDietTag = "DietTag";
        public string SelectedDietTag
        {
            get => _selectedDietTag;
            set
            {
                if (SetProperty(ref _selectedDietTag, value))
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
        _ = LoadDietTagsAsync();
    }

    private async Task LoadRecipesAsync()
{
    if (IsBusy) return;
    try
    {
        IsBusy = true;

        // Load the main recipes
        var recipes = await _recipeService.GetRecipesAsync();
        Recipes.Clear();
        foreach (var recipe in recipes)
            Recipes.Add(recipe);

        // Also load Difficulty & CookingTime pickers here, awaited
        await LoadDifficultyLevelsAsync();
        await LoadCookingTimesAsync();
        await LoadDietTagsAsync();

        // Finally filter after everything is loaded
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
    var filtered = Recipes.Where(r =>
    {
        // Apply search, difficulty, and cooking time filters (as you already have)
        bool matchesSearch = string.IsNullOrWhiteSpace(SearchText) || r.Name.ToLower().Contains(SearchText.ToLower());
        bool matchesDifficulty = string.IsNullOrWhiteSpace(SelectedDifficulty) || SelectedDifficulty == "Difficulty" ||
            (r.DifficultyLevel != null && r.DifficultyLevel.Equals(SelectedDifficulty, StringComparison.OrdinalIgnoreCase));
        bool matchesCookingTime = string.IsNullOrWhiteSpace(SelectedCookingTime) || SelectedCookingTime == "Cooking time" ||
            r.CookingTime.ToString() == SelectedCookingTime;

        // DietTag filtering
        bool matchesDiet = true;
        if (!string.IsNullOrWhiteSpace(SelectedDietTag) && SelectedDietTag != "DietTag")
        {
            // Gather all diettags for the current recipe from its ingredients
            var recipeDietTags = r.RecipeIngredients
                .Select(ri => ri.Ingredient?.DietTag)
                .Where(tag => !string.IsNullOrWhiteSpace(tag))
                .Distinct()
                .ToList();

            if (SelectedDietTag.Equals("Non-Vegetarian", StringComparison.OrdinalIgnoreCase))
            {
                // For non-vegetarian, we show all recipes regardless
                matchesDiet = true;
            }
            else if (SelectedDietTag.Equals("Pescatarian", StringComparison.OrdinalIgnoreCase))
            {
                // Exclude if any ingredient is Non-Vegetarian
                matchesDiet = !recipeDietTags.Any(tag => tag.Equals("Non-Vegetarian", StringComparison.OrdinalIgnoreCase));
            }
            else if (SelectedDietTag.Equals("Vegetarian", StringComparison.OrdinalIgnoreCase))
            {
                // Exclude if any ingredient is Non-Vegetarian or Pescatarian
                matchesDiet = !recipeDietTags.Any(tag => 
                    tag.Equals("Non-Vegetarian", StringComparison.OrdinalIgnoreCase) || 
                    tag.Equals("Pescatarian", StringComparison.OrdinalIgnoreCase));
            }
            else if (SelectedDietTag.Equals("Vegan", StringComparison.OrdinalIgnoreCase))
            {
                // Exclude if any ingredient is Non-Vegetarian, Pescatarian or Vegetarian
                matchesDiet = !recipeDietTags.Any(tag => 
                    tag.Equals("Non-Vegetarian", StringComparison.OrdinalIgnoreCase) || 
                    tag.Equals("Pescatarian", StringComparison.OrdinalIgnoreCase) ||
                    tag.Equals("Vegetarian", StringComparison.OrdinalIgnoreCase));
            }
        }

        return matchesSearch && matchesDifficulty && matchesCookingTime && matchesDiet;
    }).ToList();

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

    private async Task LoadDietTagsAsync()
{
        var tags = await _recipeService.GetDietTagsAsync();
        DietTags.Clear();
        DietTags.Add("DietTag"); // Placeholder option
        foreach (var tag in tags)
        {
            DietTags.Add(tag);
        }
        SelectedDietTag = "DietTag";
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
