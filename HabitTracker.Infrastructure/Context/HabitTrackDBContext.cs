using HabitTracker.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace HabitTracker.Infrastructure.Context
{
    public class HabitTrackDBContext(DbContextOptions<HabitTrackDBContext> options) : DbContext(options)
    {
        public DbSet<UserEntity> Users { get; set; }
        public DbSet<HabitEntity> Habits { get; set; }
        public DbSet<CategoryEntity> Categories { get; set; }
    } 
}
