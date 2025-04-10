using System.ComponentModel.DataAnnotations;

namespace FilmLibrary.Models
{
    public class Director
    {
        public Director()
        {
            MovieDirectors = new List<MovieDirector>();
        }

        public int DirectorId { get; set; }

        [Required]
        [StringLength(100)]
        public required string Name { get; set; }

        public string? Biography { get; set; }

        public string? PhotoPath { get; set; }

        // Свойство навигации
        public virtual ICollection<MovieDirector> MovieDirectors { get; init; }
    }
}