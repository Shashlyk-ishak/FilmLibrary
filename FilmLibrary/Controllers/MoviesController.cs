using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using FilmLibrary.Services;
using FilmLibrary.Models;
using FilmLibrary.ViewModels;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using System.IO;
using System.Security.Claims;
using Microsoft.Extensions.Logging;
using System.Linq;
using System;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;
using FilmLibrary.Services.Interfaces;

namespace FilmLibrary.Controllers
{
    public class MoviesController : Controller
    {
        private readonly IMovieService _movieService;
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<MoviesController> _logger;
        private readonly UserManager<User> _userManager;

        public MoviesController(
            IMovieService movieService,
            IWebHostEnvironment environment,
            ILogger<MoviesController> logger,
            UserManager<User> userManager)
        {
            _movieService = movieService ?? throw new ArgumentNullException(nameof(movieService));
            _environment = environment ?? throw new ArgumentNullException(nameof(environment));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                _logger.LogInformation("Fetching all movies");
                var movies = await _movieService.GetAllMoviesAsync();
                if (movies == null)
                {
                    _logger.LogWarning("No movies found");
                    return View(Enumerable.Empty<Movie>());
                }
                return View(movies);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching movies: {Message}", ex.Message);
                return View(Enumerable.Empty<Movie>());
            }
        }

        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var movie = await _movieService.GetMovieByIdAsync(id);
                if (movie == null)
                {
                    _logger.LogWarning("Movie not found: {MovieId}", id);
                    return NotFound();
                }

                var ratings = await _movieService.GetMovieRatingsAsync(id);
                var averageRating = await _movieService.GetAverageRatingAsync(id);

                var userId = _userManager.GetUserId(User);
                var userRating = userId != null ? await _movieService.GetUserRatingAsync(id, userId) : null;

                var viewModel = new MovieDetailsViewModel
                {
                    Movie = movie,
                    AverageRating = averageRating,
                    UserRating = userRating,
                    Ratings = ratings ?? Enumerable.Empty<UserRating>(),
                    MovieActors = await _movieService.GetMovieActorsAsync(id) ?? Enumerable.Empty<MovieActor>(),
                    MovieDirectors = await _movieService.GetMovieDirectorsAsync(id) ?? Enumerable.Empty<MovieDirector>()
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching movie details: {MovieId}", id);
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpGet]
        [Authorize(Roles = "Administrator")]
        public IActionResult Create()
        {
            var viewModel = new MovieViewModel
            {
                Title = "",
                Description = "",
                Genre = "",
                ReleaseYear = DateTime.Now.Year
            };
            return View(viewModel);
        }

