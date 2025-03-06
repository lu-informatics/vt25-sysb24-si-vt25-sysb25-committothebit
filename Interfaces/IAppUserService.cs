using Informatics.Appetite.Models;

namespace Informatics.Appetite.Interfaces;

public interface IAppUserService
{
    Task<IEnumerable<AppUser>> GetUsersAsync();
    Task<AppUser?> GetUserByIdAsync(int id);
    Task<AppUser?> GetUserByUsernameAsync(string name);
    Task<AppUser> SaveUserAsync(AppUser appUser);
    Task<bool> DeleteUserByIdAsync(int id);
    Task<bool> DeleteUserByUsernameAsync(string name);
}
