using System;
using Informatics.Appetite.Interfaces;
using Informatics.Appetite.Models;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using Informatics.Appetite.Pages;
using System.Diagnostics;
using Microsoft.Maui.Controls;

namespace Informatics.Appetite.ViewModels;

public partial class IngredientsViewModel : BaseViewModel
{
    private readonly IUserIngredientService _userIngredientService;
    private readonly IAppUserService _appUserService;
    public ObservableCollection<UserIngredient> UserIngredients { get; }
    
    // Add initialization tracking
    public bool IsInitialized { get; set; } = false;
    
    public IngredientsViewModel(IUserIngredientService userIngredientService, IAppUserService appUserService)
    {
        Debug.WriteLine($"**DIAG** IngredientsViewModel: Constructor started at {DateTime.Now:HH:mm:ss.fff}");
        _userIngredientService = userIngredientService;
        _appUserService = appUserService;
        UserIngredients = new ObservableCollection<UserIngredient>();
        Title = "My Ingredients";

        RefreshCommand = new AsyncRelayCommand(LoadUserIngredientsAsync);
        OpenIngredientDetailsCommand = new RelayCommand<int>(OpenIngredientDetails);
        OpenAddIngredientCommand = new RelayCommand(OpenAddIngredient);
        DeleteUserIngredientCommand = new AsyncRelayCommand<UserIngredient>(DeleteUserIngredientAsync);
        Debug.WriteLine($"**DIAG** IngredientsViewModel: Constructor completed at {DateTime.Now:HH:mm:ss.fff}");
    }

    public IAsyncRelayCommand RefreshCommand { get; }
    public IRelayCommand<int> OpenIngredientDetailsCommand { get; }
    public IRelayCommand OpenAddIngredientCommand { get; }
    public IRelayCommand<UserIngredient> DeleteUserIngredientCommand { get; }

    private async Task LoadUserIngredientsAsync()
    {
        if (IsBusy) 
        {
            Debug.WriteLine($"**DIAG** LoadUserIngredientsAsync: Already busy, skipping at {DateTime.Now:HH:mm:ss.fff}");
            return;
        }

        Debug.WriteLine($"**DIAG** LoadUserIngredientsAsync: Started at {DateTime.Now:HH:mm:ss.fff}");
        var methodStartTime = DateTime.Now;
        
        try
        {
            IsBusy = true;
            
            Debug.WriteLine("**DIAG** LoadUserIngredientsAsync: Getting current user");
            var userStartTime = DateTime.Now;
            AppUser appUser = await _appUserService.GetCurrentUserAsync();
            int appUserId = appUser.Id;
            Debug.WriteLine($"**DIAG** LoadUserIngredientsAsync: Got user with ID {appUserId} in {(DateTime.Now - userStartTime).TotalMilliseconds:F1}ms");
            
            Debug.WriteLine("**DIAG** LoadUserIngredientsAsync: Getting user ingredients");
            var ingredientsStartTime = DateTime.Now;
            var userIngredients = await _userIngredientService.GetUserIngredientsByUserIdAsync(appUserId);
            Debug.WriteLine($"**DIAG** LoadUserIngredientsAsync: Got {userIngredients?.Count() ?? 0} user ingredients in {(DateTime.Now - ingredientsStartTime).TotalMilliseconds:F1}ms");
            
            // Prepare data before updating the UI
            var tempIngredients = userIngredients.ToList();
            Debug.WriteLine($"**DIAG** LoadUserIngredientsAsync: Prepared temp list with {tempIngredients.Count} items");
            
            // Use dispatcher to batch update UI
            var uiUpdateStartTime = DateTime.Now;
            if (Application.Current != null)
            {
                Application.Current.Dispatcher.Dispatch(() => 
                {
                    UserIngredients.Clear();
                    foreach (var ingredient in tempIngredients)
                    {
                        UserIngredients.Add(ingredient);
                    }
                });
            }
            else
            {
                // Fallback if Application.Current is null
                UserIngredients.Clear();
                foreach (var ingredient in tempIngredients)
                {
                    UserIngredients.Add(ingredient);
                }
            }
            Debug.WriteLine($"**DIAG** LoadUserIngredientsAsync: Updated UI collection in {(DateTime.Now - uiUpdateStartTime).TotalMilliseconds:F1}ms");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"**DIAG** ERROR loading user ingredients: {ex.Message}");
            Debug.WriteLine($"**DIAG** Stack trace: {ex.StackTrace}");
        }
        finally
        {
            IsBusy = false;
            Debug.WriteLine($"**DIAG** LoadUserIngredientsAsync: Completed at {DateTime.Now:HH:mm:ss.fff}, total time: {(DateTime.Now - methodStartTime).TotalMilliseconds:F1}ms");
        }
    }

    private void OpenAddIngredient() => OpenIngredientDetails(-1);
    
    private async void OpenIngredientDetails(int id)
    {
        await Shell.Current.GoToAsync($"{nameof(IngredientDetailsPage)}?ingredientId={id}");
    }

    private async Task DeleteUserIngredientAsync(UserIngredient userIngredient)
    {
        Debug.WriteLine($"[DeleteUserIngredientAsync] Clicked for ingredient: Id={userIngredient.IngredientId}, Name={userIngredient.Ingredient?.Name}");
        try
        {
            await _userIngredientService.DeleteUserIngredientAsync(userIngredient.AppUserId, userIngredient.IngredientId);
            
            // Use dispatcher for UI updates here too
            if (Application.Current != null)
            {
                Application.Current.Dispatcher.Dispatch(() => 
                {
                    UserIngredients.Remove(userIngredient);
                });
            }
            else
            {
                UserIngredients.Remove(userIngredient);
            }
            
            Debug.WriteLine($"[DeleteUserIngredientAsync] Successfully removed ingredient: Id={userIngredient.IngredientId}");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[DeleteUserIngredientAsync] Exception: {ex.Message}");
        }
        finally
        {
            Debug.WriteLine("[DeleteUserIngredientAsync] Completed.");
        }
    }
}
