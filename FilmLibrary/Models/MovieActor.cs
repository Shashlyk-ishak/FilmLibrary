using System.ComponentModel.DataAnnotations;

namespace FilmLibrary.Models
{
    public class MovieActor
    {
        public int MovieId { get; set; }
        public int ActorId { get; set; }

        [Required]
        [StringLength(100)]
        public required string Role { get; set; }

        // Свойство навигации
        public required virtual Movie Movie { get; set; }
        public required virtual Actor Actor { get; set; }
    }
}