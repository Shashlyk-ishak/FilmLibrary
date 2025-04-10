using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using FilmLibrary.Services.Interfaces;
using System.Threading.Tasks;

namespace FilmLibrary.Controllers
{
    [Authorize]
    public class ProfileController : Controller
    {
        private readonly IAuthService _authService;

        public ProfileController(IAuthService authService)
        {
            _authService = authService;
        }

        public async Task<IActionResult> Index()
        {
            var user = await _authService.GetCurrentUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }
            return View("~/Views/Account/Profile.cshtml", user);
        }
    }
}