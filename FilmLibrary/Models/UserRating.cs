using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FilmLibrary.Models
{
    public class UserRating
    {
        [Key]
        public int UserRatingId { get; set; }
        public int MovieId { get; set; }
        public string UserId { get; set; } = string.Empty;
        public int Rating { get; set; }
        public string? Comment { get; set; }
        public DateTime RatedAt { get; set; } = DateTime.UtcNow;

        // Свойство навигации
        [ForeignKey("MovieId")]
        public virtual Movie Movie { get; set; } = null!;

        [ForeignKey("UserId")]
        public virtual User User { get; set; } = null!;
    }
}