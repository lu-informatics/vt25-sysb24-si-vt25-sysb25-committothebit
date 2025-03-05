using Informatics.Appetite.Contexts;
using Informatics.Appetite.Interfaces;
using Informatics.Appetite.Models;
using Microsoft.EntityFrameworkCore;

namespace Informatics.Appetite.Services;

public class UserIngredientService: IUserIngredientService
{
    private readonly RecipeContext _context;

    public UserIngredientService(RecipeContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<IEnumerable<UserIngredient>> GetUserIngredientsAsync()
    {
        return await _context.UserIngredients
            .AsNoTracking()
            .Include(ui => ui.AppUser)
            .Include(ui => ui.Ingredient)
            .ToListAsync();
    }

    public async Task<IEnumerable<UserIngredient>> GetUserIngredientsByUserIdAsync(int userId)
    {
        return await _context.UserIngredients
            .AsNoTracking()
            .Include(ui => ui.AppUser)
            .Include(ui => ui.Ingredient)
            .Where(ui => ui.AppUserId == userId)
            .ToListAsync();
    }

    public async Task<IEnumerable<AppUser>> GetUsersByIngredientIdAsync(int ingredientId)
    {
        return await _context.UserIngredients
            .AsNoTracking()
            .Include(ui => ui.AppUser)
            .Where(ui => ui.IngredientId == ingredientId)
            .Select(ui => ui.AppUser)
            .OfType<AppUser>() // Filter out null values and cast to AppUser
            .ToListAsync();
    }

    public async Task<UserIngredient?> GetUserIngredientAsync(int userId, int ingredientId)
    {
        return await _context.UserIngredients
            .AsNoTracking()
            .Include(ui => ui.AppUser)
            .Include(ui => ui.Ingredient)
            .FirstOrDefaultAsync(ui => ui.AppUserId == userId && ui.IngredientId == ingredientId);
    }

    public async Task<UserIngredient> SaveUserIngredientAsync(UserIngredient userIngredient)
    {
        var existingUserIngredient = await GetUserIngredientAsync(userIngredient.AppUserId, userIngredient.IngredientId);
        if (existingUserIngredient != null)
        {
            existingUserIngredient.Amount = userIngredient.Amount;
            _context.UserIngredients.Update(existingUserIngredient);
            await _context.SaveChangesAsync();
            return existingUserIngredient;
        }

        _context.UserIngredients.Add(userIngredient);
        await _context.SaveChangesAsync();
        return userIngredient;
    }

    public async Task<bool> DeleteUserIngredientAsync(int userId, int ingredientId)
    {
        var userIngredient = await GetUserIngredientAsync(userId, ingredientId);
        if (userIngredient == null)
        {
            return false;
        }

        _context.UserIngredients.Remove(userIngredient);
        await _context.SaveChangesAsync();
        return true;
    }
}
