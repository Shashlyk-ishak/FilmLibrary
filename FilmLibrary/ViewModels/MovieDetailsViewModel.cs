using FilmLibrary.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FilmLibrary.ViewModels
{
    public class MovieDetailsViewModel
    {
        public Movie Movie { get; set; } = null!;
        public double AverageRating { get; set; }
        public UserRating? UserRating { get; set; }
        public IEnumerable<UserRating> Ratings { get; set; } = Enumerable.Empty<UserRating>();
        public IEnumerable<MovieActor> MovieActors { get; set; } = Enumerable.Empty<MovieActor>();
        public IEnumerable<MovieDirector> MovieDirectors { get; set; } = Enumerable.Empty<MovieDirector>();
    }
}