        [HttpPost]
        [Authorize(Roles = "Administrator")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(MovieViewModel viewModel, IFormFile CoverImage)
        {
            try
            {
                _logger.LogInformation("Starting movie creation process with data: {@MovieData}",
                    new { viewModel.Title, viewModel.Description, viewModel.Genre, viewModel.ReleaseYear });

                if (viewModel == null)
                {
                    _logger.LogError("MovieViewModel object is null");
                    return BadRequest("Movie object is null");
                }

                // Получение идентификатора пользователя из утверждений
                var userId = _userManager.GetUserId(User);
                if (userId == null)
                {
                    _logger.LogError("User ID not found");
                    ModelState.AddModelError("", "Unable to determine user identity");
                    return View(viewModel);
                }

                // Проверка изображения плаката
                if (CoverImage == null || CoverImage.Length == 0)
                {
                    _logger.LogError("No cover image provided or image is empty");
                    ModelState.AddModelError("CoverImage", "Please select a movie cover image");
                    return View(viewModel);
                }

                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("ModelState is invalid: {@ModelStateErrors}",
                        string.Join(", ", ModelState.Values
                            .SelectMany(v => v.Errors)
                            .Select(e => e.ErrorMessage)));
                    return View(viewModel);
                }

                // Обработка изображения обложки
                var fileName = $"{Guid.NewGuid()}{Path.GetExtension(CoverImage.FileName)}";
                var uploadsFolder = Path.Combine(_environment.WebRootPath, "images", "covers");

                _logger.LogInformation("Saving image to uploads folder: {UploadsFolder}", uploadsFolder);

                if (!Directory.Exists(uploadsFolder))
                {
                    _logger.LogInformation("Creating uploads folder: {UploadsFolder}", uploadsFolder);
                    Directory.CreateDirectory(uploadsFolder);
                }

                var filePath = Path.Combine(uploadsFolder, fileName);
                _logger.LogInformation("Saving file to: {FilePath}", filePath);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await CoverImage.CopyToAsync(stream);
                }

                viewModel.CoverImage = CoverImage;
                viewModel.ExistingCoverImagePath = $"/images/covers/{fileName}";

                _logger.LogInformation("Creating movie with cover image path: {CoverImagePath}", viewModel.ExistingCoverImagePath);

                // Создать фильм
                var movie = await _movieService.CreateMovieAsync(viewModel, userId);
                _logger.LogInformation("Movie created successfully: {MovieId} - {Title}", movie.MovieId, viewModel.Title);

                TempData["SuccessMessage"] = "Movie added successfully!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating movie: {Message}", ex.Message);
                ModelState.AddModelError("", "An error occurred while creating the movie. Please try again.");
                return View(viewModel);
            }
        }

        [HttpGet]
        [Route("Edit/{id}")]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                _logger.LogInformation("Fetching movie for edit: {MovieId}", id);
                var movie = await _movieService.GetMovieByIdAsync(id);
                if (movie == null)
                {
                    _logger.LogWarning("Movie not found: {MovieId}", id);
                    return NotFound();
                }

                var viewModel = new MovieViewModel
                {
                    MovieId = movie.MovieId,
                    Title = movie.Title,
                    Description = movie.Description,
                    Genre = movie.Genre,
                    ReleaseYear = movie.ReleaseYear,
                    Rating = (decimal)movie.Rating,
                    TrailerUrl = movie.TrailerUrl,
                    ExistingCoverImagePath = movie.CoverImagePath
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching movie for edit: {MovieId}", id);
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [Route("Edit/{id}")]
        [Authorize(Roles = "Administrator")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, MovieViewModel viewModel)
        {
            if (id != viewModel.MovieId)
            {
                _logger.LogWarning("ID mismatch in Edit action. URL ID: {UrlId}, Model ID: {ModelId}", id, viewModel.MovieId);
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid model state in Edit action: {@ModelStateErrors}",
                    string.Join(", ", ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)));
                return View(viewModel);
            }

            try
            {
                var userId = _userManager.GetUserId(User);
                if (userId == null)
                {
                    _logger.LogError("User ID not found in Edit action");
                    ModelState.AddModelError("", "Unable to determine user identity");
                    return View(viewModel);
                }

                if (viewModel.CoverImage != null && viewModel.CoverImage.Length > 0)
                {
                    _logger.LogInformation("Processing new cover image for movie: {MovieId}", id);

                    // Удалить старую обложку если она есть
                    if (!string.IsNullOrEmpty(viewModel.ExistingCoverImagePath))
                    {
                        var oldFilePath = Path.Combine(_environment.WebRootPath, viewModel.ExistingCoverImagePath.TrimStart('/'));
                        if (System.IO.File.Exists(oldFilePath))
                        {
                            _logger.LogInformation("Deleting old cover image: {FilePath}", oldFilePath);
                            System.IO.File.Delete(oldFilePath);
                        }
                    }

                    // Создайте уникальное имя файла и сохраните новое изображение
                    var fileName = $"{Guid.NewGuid()}{Path.GetExtension(viewModel.CoverImage.FileName)}";
                    var uploadsFolder = Path.Combine(_environment.WebRootPath, "images", "covers");

                    if (!Directory.Exists(uploadsFolder))
                    {
                        _logger.LogInformation("Creating uploads folder: {UploadsFolder}", uploadsFolder);
                        Directory.CreateDirectory(uploadsFolder);
                    }

                    var filePath = Path.Combine(uploadsFolder, fileName);
                    _logger.LogInformation("Saving new cover image to: {FilePath}", filePath);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await viewModel.CoverImage.CopyToAsync(stream);
                    }

                    viewModel.ExistingCoverImagePath = $"/images/covers/{fileName}";
                }

                _logger.LogInformation("Updating movie: {MovieId}", id);
                await _movieService.UpdateMovieAsync(viewModel, userId);

                _logger.LogInformation("Movie updated successfully: {MovieId}", id);
                TempData["SuccessMessage"] = "Movie updated successfully!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating movie: {MovieId}", id);
                ModelState.AddModelError("", "An error occurred while updating the movie. Please try again.");
                return View(viewModel);
            }
        }

        [HttpGet]
        [Route("Delete/{id}")]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                _logger.LogInformation("Fetching movie for delete confirmation: {MovieId}", id);
                var movie = await _movieService.GetMovieByIdAsync(id);
                if (movie == null)
                {
                    _logger.LogWarning("Movie not found: {MovieId}", id);
                    return NotFound();
                }

                return View(movie);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching movie for delete: {MovieId}", id);
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [Route("Delete/{id}")]
        [Authorize(Roles = "Administrator")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                _logger.LogInformation("Deleting movie: {MovieId}", id);
                var movie = await _movieService.GetMovieByIdAsync(id);
                if (movie != null)
                {
                    // Удалить файл изображения, если он существует
                    if (!string.IsNullOrEmpty(movie.CoverImagePath))
                    {
                        var filePath = Path.Combine(_environment.WebRootPath, movie.CoverImagePath.TrimStart('/'));
                        if (System.IO.File.Exists(filePath))
                        {
                            _logger.LogInformation("Deleting cover image: {FilePath}", filePath);
                            System.IO.File.Delete(filePath);
                        }
                    }

                    await _movieService.DeleteMovieAsync(id);
                    _logger.LogInformation("Movie deleted successfully: {MovieId}", id);
                    TempData["SuccessMessage"] = "Movie deleted successfully!";
                }
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting movie: {MovieId}", id);
                TempData["ErrorMessage"] = "An error occurred while deleting the movie.";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Rate(int movieId, int rating)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Json(new { success = false, message = "You must be logged in to rate movies." });
            }

            try
            {
                var success = await _movieService.AddOrUpdateRatingAsync(movieId, userId, rating);
                if (success)
                {
                    var averageRating = await _movieService.GetAverageRatingAsync(movieId);
                    return Json(new { success = true, averageRating = averageRating });
                }
                return Json(new { success = false, message = "Failed to save your rating." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving rating for movie {MovieId} by user {UserId}", movieId, userId);
                return Json(new { success = false, message = "An error occurred while saving your rating." });
            }
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> AddComment(int id, string comment)
        {
            try
            {
                var userId = _userManager.GetUserId(User);
                if (string.IsNullOrEmpty(userId))
                {
                    TempData["ErrorMessage"] = "User not found";
                    return RedirectToAction(nameof(Details), new { id });
                }

                await _movieService.AddOrUpdateCommentAsync(id, userId, comment);
                TempData["SuccessMessage"] = "Review submitted successfully!";
                return RedirectToAction(nameof(Details), new { id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding comment to movie: {MovieId}", id);
                TempData["ErrorMessage"] = "An error occurred while submitting your review";
                return RedirectToAction(nameof(Details), new { id });
            }
        }
    }
}