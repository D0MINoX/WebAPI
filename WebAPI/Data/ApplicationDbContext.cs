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
    }
}