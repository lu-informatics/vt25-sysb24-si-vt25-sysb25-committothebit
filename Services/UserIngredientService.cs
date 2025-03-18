using Informatics.Appetite.Contexts;
using Informatics.Appetite.Interfaces;
using Informatics.Appetite.Models;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

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
        try
        {
            return await _context.UserIngredients
                .Include(ui => ui.Ingredient)  // Include the Ingredient navigation property
                .Where(ui => ui.AppUserId == userId)
                .AsNoTracking()  // Add this to prevent tracking issues
                .ToListAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in GetUserIngredientsByUserIdAsync: {ex.Message}");
            throw;
        }
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
        Debug.WriteLine($"GetUserIngredientAsync: Looking for User={userId}, Ingredient={ingredientId}");
        
        var result = await _context.UserIngredients
            .AsNoTracking()
            .Include(ui => ui.AppUser)
            .Include(ui => ui.Ingredient)
            .FirstOrDefaultAsync(ui => ui.AppUserId == userId && ui.IngredientId == ingredientId);
        
        Debug.WriteLine($"GetUserIngredientAsync: Found? {result != null}");
        
        return result;
    }

    public async Task<UserIngredient> SaveUserIngredientAsync(UserIngredient userIngredient)
    {
        Debug.WriteLine($"SaveUserIngredientAsync: Starting with User={userIngredient.AppUserId}, Ingredient={userIngredient.IngredientId}");
        
        var existingUserIngredient = await GetUserIngredientAsync(userIngredient.AppUserId, userIngredient.IngredientId);
        Debug.WriteLine($"SaveUserIngredientAsync: existingUserIngredient found? {existingUserIngredient != null}");
        
        try
        {
            if (existingUserIngredient != null)
            {
                // Update existing ingredient
                Debug.WriteLine($"SaveUserIngredientAsync: Updating existing entry, old amount: {existingUserIngredient.Amount}, new amount: {userIngredient.Amount}");
                
                // Detach the current entity from tracking
                _context.Entry(existingUserIngredient).State = EntityState.Detached;
                
                // Update properties on the entity we're saving
                _context.UserIngredients.Attach(userIngredient);
                _context.Entry(userIngredient).State = EntityState.Modified;
            }
            else
            {
                // Adding new ingredient
                Debug.WriteLine($"SaveUserIngredientAsync: Adding new entry");
                
                // Check if the entity is being tracked already
                var trackedEntity = _context.UserIngredients.Local.FirstOrDefault(
                    e => e.AppUserId == userIngredient.AppUserId && e.IngredientId == userIngredient.IngredientId);
                
                if (trackedEntity != null)
                {
                    Debug.WriteLine($"SaveUserIngredientAsync: Found tracked entity, detaching it");
                    _context.Entry(trackedEntity).State = EntityState.Detached;
                }
                
                _context.UserIngredients.Add(userIngredient);
            }
            
            await _context.SaveChangesAsync();
            Debug.WriteLine($"SaveUserIngredientAsync: Changes saved to database");

            // Get fresh copy from database to ensure all related entities are loaded
            return await GetUserIngredientAsync(userIngredient.AppUserId, userIngredient.IngredientId);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"SaveUserIngredientAsync ERROR: {ex.Message}");
            throw;
        }
    }

    public async Task<bool> DeleteUserIngredientAsync(int userId, int ingredientId)
    {
        Debug.WriteLine($"DeleteUserIngredientAsync: Starting with User={userId}, Ingredient={ingredientId}");
        
        var userIngredient = await GetUserIngredientAsync(userId, ingredientId);
        Debug.WriteLine($"DeleteUserIngredientAsync: userIngredient found? {userIngredient != null}");
        
        if (userIngredient == null)
        {
            Debug.WriteLine($"DeleteUserIngredientAsync: Nothing to delete, returning");
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

        // Check for and detach any duplicate tracked Ingredient instance.
        if (userIngredient.Ingredient != null)
        {
            var trackedIngredient = _context.ChangeTracker.Entries<Ingredient>()
                .FirstOrDefault(e => e.Entity.Id == userIngredient.Ingredient.Id);
            if (trackedIngredient != null)
            {
                trackedIngredient.State = EntityState.Detached;
            }
        }

        _context.UserIngredients.Remove(userIngredient);
        Debug.WriteLine($"DeleteUserIngredientAsync: Removing from context");
        
        await _context.SaveChangesAsync();
        Debug.WriteLine($"DeleteUserIngredientAsync: Changes saved to database");

        // Add this line to see the entity state
        Debug.WriteLine($"DeleteUserIngredientAsync: Entity state after deletion: {_context.Entry(userIngredient).State}");

        return true;
    }
}
