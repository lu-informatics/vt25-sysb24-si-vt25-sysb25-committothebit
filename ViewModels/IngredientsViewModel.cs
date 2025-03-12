using System;
using Informatics.Appetite.Interfaces;
using Informatics.Appetite.Models;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using Informatics.Appetite.Pages;
using System.Diagnostics;

namespace Informatics.Appetite.ViewModels;

public partial class IngredientsViewModel : BaseViewModel
{
    private readonly IUserIngredientService _userIngredientService;
    public ObservableCollection<UserIngredient> UserIngredients { get; }
    public IngredientsViewModel(IUserIngredientService userIngredientService)
    {
        _userIngredientService = userIngredientService;
        UserIngredients = new ObservableCollection<UserIngredient>();
        Title = "My Ingredients";

        RefreshCommand = new AsyncRelayCommand(LoadUserIngredientsAsync);
        OpenIngredientDetailsCommand = new RelayCommand<int>(OpenIngredientDetails);
        OpenAddIngredientCommand = new RelayCommand(OpenAddIngredient);
        DeleteUserIngredientCommand = new AsyncRelayCommand<UserIngredient>(DeleteUserIngredientAsync);
    }

    public IAsyncRelayCommand RefreshCommand { get; }
    public IRelayCommand<int> OpenIngredientDetailsCommand { get; }
    public IRelayCommand OpenAddIngredientCommand { get; }
    public IRelayCommand<UserIngredient> DeleteUserIngredientCommand { get; }

    private async Task LoadUserIngredientsAsync()
    {
        if (IsBusy) return;

        try
        {
            IsBusy = true;
            var userIngredients = await _userIngredientService.GetUserIngredientsByUserIdAsync(1);
            UserIngredients.Clear();
            foreach(var userIngredient in userIngredients)
            {
                UserIngredients.Add(userIngredient);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading user ingredients: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
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
            UserIngredients.Remove(userIngredient);
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
