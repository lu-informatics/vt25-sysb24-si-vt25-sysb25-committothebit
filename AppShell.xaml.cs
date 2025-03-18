using Informatics.Appetite.Pages;
using Microsoft.Maui.Storage;
using Microsoft.Maui.Controls;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Informatics.Appetite.Services;
using Informatics.Appetite.Models;      // For AppUser
using Informatics.Appetite.Interfaces;  // For IAppUserService

namespace Informatics.Appetite;

public partial class AppShell : Shell
{
    private readonly bool _developerModeEnabled;
    private readonly IAppUserService _appUserService;

    public AppShell(IConfiguration config, IAppUserService appUserService)
    {
        InitializeComponent();
        _developerModeEnabled = config.GetValue<bool>("DeveloperMode", false);
        _appUserService = appUserService;

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
            // Just ensure the admin user exists
            var adminUser = await EnsureDevUserExists();
            if (adminUser != null)
            {
                // Directly set as authenticated without password check
                _appUserService.SetCurrentUser(adminUser);
                Shell.SetTabBarIsVisible(this, true);
            }
        }
        else if (!Preferences.Get("IsLoggedIn", false))
        {
            var signInPage = App.Container.GetRequiredService<SignInPage>();
            await this.Navigation.PushModalAsync(new NavigationPage(signInPage));
        }
    }

    private async Task<AppUser?> EnsureDevUserExists()
    {
        var adminUser = await _appUserService.GetUserByUsernameAsync("admin");
        if (adminUser == null)
        {
            // Create admin user with username/password both as "admin"
            adminUser = await _appUserService.CreateUserAsync("admin", "admin");
        }
        return adminUser;
    }
}
