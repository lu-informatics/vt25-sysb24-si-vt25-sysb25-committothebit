using Informatics.Appetite.ViewModels;
using Microsoft.Maui.Controls;
using System.Threading.Tasks;
using System.Diagnostics;

namespace Informatics.Appetite.Pages;

[QueryProperty(nameof(RecipeId), "recipeId")]
public partial class RecipeDetailsPage : ContentPage
{
    private readonly RecipeDetailsViewModel _viewModel;

    private int _recipeId;
    public int RecipeId
    {
        get => _recipeId;
        set
        {
            Debug.WriteLine($"**DIAG** RecipeDetailsPage: RecipeId property set to {value} at {DateTime.Now:HH:mm:ss.fff}");
            _recipeId = value;
            // Don't load immediately, wait for OnNavigatedTo for better performance
        }
    }

    public RecipeDetailsPage(RecipeDetailsViewModel viewModel)
    {
        Debug.WriteLine($"**DIAG** RecipeDetailsPage: Constructor called at {DateTime.Now:HH:mm:ss.fff}");
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
    }

    protected override async void OnNavigatedTo(NavigatedToEventArgs args)
    {
        Debug.WriteLine($"**DIAG** RecipeDetailsPage.OnNavigatedTo: Started at {DateTime.Now:HH:mm:ss.fff}");
        base.OnNavigatedTo(args);

        if (_recipeId > 0)
        {
            Debug.WriteLine($"**DIAG** RecipeDetailsPage.OnNavigatedTo: Loading recipe {_recipeId}");
            await LoadRecipeAsync(_recipeId);
            Debug.WriteLine($"**DIAG** RecipeDetailsPage.OnNavigatedTo: Finished loading recipe {_recipeId}");
        }
        
        Debug.WriteLine($"**DIAG** RecipeDetailsPage.OnNavigatedTo: Completed at {DateTime.Now:HH:mm:ss.fff}");
    }

    private async Task LoadRecipeAsync(int recipeId)
    {
        var startTime = DateTime.Now;
        Debug.WriteLine($"**DIAG** LoadRecipeAsync: Started for recipeId={recipeId} at {DateTime.Now:HH:mm:ss.fff}");
        
        if (_viewModel != null)
        {
            await _viewModel.LoadRecipeAsync(recipeId);
        }
        else
        {
            Debug.WriteLine($"**DIAG** ERROR: ViewModel is null in LoadRecipeAsync");
        }
        
        Debug.WriteLine($"**DIAG** LoadRecipeAsync: Completed in {(DateTime.Now - startTime).TotalMilliseconds:F1}ms");
    }
}