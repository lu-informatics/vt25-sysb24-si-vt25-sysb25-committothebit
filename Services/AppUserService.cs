using System.Security.Cryptography;
using System.Text;
using Informatics.Appetite.Contexts;
using Informatics.Appetite.Interfaces;
using Informatics.Appetite.Models;
using Microsoft.EntityFrameworkCore;

namespace Informatics.Appetite.Services
{
    public class AppUserService : IAppUserService
    {
        private readonly RecipeContext _context;
        private AppUser? _currentUser;
        private int _currentUserId;
        private bool _isAuthenticated;

        public AppUserService(RecipeContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<AppUser> GetCurrentUserAsync()
        {
            if (_currentUser == null)
            {
                // Just fetch the existing admin user - don't try to create it again
                var adminUser = await _context.AppUsers
                    .FirstOrDefaultAsync(u => u.Username == "admin");
                
                if (adminUser != null)
                {
                    // Store the user in memory
                    _currentUser = adminUser;
                    _currentUserId = adminUser.Id;
                    _isAuthenticated = true;
                }
                else
                {
                    Console.WriteLine("Warning: Admin user not found in database. This should be created by AppShell.");
                    throw new InvalidOperationException("Admin user not found. Ensure it's created at startup.");
                }
            }
            
            return _currentUser;
        }

        public AppUser GetCurrentUser()
        {
            if (_currentUser == null)
            {
                throw new InvalidOperationException("Current user not initialized. Call GetCurrentUserAsync first.");
            }
            return _currentUser;
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

        public async Task<AppUser?> AuthenticateUserAsync(string username, string password)
        {
            var user = await _context.AppUsers.FirstOrDefaultAsync(u => u.Username == username);
            if (user == null)
                return null;

            var hashedInput = AppUser.HashPassword(password, user.Salt);
            if (user.PasswordHash.SequenceEqual(hashedInput))
            {
                _currentUser = user;
                _currentUserId = user.Id;
                _isAuthenticated = true;
                return user;
            }
            return null;
        }

        // Create a new user, storing a salted and hashed password.
        public async Task<AppUser?> CreateUserAsync(string username, string password)
        {
            var existingUser = await GetUserByUsernameAsync(username);
            if (existingUser != null)
            {
                return null;
            }

            var salt = AppUser.GenerateSalt();
            var hash = AppUser.HashPassword(password, salt);

            var user = new AppUser
            {
                Username = username,
                Salt = salt,
                PasswordHash = hash
            };

            await SaveUserAsync(user);
            return user;
        }

        public bool IsAuthenticated() => _isAuthenticated;
        
        public void SignOut()
        {
            _currentUser = null;
            _currentUserId = 0;
            _isAuthenticated = false;
        }

        public void SetCurrentUser(AppUser user)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));
            
            _currentUser = user;
            _currentUserId = user.Id;
            _isAuthenticated = true;
        }
    }
}
