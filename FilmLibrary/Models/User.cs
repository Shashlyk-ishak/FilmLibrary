using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;

namespace FilmLibrary.Models
{
    public class User : IdentityUser
    {
        public User()
        {
            CreatedMovies = new List<Movie>();
            UserRatings = new List<UserRating>();
            CreatedAt = DateTime.UtcNow;
            IsActive = true;
        }

        public DateTime CreatedAt { get; set; }
        public DateTime? LastLoginAt { get; set; }
        public bool IsActive { get; set; }

        //Свойство навигации
        public virtual ICollection<Movie> CreatedMovies { get; set; }
        public virtual ICollection<UserRating> UserRatings { get; set; }
    }
}