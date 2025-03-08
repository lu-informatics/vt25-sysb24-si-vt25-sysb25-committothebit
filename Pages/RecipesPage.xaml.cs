using Informatics.Appetite.ViewModels;

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



    protected override async void OnNavigatedTo(NavigatedToEventArgs args)
{
    base.OnNavigatedTo(args);
    var viewModel = BindingContext as RecipesViewModel;
    viewModel?.RefreshCommand.Execute(null);
}

}