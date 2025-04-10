namespace FilmLibrary.Models
{
    public class MovieDirector
    {
        public int MovieId { get; set; }
        public int DirectorId { get; set; }

        // Свойство навигации
        public required virtual Movie Movie { get; set; }
        public required virtual Director Director { get; set; }
    }
}