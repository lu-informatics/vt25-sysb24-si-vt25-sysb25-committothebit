using Informatics.Appetite.Contexts;
using Informatics.Appetite.Interfaces;
using Informatics.Appetite.Models;
using Microsoft.EntityFrameworkCore;

namespace Informatics.Appetite.Services;

public class UserIngredientService : IUserIngredientService
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
            // Detach any tracked instances of related entities
            if (existingUserIngredient.AppUser != null)
            {
                var trackedAppUser = _context.ChangeTracker.Entries<AppUser>()
                    .FirstOrDefault(e => e.Entity.Id == existingUserIngredient.AppUser.Id);
                if (trackedAppUser != null)
                {
                    trackedAppUser.State = EntityState.Detached;
                }
            }

            if (existingUserIngredient.Ingredient != null)
            {
                var trackedIngredient = _context.ChangeTracker.Entries<Ingredient>()
                    .FirstOrDefault(e => e.Entity.Id == existingUserIngredient.Ingredient.Id);
                if (trackedIngredient != null)
                {
                    trackedIngredient.State = EntityState.Detached;
                }
            }

            existingUserIngredient.Amount = userIngredient.Amount;
            _context.UserIngredients.Update(existingUserIngredient);
            await _context.SaveChangesAsync();
            return existingUserIngredient;
        }

        // Detach any tracked instances of related entities for the new entity
        if (userIngredient.AppUser != null)
        {
            var trackedAppUser = _context.ChangeTracker.Entries<AppUser>()
                .FirstOrDefault(e => e.Entity.Id == userIngredient.AppUser.Id);
            if (trackedAppUser != null)
            {
                trackedAppUser.State = EntityState.Detached;
            }
        }

        if (userIngredient.Ingredient != null)
        {
            var trackedIngredient = _context.ChangeTracker.Entries<Ingredient>()
                .FirstOrDefault(e => e.Entity.Id == userIngredient.Ingredient.Id);
            if (trackedIngredient != null)
            {
                trackedIngredient.State = EntityState.Detached;
            }
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

        // Check for and detach any duplicate tracked AppUser instance.
        if (userIngredient.AppUser != null)
        {
            var trackedAppUser = _context.ChangeTracker.Entries<AppUser>()
                .FirstOrDefault(e => e.Entity.Id == userIngredient.AppUser.Id);
            if (trackedAppUser != null)
            {
                trackedAppUser.State = EntityState.Detached;
            }
        }

        _context.UserIngredients.Remove(userIngredient);
        await _context.SaveChangesAsync();
        return true;
    }
}
