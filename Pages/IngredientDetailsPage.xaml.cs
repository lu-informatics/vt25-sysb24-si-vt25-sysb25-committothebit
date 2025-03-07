using Informatics.Appetite.ViewModels;

namespace Informatics.Appetite.Pages;

public partial class IngredientDetailsPage : ContentPage
{
	public IngredientDetailsPage(IngredientDetailsViewModel viewModel)
	{
		InitializeComponent();
		BindingContext = viewModel;
	}

    protected override async void OnNavigatedTo(NavigatedToEventArgs args)
    {
        base.OnNavigatedTo(args);
        var viewModel = BindingContext as IngredientDetailsViewModel;
        viewModel?.RefreshCommand.Execute(null);
    }
}