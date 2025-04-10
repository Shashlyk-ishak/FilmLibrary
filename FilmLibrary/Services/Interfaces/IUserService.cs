using FilmLibrary.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FilmLibrary.Services.Interfaces
{
    public interface IUserService
    {
        Task<IEnumerable<User>> GetAllUsersAsync();
        Task<User> GetUserByIdAsync(string id);
        Task<bool> UpdateUserAsync(User user);
        Task<bool> DeleteUserAsync(string id);
        Task<bool> ToggleUserStatusAsync(string id);
        Task<IEnumerable<string>> GetUserRolesAsync(string userId);
    }
}