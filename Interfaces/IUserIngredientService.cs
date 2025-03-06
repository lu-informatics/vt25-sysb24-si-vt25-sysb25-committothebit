using Informatics.Appetite.Models;

namespace Informatics.Appetite.Interfaces;

public interface IUserIngredientService
{
    Task<IEnumerable<UserIngredient>> GetUserIngredientsAsync();
    Task<IEnumerable<UserIngredient>> GetUserIngredientsByUserIdAsync(int userId);
    Task<IEnumerable<AppUser>> GetUsersByIngredientIdAsync(int ingredientId);
    Task<UserIngredient?> GetUserIngredientAsync(int userId, int ingredientId);
    Task<UserIngredient> SaveUserIngredientAsync(UserIngredient userIngredient);
    Task<bool> DeleteUserIngredientAsync(int userId, int ingredientId);
}
