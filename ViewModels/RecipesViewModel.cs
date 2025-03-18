using Informatics.Appetite.Interfaces;
using Informatics.Appetite.Models;
using Informatics.Appetite.Pages;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Linq;
using System.Diagnostics;
using Microsoft.Maui.Controls;

namespace Informatics.Appetite.ViewModels;

public partial class RecipesViewModel : BaseViewModel
{
    private readonly IRecipeService _recipeService;
    private readonly IUserIngredientService _userIngredientService;
    private readonly IAppUserService _appUserService;
    private string _searchText;
    private IEnumerable<UserIngredient> _userIngredients;

    public bool IsInitialized { get; set; } = false;

    public ObservableCollection<Recipe> Recipes { get; }
    public ObservableCollection<Recipe> FilteredRecipes { get; }

    public ObservableCollection<string> Difficulties { get; } = new ObservableCollection<string>();
    public ObservableCollection<string> CookingTimes { get; } = new ObservableCollection<string>();

    public ObservableCollection<string> DietTags { get; } = new ObservableCollection<string>();

    public ObservableCollection<string> Categories { get; } = new ObservableCollection<string>();

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
    private string _selectedCategory = "Category";
public string SelectedCategory
{
    get => _selectedCategory;
    set
    {
        if (SetProperty(ref _selectedCategory, value))
        {
            FilterRecipes();
        }
    }
}
    private string _selectedDietTag = "Diet tag";
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

    public RecipesViewModel(IRecipeService recipeService, IUserIngredientService userIngredientService, IAppUserService appUserService)
    {
        Debug.WriteLine($"**DIAG** Constructor started at {DateTime.Now:HH:mm:ss.fff}");
        _recipeService = recipeService;
        _userIngredientService = userIngredientService;
        _appUserService = appUserService;
        Recipes = new ObservableCollection<Recipe>();
        FilteredRecipes = new ObservableCollection<Recipe>();
        Title = "Recipes";

        RefreshCommand = new AsyncRelayCommand(LoadRecipesAsync);
        OpenRecipeDetailsCommand = new RelayCommand<int>(OpenRecipeDetails);
        OpenAddRecipeCommand = new RelayCommand(OpenAddRecipe);
        
        Debug.WriteLine($"**DIAG** Constructor completed at {DateTime.Now:HH:mm:ss.fff}");
    }

