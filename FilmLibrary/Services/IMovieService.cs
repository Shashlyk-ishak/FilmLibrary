using FilmLibrary.Models;
using FilmLibrary.ViewModels;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FilmLibrary.Services.Interfaces
{
    public interface IMovieService
    {
        Task<IEnumerable<Movie>> GetAllMoviesAsync();
        Task<Movie?> GetMovieByIdAsync(int id);
        Task<Movie> CreateMovieAsync(MovieViewModel model, string userId);
        Task<Movie> UpdateMovieAsync(MovieViewModel model, string userId);
        Task<MovieViewModel> GetMovieForEditAsync(int id);
        Task<bool> DeleteMovieAsync(int id);
        Task<bool> RateMovieAsync(int movieId, string userId, int rating, string comment);
        Task<double> GetAverageRatingAsync(int movieId);
        Task<UserRating?> GetUserRatingAsync(int movieId, string userId);
        Task<IEnumerable<UserRating>> GetMovieRatingsAsync(int movieId);
        Task<bool> AddActorToMovieAsync(int movieId, int actorId, string role);
        Task<bool> RemoveActorFromMovieAsync(int movieId, int actorId);
        Task<bool> AddDirectorToMovieAsync(int movieId, int directorId);
        Task<bool> RemoveDirectorFromMovieAsync(int movieId, int directorId);
        Task<IEnumerable<Movie>> SearchMoviesAsync(string searchTerm);
        Task<IEnumerable<Movie>> GetMoviesByRatingRangeAsync(double minRating, double maxRating);
        Task<IEnumerable<MovieActor>> GetMovieActorsAsync(int movieId);
        Task<IEnumerable<MovieDirector>> GetMovieDirectorsAsync(int movieId);
        Task<User?> GetUserByIdAsync(string userId);
        Task<bool> AddOrUpdateRatingAsync(int movieId, string userId, int rating);
        Task<bool> AddOrUpdateCommentAsync(int movieId, string userId, string comment);
    }
}