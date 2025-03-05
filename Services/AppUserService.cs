using Informatics.Appetite.Contexts;
using Informatics.Appetite.Interfaces;
using Informatics.Appetite.Models;
using Microsoft.EntityFrameworkCore;

namespace Informatics.Appetite.Services;

public class AppUserService : IAppUserService
{
    private readonly RecipeContext _context;

    public AppUserService(RecipeContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<IEnumerable<AppUser>> GetUsersAsync()
    {
        return await _context.AppUsers
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<AppUser?> GetUserByIdAsync(int id)
    {
        return await _context.AppUsers
            .AsNoTracking()
            .FirstOrDefaultAsync(i => i.Id == id);
    }

    public async Task<AppUser?> GetUserByUsernameAsync(string name)
    {
        return await _context.AppUsers
            .AsNoTracking()
            .FirstOrDefaultAsync(i => i.Username == name);
    }

    public async Task<AppUser> SaveUserAsync(AppUser user)
    {
        if (user.Id == 0)
        {
            _context.AppUsers.Add(user);
        }
        else
        {
            _context.AppUsers.Update(user);
        }

        await _context.SaveChangesAsync();
        return user;
    }

    public async Task<bool> DeleteUserByIdAsync(int id)
    {
        var user = await GetUserByIdAsync(id);
        if (user == null)
        {
            return false;
        }

        _context.AppUsers.Remove(user);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteUserByUsernameAsync(string name)
    {
        var user = await GetUserByUsernameAsync(name);
        if (user == null)
        {
            return false;
        }

        _context.AppUsers.Remove(user);
        await _context.SaveChangesAsync();
        return true;
    }
}
