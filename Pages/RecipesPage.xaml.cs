using Informatics.Appetite.ViewModels;
using System.Diagnostics;

namespace Informatics.Appetite.Pages;

public partial class RecipesPage : ContentPage
{
    public RecipesPage(RecipesViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    private void OnDifficultyIconTapped(object sender, EventArgs e)
{
    DifficultyPicker.Focus();
}
     private void OnCookingTimeIconTapped(object sender, EventArgs e)
{
   CookingTimePicker.Focus();
}

    private void OnDietTagIconTapped(object sender, EventArgs e)
{
    DietTagPicker.Focus();
}

    private void OnCategoryIconTapped(object sender, EventArgs e)
{
    CategoryPicker.Focus();
}



    protected override async void OnNavigatedTo(NavigatedToEventArgs args)
{
    base.OnNavigatedTo(args);
    Debug.WriteLine($"**DIAG** RecipesPage.OnNavigatedTo: Started at {DateTime.Now:HH:mm:ss.fff}");
    var viewModel = BindingContext as RecipesViewModel;
    
    if (viewModel != null)
    {
        // First time initialization check - load metadata only once
        if (!viewModel.IsInitialized)
        {
            Debug.WriteLine($"**DIAG** RecipesPage.OnNavigatedTo: Loading metadata at {DateTime.Now:HH:mm:ss.fff}");
            await viewModel.LoadMetadataAsync();
            viewModel.IsInitialized = true;
        }
        
        Debug.WriteLine($"**DIAG** RecipesPage.OnNavigatedTo: Calling RefreshCommand at {DateTime.Now:HH:mm:ss.fff}");
        await viewModel.RefreshCommand.ExecuteAsync(null);
        Debug.WriteLine($"**DIAG** RecipesPage.OnNavigatedTo: RefreshCommand executed at {DateTime.Now:HH:mm:ss.fff}");
    }
    else
    {
        Debug.WriteLine($"**DIAG** RecipesPage.OnNavigatedTo: ViewModel is null!");
    }
    
    Debug.WriteLine($"**DIAG** RecipesPage.OnNavigatedTo: Completed at {DateTime.Now:HH:mm:ss.fff}");
}

}