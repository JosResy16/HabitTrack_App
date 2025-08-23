using HabitTracker.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace HabitTracker.Infrastructure.Context
{
    public class HabitTrackDBContext(DbContextOptions<HabitTrackDBContext> options) : DbContext(options)
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Habit> Habits { get; set; }
    } 
}