    public async Task LoadRecipesAsync()
    {
        if (IsBusy) 
        {
            Debug.WriteLine($"**DIAG** LoadRecipesAsync: Already busy, skipping at {DateTime.Now:HH:mm:ss.fff}");
            return;
        }
        
        Debug.WriteLine($"**DIAG** LoadRecipesAsync: Started at {DateTime.Now:HH:mm:ss.fff}");
        var methodStartTime = DateTime.Now;
        
        try
        {
            IsBusy = true;
            Debug.WriteLine("**DIAG** LoadRecipesAsync: Starting to load data");

            // Load user ingredients first
            Debug.WriteLine("**DIAG** LoadRecipesAsync: Getting current user");
            var userStartTime = DateTime.Now;
            AppUser appUser = await _appUserService.GetCurrentUserAsync();
            int appUserId = appUser.Id;
            Debug.WriteLine($"**DIAG** LoadRecipesAsync: Got user with ID {appUserId} in {(DateTime.Now - userStartTime).TotalMilliseconds:F1}ms");
            
            Debug.WriteLine("**DIAG** LoadRecipesAsync: Getting user ingredients");
            var ingredientsStartTime = DateTime.Now;
            _userIngredients = await _userIngredientService.GetUserIngredientsByUserIdAsync(appUserId);
            Debug.WriteLine($"**DIAG** LoadRecipesAsync: Got {_userIngredients?.Count() ?? 0} user ingredients in {(DateTime.Now - ingredientsStartTime).TotalMilliseconds:F1}ms");

            // Load the main recipes
            Debug.WriteLine("**DIAG** LoadRecipesAsync: Getting recipes");
            var recipesStartTime = DateTime.Now;
            var recipes = await _recipeService.GetRecipesAsync();
            Debug.WriteLine($"**DIAG** LoadRecipesAsync: Got {recipes?.Count() ?? 0} recipes in {(DateTime.Now - recipesStartTime).TotalMilliseconds:F1}ms");
            
            var clearStartTime = DateTime.Now;
            Recipes.Clear();
            Debug.WriteLine($"**DIAG** LoadRecipesAsync: Cleared recipes in {(DateTime.Now - clearStartTime).TotalMilliseconds:F1}ms");
            
            if (recipes != null)
            {
                var processStartTime = DateTime.Now;
                int count = 0;
                foreach (var recipe in recipes)
                {
                    var recipeStartTime = DateTime.Now;
                    recipe.HasAllIngredients = RecipeHasAllIngredients(recipe);
                    Recipes.Add(recipe);
                    count++;
                    if (count % 10 == 0 || count == recipes.Count())
                    {
                        Debug.WriteLine($"**DIAG** LoadRecipesAsync: Processed {count}/{recipes.Count()} recipes in {(DateTime.Now - processStartTime).TotalMilliseconds:F1}ms");
                    }
                }
                Debug.WriteLine($"**DIAG** LoadRecipesAsync: Added {Recipes.Count} recipes to collection in {(DateTime.Now - processStartTime).TotalMilliseconds:F1}ms");
            }
            else
            {
                Debug.WriteLine("**DIAG** ERROR: recipes is null!");
            }

            var filterStartTime = DateTime.Now;
            FilterRecipes();
            Debug.WriteLine($"**DIAG** LoadRecipesAsync: FilterRecipes took {(DateTime.Now - filterStartTime).TotalMilliseconds:F1}ms");
            Debug.WriteLine($"**DIAG** LoadRecipesAsync: After filtering: {FilteredRecipes.Count} recipes");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"**DIAG** ERROR loading recipes: {ex.Message}");
            Debug.WriteLine($"**DIAG** Stack trace: {ex.StackTrace}");
        }
        finally
        {
            IsBusy = false;
            Debug.WriteLine($"**DIAG** LoadRecipesAsync: Completed at {DateTime.Now:HH:mm:ss.fff}, total time: {(DateTime.Now - methodStartTime).TotalMilliseconds:F1}ms");
        }
    }

    private void FilterRecipes()
    {
        Debug.WriteLine($"**DIAG** FilterRecipes: Started at {DateTime.Now:HH:mm:ss.fff}");
        try 
        {
            var startTime = DateTime.Now;
            
            // Start with all recipes
            var filteredList = new List<Recipe>(Recipes);
            Debug.WriteLine($"**DIAG** FilterRecipes: Created filtered list with {filteredList.Count} recipes in {(DateTime.Now - startTime).TotalMilliseconds:F1}ms");
            
            // Apply search filter
            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                Debug.WriteLine($"**DIAG** FilterRecipes: Filtering by search text '{SearchText}'");
                filteredList = filteredList.Where(r => 
                    r.Name?.Contains(SearchText, StringComparison.OrdinalIgnoreCase) == true).ToList();
            }
            
            // Apply difficulty filter
            if (!string.IsNullOrEmpty(SelectedDifficulty) && SelectedDifficulty != "Difficulty")
            {
                Debug.WriteLine($"**DIAG** FilterRecipes: Filtering by difficulty '{SelectedDifficulty}'");
                filteredList = filteredList.Where(r => 
                    r.DifficultyLevel == SelectedDifficulty).ToList();
            }
            
            // Apply cooking time filter
            if (!string.IsNullOrEmpty(SelectedCookingTime) && SelectedCookingTime != "Cooking time")
            {
                Debug.WriteLine($"**DIAG** FilterRecipes: Filtering by cooking time '{SelectedCookingTime}'");
                int time = int.TryParse(SelectedCookingTime, out int t) ? t : 0;
                filteredList = filteredList.Where(r => r.CookingTime == time).ToList();
            }
            
            // Apply diet tag filter
            if (!string.IsNullOrEmpty(SelectedDietTag) && SelectedDietTag != "Diet Tag")
            {
                Debug.WriteLine($"**DIAG** FilterRecipes: Filtering by diet tag '{SelectedDietTag}'");
                filteredList = filteredList.Where(r => 
                    r.DietTag == SelectedDietTag).ToList();
            }
            
            // Apply category filter
            if (!string.IsNullOrEmpty(SelectedCategory) && SelectedCategory != "Category")
            {
                Debug.WriteLine($"**DIAG** FilterRecipes: Filtering by category '{SelectedCategory}'");
                filteredList = filteredList.Where(r => 
                    r.RecipeIngredients.Any(ri => 
                        ri.Ingredient != null && ri.Ingredient.Category == SelectedCategory)).ToList();
            }
            
            Debug.WriteLine($"**DIAG** FilterRecipes: After all filters, {filteredList.Count} recipes remain");
            
            var batchUpdateStartTime = DateTime.Now;
            
            // Update the UI with filtered results
            if (Application.Current != null)
            {
                Application.Current.Dispatcher.Dispatch(() => 
                {
                    FilteredRecipes.Clear();
                    foreach (var recipe in filteredList)
                    {
                        FilteredRecipes.Add(recipe);
                    }
                });
            }
            else
            {
                // Fallback in case Application.Current is null
                FilteredRecipes.Clear();
                foreach (var recipe in filteredList)
                {
                    FilteredRecipes.Add(recipe);
                }
            }
            
            Debug.WriteLine($"**DIAG** FilterRecipes: Updated FilteredRecipes in {(DateTime.Now - batchUpdateStartTime).TotalMilliseconds:F1}ms");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"**DIAG** ERROR in FilterRecipes: {ex.Message}");
        }
        finally
        {
            Debug.WriteLine($"**DIAG** FilterRecipes: Completed at {DateTime.Now:HH:mm:ss.fff}");
        }
    }

    private async Task LoadDifficultyLevelsAsync()
    {
        Debug.WriteLine($"**DIAG** LoadDifficultyLevelsAsync: Started at {DateTime.Now:HH:mm:ss.fff}");
        var startTime = DateTime.Now;
        
        var difficultyLevels = await _recipeService.GetDifficultyLevelsAsync();
        Debug.WriteLine($"**DIAG** LoadDifficultyLevelsAsync: Retrieved {difficultyLevels.Count} levels in {(DateTime.Now - startTime).TotalMilliseconds:F1}ms");
        
        var updateStartTime = DateTime.Now;
        Difficulties.Clear();
        Difficulties.Add("Difficulty");
        foreach (var level in difficultyLevels)
        {
            Difficulties.Add(level);
        }
        SelectedDifficulty = "Difficulty";
        Debug.WriteLine($"**DIAG** LoadDifficultyLevelsAsync: Updated collection in {(DateTime.Now - updateStartTime).TotalMilliseconds:F1}ms");
        Debug.WriteLine($"**DIAG** LoadDifficultyLevelsAsync: Completed at {DateTime.Now:HH:mm:ss.fff}");
    }

    private async Task LoadCookingTimesAsync()
    {
        Debug.WriteLine($"**DIAG** LoadCookingTimesAsync: Started at {DateTime.Now:HH:mm:ss.fff}");
        var startTime = DateTime.Now;
        
        var cookingTimes = await _recipeService.GetCookingTimesAsync();
        Debug.WriteLine($"**DIAG** LoadCookingTimesAsync: Retrieved {cookingTimes.Count} times in {(DateTime.Now - startTime).TotalMilliseconds:F1}ms");
        
        var updateStartTime = DateTime.Now;
        CookingTimes.Clear();
        CookingTimes.Add("Cooking time");
        foreach (var time in cookingTimes)
        {
            CookingTimes.Add(time);
        }
        SelectedCookingTime = "Cooking time";
        Debug.WriteLine($"**DIAG** LoadCookingTimesAsync: Updated collection in {(DateTime.Now - updateStartTime).TotalMilliseconds:F1}ms");
        Debug.WriteLine($"**DIAG** LoadCookingTimesAsync: Completed at {DateTime.Now:HH:mm:ss.fff}");
    }

    private async Task LoadDietTagsAsync()
    {
        Debug.WriteLine($"**DIAG** LoadDietTagsAsync: Started at {DateTime.Now:HH:mm:ss.fff}");
        var startTime = DateTime.Now;
        
        var tags = await _recipeService.GetDietTagsAsync();
        Debug.WriteLine($"**DIAG** LoadDietTagsAsync: Retrieved {tags.Count} tags in {(DateTime.Now - startTime).TotalMilliseconds:F1}ms");
        
        var updateStartTime = DateTime.Now;
        DietTags.Clear();
        DietTags.Add("Diet Tag"); // Placeholder option
        foreach (var tag in tags)
        {
            DietTags.Add(tag);
        }
        SelectedDietTag = "Diet Tag";
        Debug.WriteLine($"**DIAG** LoadDietTagsAsync: Updated collection in {(DateTime.Now - updateStartTime).TotalMilliseconds:F1}ms");
        Debug.WriteLine($"**DIAG** LoadDietTagsAsync: Completed at {DateTime.Now:HH:mm:ss.fff}");
    }

    private async Task LoadCategoriesAsync()
    {
        Debug.WriteLine($"**DIAG** LoadCategoriesAsync: Started at {DateTime.Now:HH:mm:ss.fff}");
        var startTime = DateTime.Now;
        
        var categories = await _recipeService.GetCategoriesAsync();
        Debug.WriteLine($"**DIAG** LoadCategoriesAsync: Retrieved {categories.Count} categories in {(DateTime.Now - startTime).TotalMilliseconds:F1}ms");
        
        var updateStartTime = DateTime.Now;
        Categories.Clear();
        Categories.Add("Category");  // Placeholder option
        foreach (var category in categories)
        {
            Categories.Add(category);
        }
        SelectedCategory = "Category";
        Debug.WriteLine($"**DIAG** LoadCategoriesAsync: Updated collection in {(DateTime.Now - updateStartTime).TotalMilliseconds:F1}ms");
        Debug.WriteLine($"**DIAG** LoadCategoriesAsync: Completed at {DateTime.Now:HH:mm:ss.fff}");
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

    public bool RecipeHasAllIngredients(Recipe recipe)
    {
        var startTime = DateTime.Now;
        IEnumerable<RecipeIngredient> RecipeIngredients = recipe.RecipeIngredients;

        if (RecipeIngredients == null || !RecipeIngredients.Any())
        {
            Debug.WriteLine($"**DIAG** RecipeHasAllIngredients: Recipe {recipe.Id} has no ingredients, completed in {(DateTime.Now - startTime).TotalMilliseconds:F1}ms");
            return false;
        }

        foreach(RecipeIngredient recipeIngredient in RecipeIngredients)
        {
            bool hasIngredient = false;
            bool hasAmount = false;
            foreach(UserIngredient userIngredient in _userIngredients)
            {
                if (userIngredient.IngredientId == recipeIngredient.IngredientId)
                {
                    hasIngredient = true;
                    if (userIngredient.Amount >= recipeIngredient.Amount)
                    {
                        hasAmount = true;
                    }
                }
            }
            if (!hasIngredient || !hasAmount)
            {
                Debug.WriteLine($"**DIAG** RecipeHasAllIngredients: Recipe {recipe.Id} missing ingredients, completed in {(DateTime.Now - startTime).TotalMilliseconds:F1}ms");
                return false;
            }
        }
        Debug.WriteLine($"**DIAG** RecipeHasAllIngredients: Recipe {recipe.Id} has all ingredients, completed in {(DateTime.Now - startTime).TotalMilliseconds:F1}ms");
        return true;
    }

    public async Task LoadMetadataAsync()
    {
        Debug.WriteLine($"**DIAG** LoadMetadataAsync: Started at {DateTime.Now:HH:mm:ss.fff}");
        
        // Load each metadata type sequentially to avoid DbContext concurrency issues
        await LoadDifficultyLevelsAsync();
        await LoadCookingTimesAsync(); 
        await LoadDietTagsAsync();
        await LoadCategoriesAsync();
        
        Debug.WriteLine($"**DIAG** LoadMetadataAsync: Completed at {DateTime.Now:HH:mm:ss.fff}");
    }
}
