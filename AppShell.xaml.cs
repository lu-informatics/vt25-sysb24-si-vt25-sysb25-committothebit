using Informatics.Appetite.Pages;
using Microsoft.Maui.Storage;
using Microsoft.Maui.Controls;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace Informatics.Appetite;

public partial class AppShell : Shell
{
    private readonly bool _developerModeEnabled;

    public AppShell(IConfiguration config)
    {
        InitializeComponent();
        _developerModeEnabled = config.GetValue<bool>("DeveloperMode", false);

        // Register routes.
        Routing.RegisterRoute(nameof(SignInPage), typeof(SignInPage));
        Routing.RegisterRoute(nameof(RecipeDetailsPage), typeof(RecipeDetailsPage));
        Routing.RegisterRoute(nameof(IngredientDetailsPage), typeof(IngredientDetailsPage));

        // Initially hide the TabBar.
        Shell.SetTabBarIsVisible(this, false);

        // Check authentication.
        CheckAuthentication();
    }

    private async void CheckAuthentication()
    {
        await Task.Delay(100); // Ensures Shell is fully loaded before navigation


        if (_developerModeEnabled)
        {
            // Resolve the SignInPage from the DI container.
            var signInPage = App.Container.GetRequiredService<SignInPage>();
            await this.Navigation.PushModalAsync(new NavigationPage(signInPage));
        }
    }

}
