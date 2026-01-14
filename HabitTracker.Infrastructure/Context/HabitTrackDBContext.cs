using HabitTracker.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace HabitTracker.Infrastructure.Context
{
    public class HabitTrackDBContext(DbContextOptions<HabitTrackDBContext> options) : DbContext(options)
    {
        public DbSet<UserEntity> Users { get; set; }
        public DbSet<HabitEntity> Habits { get; set; }
        public DbSet<CategoryEntity> Categories { get; set; }
        public DbSet<HabitLog> Logs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<UserEntity>()
                .HasIndex(u => u.Email)
                .IsUnique();

            modelBuilder.Entity<HabitEntity>()
                .HasQueryFilter(h => !h.IsDeleted);

            modelBuilder.Entity<CategoryEntity>()
                .HasQueryFilter(c => !c.IsDeleted);
        }
    } 
}
