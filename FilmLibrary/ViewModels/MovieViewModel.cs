using System.ComponentModel.DataAnnotations;
using FilmLibrary.Models;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;

namespace FilmLibrary.ViewModels
{
    public class MovieViewModel
    {
        public int MovieId { get; set; }

        [Required(ErrorMessage = "Название фильма обязательно для заполнения")]
        [StringLength(200, ErrorMessage = "Название фильма не может быть длиннее 200 символов")]
        [Display(Name = "Название")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Описание фильма обязательно для заполнения")]
        [Display(Name = "Описание")]
        public string Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "Жанр обязателен для заполнения")]
        [StringLength(50, ErrorMessage = "Жанр не может быть длиннее 50 символов")]
        [Display(Name = "Жанр")]
        public string Genre { get; set; } = string.Empty;

        [Required(ErrorMessage = "Год выпуска обязателен для заполнения")]
        [Range(1888, 2100, ErrorMessage = "Год выпуска должен быть между 1888 и 2100")]
        [Display(Name = "Год выпуска")]
        public int ReleaseYear { get; set; }

        [Range(0, 10, ErrorMessage = "Рейтинг должен быть между 0 и 10")]
        [Display(Name = "Рейтинг")]
        public decimal Rating { get; set; }

        [Url(ErrorMessage = "Пожалуйста, введите корректный URL")]
        [Display(Name = "Ссылка на трейлер")]
        public string? TrailerUrl { get; set; }

        [Display(Name = "Постер фильма")]
        public IFormFile? CoverImage { get; set; }

        [Display(Name = "Текущий постер")]
        public string? ExistingCoverImagePath { get; set; }

        public List<string> ActorNames { get; set; } = new();
        public List<string> ActorRoles { get; set; } = new();
        public List<string> DirectorNames { get; set; } = new();

        public Movie ToMovie()
        {
            return new Movie
            {
                MovieId = MovieId,
                Title = Title,
                Description = Description,
                Genre = Genre,
                ReleaseYear = ReleaseYear,
                Rating = (double)Rating,
                TrailerUrl = TrailerUrl,
                CoverImagePath = ExistingCoverImagePath
            };
        }

        public static MovieViewModel FromMovie(Movie movie)
        {
            return new MovieViewModel
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
        }
    }
}