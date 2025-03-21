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

        // Check authentication.
        CheckAuthentication();
    }

    private async void CheckAuthentication()
    {
        await Task.Delay(100);

        if (_developerModeEnabled)
        {
            // Developer mode: automatically use the admin user.
            var adminUser = await EnsureDevUserExists();
            if (adminUser != null)
            {
                _appUserService.SetCurrentUser(adminUser);
            }
            UpdateCurrentUserDisplay();
        }
        else
        {
            var signInPage = App.Container.GetRequiredService<SignInPage>();
            signInPage.Disappearing += (s, e) =>
            {
                UpdateCurrentUserDisplay();
            };
            await this.Navigation.PushModalAsync(new NavigationPage(signInPage));
        }
    }

    private async void UpdateCurrentUserDisplay()
    {
        try
        {
            var currentUser = await _appUserService.GetCurrentUserAsync();
            CurrentUserLabel.Text = $"Signed in: {currentUser.Username}";
        }
        catch (Exception ex)
        {
            CurrentUserLabel.Text = "User not found";
        }
    }

    private async Task<AppUser?> EnsureDevUserExists()
    {
        var adminUser = await _appUserService.GetUserByUsernameAsync("admin");
        if (adminUser == null)
        {
            adminUser = await _appUserService.CreateUserAsync("admin", "admin");
        }
        return adminUser;
    }
}
