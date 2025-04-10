using System.Security.Claims;
using FilmLibrary.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FilmLibrary.Services.Interfaces
{
    public interface IAuthService
    {
        Task<bool> RegisterUserAsync(string username, string email, string password, string phoneNumber);
        Task<bool> LoginUserAsync(string email, string password);
        Task LogoutUserAsync();
        Task<User?> GetCurrentUserAsync(ClaimsPrincipal principal);
        Task<bool> IsInRoleAsync(string userId, string roleName);
        Task<bool> IsFirstUserAsync();
        Task InitializeRolesAndAdminAsync(string adminEmail);
        Task<bool> ToggleUserStatusAsync(string userId);
        Task<bool> AddUserToRoleAsync(string userId, string roleName);
        Task<bool> RemoveUserFromRoleAsync(string userId, string roleName);
        Task<IEnumerable<User>> GetAllUsersAsync();
    }
}