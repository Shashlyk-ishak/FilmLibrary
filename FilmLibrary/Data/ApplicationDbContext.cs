using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using FilmLibrary.Models;
using Microsoft.AspNetCore.Identity;

namespace FilmLibrary.Data
{
    public class ApplicationDbContext : IdentityDbContext<User>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Movie> Movies { get; set; } = null!;
        public DbSet<Actor> Actors { get; set; } = null!;
        public DbSet<Director> Directors { get; set; } = null!;
        public DbSet<MovieActor> MovieActors { get; set; } = null!;
        public DbSet<MovieDirector> MovieDirectors { get; set; } = null!;
        public DbSet<UserRating> UserRatings { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Настройка связей и ограничений
            modelBuilder.Entity<MovieActor>()
                .HasKey(ma => new { ma.MovieId, ma.ActorId });

            modelBuilder.Entity<MovieDirector>()
                .HasKey(md => new { md.MovieId, md.DirectorId });

            modelBuilder.Entity<UserRating>()
                .HasKey(ur => ur.UserRatingId);

            modelBuilder.Entity<UserRating>()
                .HasIndex(ur => new { ur.UserId, ur.MovieId })
                .IsUnique();

            // Настройка сущности фильма
            modelBuilder.Entity<Movie>()
                .HasOne(m => m.Creator)
                .WithMany(u => u.CreatedMovies)
                .HasForeignKey(m => m.CreatedBy)
                .OnDelete(DeleteBehavior.Restrict);

            // Настройка сущности UserRating
            modelBuilder.Entity<UserRating>()
                .HasOne(ur => ur.User)
                .WithMany(u => u.UserRatings)
                .HasForeignKey(ur => ur.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<UserRating>()
                .HasOne(ur => ur.Movie)
                .WithMany(m => m.UserRatings)
                .HasForeignKey(ur => ur.MovieId)
                .OnDelete(DeleteBehavior.Cascade);

            // Настройка индексов
            modelBuilder.Entity<Movie>()
                .HasIndex(m => m.Title);

            modelBuilder.Entity<Movie>()
                .HasIndex(m => m.Rating);

            modelBuilder.Entity<Actor>()
                .HasIndex(a => a.Name);

            modelBuilder.Entity<Director>()
                .HasIndex(d => d.Name);
        }
    }
}