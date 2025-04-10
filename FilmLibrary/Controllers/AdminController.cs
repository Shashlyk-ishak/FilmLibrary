using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using FilmLibrary.Services.Interfaces;
using FilmLibrary.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using FilmLibrary.ViewModels;
using System.IO;
using System.Security.Claims;
using Microsoft.AspNetCore.Hosting;

namespace FilmLibrary.Controllers
{
    [Authorize(Roles = "Administrator")]
    public class AdminController : Controller
    {
        private readonly IMovieService _movieService;
        private readonly IUserService _userService;
        private readonly UserManager<User> _userManager;

        public AdminController(
            IMovieService movieService,
            IUserService userService,
            UserManager<User> userManager)
        {
            _movieService = movieService;
            _userService = userService;
            _userManager = userManager;
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> ManageMovies()
        {
            var movies = await _movieService.GetAllMoviesAsync();
            return View(movies);
        }

        public async Task<IActionResult> ManageUsers()
        {
            var users = await _userService.GetAllUsersAsync();
            return View(users);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleAdminRole(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound();
            }

            var isAdmin = await _userManager.IsInRoleAsync(user, "Administrator");
            if (isAdmin)
            {
                await _userManager.RemoveFromRoleAsync(user, "Administrator");
            }
            else
            {
                await _userManager.AddToRoleAsync(user, "Administrator");
            }

            return RedirectToAction(nameof(ManageUsers));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleUserStatus(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return BadRequest("User ID is required.");
            }

            try
            {
                var user = await _userManager.FindByIdAsync(id);
                if (user == null)
                {
                    return NotFound($"User with ID {id} not found.");
                }

                user.IsActive = !user.IsActive;
                var result = await _userManager.UpdateAsync(user);

                if (!result.Succeeded)
                {
                    return StatusCode(500, "Failed to update user status.");
                }

                return RedirectToAction(nameof(ManageUsers));
            }
            catch (Exception ex)
            {
                // Логировать ошибку
                return StatusCode(500, "An error occurred while updating user status.");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddUserToRole(string userId, string roleName)
        {
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(roleName))
            {
                return BadRequest("User ID and role name are required.");
            }

            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    return NotFound($"User with ID {userId} not found.");
                }

                var result = await _userManager.AddToRoleAsync(user, roleName);
                return Json(new { success = result.Succeeded, errors = result.Errors });
            }
            catch (Exception ex)
            {
                // Логировать ошибку
                return StatusCode(500, "An error occurred while adding user to role.");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveUserFromRole(string userId, string roleName)
        {
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(roleName))
            {
                return BadRequest("User ID and role name are required.");
            }

            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    return NotFound($"User with ID {userId} not found.");
                }

                var result = await _userManager.RemoveFromRoleAsync(user, roleName);
                return Json(new { success = result.Succeeded, errors = result.Errors });
            }
            catch (Exception ex)
            {
                // Логировать ошибку
                return StatusCode(500, "An error occurred while removing user from role.");
            }
        }
    }
}