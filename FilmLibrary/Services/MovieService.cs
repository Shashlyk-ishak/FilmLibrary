using FilmLibrary.Data;
using FilmLibrary.Models;
using FilmLibrary.Services.Interfaces;
using FilmLibrary.ViewModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Hosting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace FilmLibrary.Services
{
    public class MovieService : IMovieService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<MovieService> _logger;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public MovieService(
            ApplicationDbContext context,
            ILogger<MovieService> logger,
            IWebHostEnvironment webHostEnvironment)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _webHostEnvironment = webHostEnvironment ?? throw new ArgumentNullException(nameof(webHostEnvironment));
        }

        public async Task<IEnumerable<Movie>> GetAllMoviesAsync()
        {
            return await _context.Movies
                .Include(m => m.Creator)
                .Include(m => m.MovieActors)
                    .ThenInclude(ma => ma.Actor)
                .Include(m => m.MovieDirectors)
                    .ThenInclude(md => md.Director)
                .Include(m => m.UserRatings)
                .OrderByDescending(m => m.CreatedAt)
                .ToListAsync() ?? Enumerable.Empty<Movie>();
        }

        public async Task<Movie?> GetMovieByIdAsync(int id)
        {
            return await _context.Movies
                .Include(m => m.Creator)
                .Include(m => m.MovieActors)
                    .ThenInclude(ma => ma.Actor)
                .Include(m => m.MovieDirectors)
                    .ThenInclude(md => md.Director)
                .Include(m => m.UserRatings)
                    .ThenInclude(ur => ur.User)
                .FirstOrDefaultAsync(m => m.MovieId == id);
        }

        public async Task<Movie> CreateMovieAsync(MovieViewModel model, string userId)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var movie = new Movie
                {
                    Title = model.Title,
                    Description = model.Description,
                    Genre = model.Genre,
                    ReleaseYear = model.ReleaseYear,
                    Rating = (double)model.Rating,
                    TrailerUrl = model.TrailerUrl,
                    CreatedBy = userId,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                if (model.CoverImage != null)
                {
                    string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images", "covers");
                    string uniqueFileName = Guid.NewGuid().ToString() + "_" + model.CoverImage.FileName;
                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    Directory.CreateDirectory(uploadsFolder);
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await model.CoverImage.CopyToAsync(fileStream);
                    }

                    movie.CoverImagePath = "/images/covers/" + uniqueFileName;
                }

                _context.Movies.Add(movie);
                await _context.SaveChangesAsync();

                // Добавить актера
                for (int i = 0; i < model.ActorNames.Count; i++)
                {
                    var actorName = model.ActorNames[i];
                    var role = model.ActorRoles[i];

                    if (!string.IsNullOrWhiteSpace(actorName))
                    {
                        var actor = await _context.Actors.FirstOrDefaultAsync(a => a.Name == actorName)
                            ?? new Actor { Name = actorName };

                        if (actor.ActorId == 0)
                        {
                            _context.Actors.Add(actor);
                            await _context.SaveChangesAsync();
                        }

                        var movieActor = new MovieActor
                        {
                            Movie = movie,
                            MovieId = movie.MovieId,
                            Actor = actor,
                            ActorId = actor.ActorId,
                            Role = role
                        };

                        _context.MovieActors.Add(movieActor);
                    }
                }

                // Добавить режиссера
                foreach (var directorName in model.DirectorNames)
                {
                    if (!string.IsNullOrWhiteSpace(directorName))
                    {
                        var director = await _context.Directors.FirstOrDefaultAsync(d => d.Name == directorName)
                            ?? new Director { Name = directorName };

                        if (director.DirectorId == 0)
                        {
                            _context.Directors.Add(director);
                            await _context.SaveChangesAsync();
                        }

                        var movieDirector = new MovieDirector
                        {
                            Movie = movie,
                            MovieId = movie.MovieId,
                            Director = director,
                            DirectorId = director.DirectorId
                        };

                        _context.MovieDirectors.Add(movieDirector);
                    }
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return movie;
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<Movie> UpdateMovieAsync(MovieViewModel model, string userId)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var movie = await _context.Movies
                    .Include(m => m.MovieActors)
                    .Include(m => m.MovieDirectors)
                    .FirstOrDefaultAsync(m => m.MovieId == model.MovieId);

                if (movie == null)
                    throw new InvalidOperationException("Movie not found");

                movie.Title = model.Title;
                movie.Description = model.Description;
                movie.Genre = model.Genre;
                movie.ReleaseYear = model.ReleaseYear;
                movie.Rating = (double)model.Rating;
                movie.TrailerUrl = model.TrailerUrl;
                movie.UpdatedAt = DateTime.UtcNow;

                if (model.CoverImage != null)
                {
                    if (!string.IsNullOrEmpty(movie.CoverImagePath))
                    {
                        string oldImagePath = Path.Combine(_webHostEnvironment.WebRootPath, movie.CoverImagePath.TrimStart('/'));
                        if (File.Exists(oldImagePath))
                        {
                            File.Delete(oldImagePath);
                        }
                    }

                    string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images", "covers");
                    string uniqueFileName = Guid.NewGuid().ToString() + "_" + model.CoverImage.FileName;
                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    Directory.CreateDirectory(uploadsFolder);
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await model.CoverImage.CopyToAsync(fileStream);
                    }

                    movie.CoverImagePath = "/images/covers/" + uniqueFileName;
                }

                // Обновить актеров
                _context.MovieActors.RemoveRange(movie.MovieActors);
                for (int i = 0; i < model.ActorNames.Count; i++)
                {
                    var actorName = model.ActorNames[i];
                    var role = model.ActorRoles[i];

                    if (!string.IsNullOrWhiteSpace(actorName))
                    {
                        var actor = await _context.Actors.FirstOrDefaultAsync(a => a.Name == actorName)
                            ?? new Actor { Name = actorName };

                        if (actor.ActorId == 0)
                        {
                            _context.Actors.Add(actor);
                            await _context.SaveChangesAsync();
                        }

                        var movieActor = new MovieActor
                        {
                            Movie = movie,
                            MovieId = movie.MovieId,
                            Actor = actor,
                            ActorId = actor.ActorId,
                            Role = role
                        };

                        _context.MovieActors.Add(movieActor);
                    }
                }

                // Обновить режиссеров
                _context.MovieDirectors.RemoveRange(movie.MovieDirectors);
                foreach (var directorName in model.DirectorNames)
                {
                    if (!string.IsNullOrWhiteSpace(directorName))
                    {
                        var director = await _context.Directors.FirstOrDefaultAsync(d => d.Name == directorName)
                            ?? new Director { Name = directorName };

                        if (director.DirectorId == 0)
                        {
                            _context.Directors.Add(director);
                            await _context.SaveChangesAsync();
                        }

                        var movieDirector = new MovieDirector
                        {
                            Movie = movie,
                            MovieId = movie.MovieId,
                            Director = director,
                            DirectorId = director.DirectorId
                        };

                        _context.MovieDirectors.Add(movieDirector);
                    }
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return movie;
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<MovieViewModel> GetMovieForEditAsync(int id)
        {
            var movie = await _context.Movies
                .Include(m => m.MovieActors)
                    .ThenInclude(ma => ma.Actor)
                .Include(m => m.MovieDirectors)
                    .ThenInclude(md => md.Director)
                .FirstOrDefaultAsync(m => m.MovieId == id);

            if (movie == null)
                return null;

            return new MovieViewModel
            {
                MovieId = movie.MovieId,
                Title = movie.Title,
                Description = movie.Description,
                Genre = movie.Genre,
                ReleaseYear = movie.ReleaseYear,
                Rating = (decimal)movie.Rating,
                TrailerUrl = movie.TrailerUrl,
                ExistingCoverImagePath = movie.CoverImagePath,
                ActorNames = movie.MovieActors?.Select(ma => ma.Actor.Name).ToList() ?? new List<string>(),
                ActorRoles = movie.MovieActors?.Select(ma => ma.Role).ToList() ?? new List<string>(),
                DirectorNames = movie.MovieDirectors?.Select(md => md.Director.Name).ToList() ?? new List<string>()
            };
        }

        public async Task<bool> DeleteMovieAsync(int id)
        {
            var movie = await _context.Movies.FindAsync(id);
            if (movie == null)
            {
                return false;
            }

            if (!string.IsNullOrEmpty(movie.CoverImagePath))
            {
                string imagePath = Path.Combine(_webHostEnvironment.WebRootPath, movie.CoverImagePath.TrimStart('/'));
                if (File.Exists(imagePath))
                {
                    File.Delete(imagePath);
                }
            }

            _context.Movies.Remove(movie);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> RateMovieAsync(int movieId, string userId, int rating, string comment)
        {
            var movie = await _context.Movies.FindAsync(movieId);
            var user = await _context.Users.FindAsync(userId);

            if (movie == null || user == null)
            {
                return false;
            }

            var existingRating = await _context.UserRatings
                .FirstOrDefaultAsync(r => r.MovieId == movieId && r.UserId == userId);

            if (existingRating != null)
            {
                existingRating.Rating = rating;
                existingRating.Comment = comment;
                existingRating.RatedAt = DateTime.UtcNow;
            }
            else
            {
                _context.UserRatings.Add(new UserRating
                {
                    MovieId = movieId,
                    UserId = userId,
                    Movie = movie,
                    User = user,
                    Rating = rating,
                    Comment = comment,
                    RatedAt = DateTime.UtcNow
                });
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<double> GetAverageRatingAsync(int movieId)
        {
            var ratings = await _context.UserRatings
                .Where(r => r.MovieId == movieId)
                .Select(r => (double)r.Rating)
                .ToListAsync();

            return ratings.Any() ? Math.Round(ratings.Average(), 1) : 0.0;
        }

        public async Task<UserRating?> GetUserRatingAsync(int movieId, string userId)
        {
            return await _context.UserRatings
                .Include(r => r.User)
                .Include(r => r.Movie)
                .FirstOrDefaultAsync(r => r.MovieId == movieId && r.UserId == userId);
        }

        public async Task<IEnumerable<UserRating>> GetMovieRatingsAsync(int movieId)
        {
            return await _context.UserRatings
                .Include(r => r.User)
                .Include(r => r.Movie)
                .Where(r => r.MovieId == movieId)
                .OrderByDescending(r => r.RatedAt)
                .ToListAsync() ?? Enumerable.Empty<UserRating>();
        }

        public async Task<bool> AddActorToMovieAsync(int movieId, int actorId, string role)
        {
            var movie = await _context.Movies.FindAsync(movieId);
            var actor = await _context.Actors.FindAsync(actorId);

            if (movie == null || actor == null)
            {
                return false;
            }

            var movieActor = new MovieActor
            {
                MovieId = movieId,
                ActorId = actorId,
                Movie = movie,
                Actor = actor,
                Role = role
            };

            _context.MovieActors.Add(movieActor);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> RemoveActorFromMovieAsync(int movieId, int actorId)
        {
            var movieActor = await _context.MovieActors
                .FirstOrDefaultAsync(ma => ma.MovieId == movieId && ma.ActorId == actorId);

            if (movieActor == null)
            {
                return false;
            }

            _context.MovieActors.Remove(movieActor);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> AddDirectorToMovieAsync(int movieId, int directorId)
        {
            var movie = await _context.Movies.FindAsync(movieId);
            var director = await _context.Directors.FindAsync(directorId);

            if (movie == null || director == null)
            {
                return false;
            }

            var movieDirector = new MovieDirector
            {
                MovieId = movieId,
                DirectorId = directorId,
                Movie = movie,
                Director = director
            };

            _context.MovieDirectors.Add(movieDirector);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> RemoveDirectorFromMovieAsync(int movieId, int directorId)
        {
            var movieDirector = await _context.MovieDirectors
                .FirstOrDefaultAsync(md => md.MovieId == movieId && md.DirectorId == directorId);

            if (movieDirector == null)
            {
                return false;
            }

            _context.MovieDirectors.Remove(movieDirector);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<Movie>> SearchMoviesAsync(string searchTerm)
        {
            if (string.IsNullOrEmpty(searchTerm))
            {
                return Enumerable.Empty<Movie>();
            }

            return await _context.Movies
                .Include(m => m.Creator)
                .Include(m => m.MovieActors)
                    .ThenInclude(ma => ma.Actor)
                .Include(m => m.MovieDirectors)
                    .ThenInclude(md => md.Director)
                .Where(m => m.Title.Contains(searchTerm) ||
                        m.Description.Contains(searchTerm) ||
                        m.Genre.Contains(searchTerm) ||
                        m.MovieActors.Any(ma => ma.Actor.Name.Contains(searchTerm)) ||
                        m.MovieDirectors.Any(md => md.Director.Name.Contains(searchTerm)))
                .OrderByDescending(m => m.CreatedAt)
                .ToListAsync() ?? Enumerable.Empty<Movie>();
        }

        public async Task<IEnumerable<Movie>> GetMoviesByRatingRangeAsync(double minRating, double maxRating)
        {
            return await _context.Movies
                .Include(m => m.Creator)
                .Include(m => m.MovieActors)
                    .ThenInclude(ma => ma.Actor)
                .Include(m => m.MovieDirectors)
                    .ThenInclude(md => md.Director)
                .Include(m => m.UserRatings)
                .Where(m => m.UserRatings.Any() &&
                        m.UserRatings.Average(ur => (double)ur.Rating) >= minRating &&
                        m.UserRatings.Average(ur => (double)ur.Rating) <= maxRating)
                .OrderByDescending(m => m.UserRatings.Average(ur => (double)ur.Rating))
                .ToListAsync() ?? Enumerable.Empty<Movie>();
        }

        public async Task<IEnumerable<MovieActor>> GetMovieActorsAsync(int movieId)
        {
            return await _context.MovieActors
                .Include(ma => ma.Actor)
                .Include(ma => ma.Movie)
                .Where(ma => ma.MovieId == movieId)
                .ToListAsync() ?? Enumerable.Empty<MovieActor>();
        }

        public async Task<IEnumerable<MovieDirector>> GetMovieDirectorsAsync(int movieId)
        {
            return await _context.MovieDirectors
                .Include(md => md.Director)
                .Include(md => md.Movie)
                .Where(md => md.MovieId == movieId)
                .ToListAsync() ?? Enumerable.Empty<MovieDirector>();
        }

        public async Task<User?> GetUserByIdAsync(string userId)
        {
            return await _context.Users.FindAsync(userId);
        }

        public async Task<bool> AddOrUpdateRatingAsync(int movieId, string userId, int rating)
        {
            try
            {
                var existingRating = await _context.UserRatings
                    .FirstOrDefaultAsync(r => r.MovieId == movieId && r.UserId == userId);

                if (existingRating != null)
                {
                    existingRating.Rating = rating;
                    existingRating.RatedAt = DateTime.UtcNow;
                }
                else
                {
                    _context.UserRatings.Add(new UserRating
                    {
                        MovieId = movieId,
                        UserId = userId,
                        Rating = rating,
                        RatedAt = DateTime.UtcNow
                    });
                }

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding/updating rating for movie {MovieId} by user {UserId}", movieId, userId);
                return false;
            }
        }

        public async Task<bool> AddOrUpdateCommentAsync(int movieId, string userId, string comment)
        {
            try
            {
                var existingRating = await _context.UserRatings
                    .FirstOrDefaultAsync(r => r.MovieId == movieId && r.UserId == userId);

                if (existingRating != null)
                {
                    existingRating.Comment = comment;
                    existingRating.RatedAt = DateTime.UtcNow;
                }
                else
                {
                    _context.UserRatings.Add(new UserRating
                    {
                        MovieId = movieId,
                        UserId = userId,
                        Comment = comment,
                        Rating = 0, // Оценка по умолчанию, если предоставлен только комментарий
                        RatedAt = DateTime.UtcNow
                    });
                }

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding/updating comment for movie {MovieId} by user {UserId}", movieId, userId);
                return false;
            }
        }
    }
}