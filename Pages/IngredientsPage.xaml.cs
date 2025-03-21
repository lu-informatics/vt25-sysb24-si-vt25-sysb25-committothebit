using Informatics.Appetite.ViewModels;
using System.Diagnostics;

namespace Informatics.Appetite.Pages;

public partial class IngredientsPage : ContentPage
{
	public IngredientsPage(IngredientsViewModel viewModel)
	{
		InitializeComponent();
		BindingContext = viewModel;
	}

    protected override async void OnNavigatedTo(NavigatedToEventArgs args)
    {
        Debug.WriteLine($"**DIAG** IngredientsPage.OnNavigatedTo: Started at {DateTime.Now:HH:mm:ss.fff}");
        base.OnNavigatedTo(args);
        var viewModel = BindingContext as IngredientsViewModel;
        
        if (viewModel != null)
        {
            Debug.WriteLine($"**DIAG** IngredientsPage.OnNavigatedTo: Calling RefreshCommand at {DateTime.Now:HH:mm:ss.fff}");
            await viewModel.RefreshCommand.ExecuteAsync(null);
            Debug.WriteLine($"**DIAG** IngredientsPage.OnNavigatedTo: RefreshCommand executed at {DateTime.Now:HH:mm:ss.fff}");
        }
        else
        {
            Debug.WriteLine($"**DIAG** IngredientsPage.OnNavigatedTo: ViewModel is null!");
        }
        
        Debug.WriteLine($"**DIAG** IngredientsPage.OnNavigatedTo: Completed at {DateTime.Now:HH:mm:ss.fff}");
    }
}