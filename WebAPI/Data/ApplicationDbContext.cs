using Microsoft.EntityFrameworkCore;
using WebAPI.Models;

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
        public DbSet<RosaryMessages> RosaryMessages { get; set; }
        public DbSet<Parish> Parishes { get; set; }
        public DbSet<ExternalMember> ExternalMembers { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<UsersRosary>()
                .HasOne(ur => ur.User)
                .WithMany(u => u.UserRosaries)
                .HasForeignKey(ur => ur.UserId);
            modelBuilder.Entity<Rosary>()
                .HasOne(r => r.Parish)
                .WithMany() 
                .HasForeignKey(r => r.ParishValue);
            modelBuilder.Entity<UsersRosary>()
                .HasOne(ur => ur.Rosary)
                .WithMany(r => r.UsersRosary)
                .HasForeignKey(ur => ur.RosaryId);
        }
    }
}