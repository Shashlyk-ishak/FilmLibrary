using System.ComponentModel.DataAnnotations;

namespace FilmLibrary.Models
{
    public class Actor
    {
        public Actor()
        {
            MovieActors = new List<MovieActor>();
        }

        public int ActorId { get; set; }

        [Required]
        [StringLength(100)]
        public required string Name { get; set; }

        public string? Biography { get; set; }

        public string? PhotoPath { get; set; }

        // �������� ���������
        public virtual ICollection<MovieActor> MovieActors { get; init; }
    }
}