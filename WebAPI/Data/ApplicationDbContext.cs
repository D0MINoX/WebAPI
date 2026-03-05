using Microsoft.EntityFrameworkCore;
using WebAPI.Models; // Upewnij się, że masz folder Models

namespace WebAPI.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; } 
        public DbSet<Meditation> Meditations { get; set; }
        public DbSet<Rosary> Rosary { get; set; }
        public DbSet<UsersRosary> UsersRosary { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            // Konfiguracja relacji dla tabeli łączącej
            modelBuilder.Entity<UsersRosary>()
                .HasOne(ur => ur.User)
                .WithMany(u => u.UserRosaries)
                .HasForeignKey(ur => ur.UserId);

            modelBuilder.Entity<UsersRosary>()
                .HasOne(ur => ur.Rosary)
                .WithMany(r => r.UsersRosary)
                .HasForeignKey(ur => ur.RosaryId);
        }
    }
}