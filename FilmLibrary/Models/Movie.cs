using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace FilmLibrary.Models
{
    public class Movie
    {
        public Movie()
        {
            MovieActors = new List<MovieActor>();
            MovieDirectors = new List<MovieDirector>();
            UserRatings = new List<UserRating>();
            CreatedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
            Title = string.Empty;
            Description = string.Empty;
            Genre = string.Empty;
            CoverImagePath = string.Empty;
        }

        public int MovieId { get; set; }

        [Required]
        public string Title { get; set; }

        [Required]
        public string Description { get; set; }

        [Required]
        public string Genre { get; set; }

        public string CoverImagePath { get; set; }

        [Required]
        [Range(1888, 2100)]
        public int ReleaseYear { get; set; }

        public double Rating { get; set; }

        public string CreatedBy { get; set; } = string.Empty;

        public User Creator { get; set; } = null!;

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }

        public string? TrailerUrl { get; set; }

        // Свойство навигации
        public virtual ICollection<MovieActor> MovieActors { get; set; }
        public virtual ICollection<MovieDirector> MovieDirectors { get; set; }
        public virtual ICollection<UserRating> UserRatings { get; set; }
    }
}