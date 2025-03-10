using System;
using Informatics.Appetite.Interfaces;
using Informatics.Appetite.Models;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using Informatics.Appetite.Pages;

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
    public IAsyncRelayCommand<UserIngredient> DeleteUserIngredientCommand { get; }

    private async Task LoadUserIngredientsAsync()
    {

        try
        {
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
    }

    private void OpenAddIngredient() => OpenIngredientDetails(-1);
    
    private async void OpenIngredientDetails(int id)
    {
        await Shell.Current.GoToAsync($"{nameof(IngredientDetailsPage)}?ingredientId={id}");
    }

    private async Task DeleteUserIngredientAsync(UserIngredient userIngredient)
    {
        try
        {
            await _userIngredientService.DeleteUserIngredientAsync(userIngredient.AppUserId, userIngredient.IngredientId);
            UserIngredients.Remove(userIngredient);

            // Force UI update
            await Task.Delay(100);
            OnPropertyChanged(nameof(UserIngredients));
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error deleting ingredient: {ex.Message}");
        }
    }

}
