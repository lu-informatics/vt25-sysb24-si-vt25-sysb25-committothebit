using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using System.Reflection;
using Informatics.Appetite.Contexts;
using Microsoft.EntityFrameworkCore;
using Informatics.Appetite.Interfaces;
using Informatics.Appetite.Services;
using Informatics.Appetite.ViewModels;
using Informatics.Appetite.Pages;

namespace Informatics.Appetite;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        // Add appsettings.json as configuration source.
        var assembly = Assembly.GetExecutingAssembly();
        string appsettingsFileName = "Informatics.Appetite.appsettings.json";
        using (var stream = assembly.GetManifestResourceStream(appsettingsFileName))
        {
            if (stream != null)
            {
                builder.Configuration.AddJsonStream(stream);
            }
        }

        var openAiKey = builder.Configuration["AppSettings:ApiKey"];
        if (string.IsNullOrEmpty(openAiKey))
        {
            throw new Exception("OpenAI API Key is missing from appsettings.json");
        }

        builder.Services.AddDbContext<RecipeContext>(options =>
        {
            var connectionString = builder.Configuration.GetConnectionString("AppetiteDatabase");
            options.UseSqlServer(connectionString);
        });

        // Register services.
        builder.Services.AddSingleton(openAiKey);
        builder.Services.AddSingleton<IAppUserService, AppUserService>(); // Singleton for the app, since we want to store current user on there
        builder.Services.AddSingleton<IMagicRecipeGeneratorService, MagicRecipeGeneratorService>();
        builder.Services.AddScoped<IIngredientService, IngredientService>();
        builder.Services.AddScoped<IRecipeService, RecipeService>();
        builder.Services.AddScoped<IRecipeIngredientService, RecipeIngredientService>();
        builder.Services.AddScoped<IUserIngredientService, UserIngredientService>();

        // Register ViewModels.
        builder.Services.AddSingleton<IngredientsViewModel>();
        builder.Services.AddSingleton<RecipesViewModel>();
        builder.Services.AddSingleton<SignInViewModel>();
        builder.Services.AddSingleton<MagicRecipeGeneratorViewModel>();

        // Register ViewModels with transient lifetimes.
        builder.Services.AddTransient<IngredientDetailsViewModel>();
        builder.Services.AddTransient<RecipeDetailsViewModel>();

        // Register Pages.
        builder.Services.AddSingleton<AppShell>();  // Register AppShell here
        builder.Services.AddSingleton<SignInPage>();
        builder.Services.AddSingleton<IngredientsPage>();
        builder.Services.AddTransient<IngredientDetailsPage>();
        builder.Services.AddSingleton<RecipesPage>();
        builder.Services.AddTransient<RecipeDetailsPage>();
        builder.Services.AddSingleton<MagicRecipeGeneratorPage>();

#if DEBUG
		builder.Logging.AddDebug();
 #endif

        var app = builder.Build();
        // Assign the DI container to our static property.
        App.Container = app.Services;
        return app;
    }
}
