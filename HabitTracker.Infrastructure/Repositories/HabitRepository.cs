

using HabitTracker.Application.Common.Interfaces;
using HabitTracker.Application.DTOs;
using HabitTracker.Domain.Entities;
using HabitTracker.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace HabitTracker.Infrastructure.Repositories
{
    public class HabitRepository : IHabitRepository
    {
        private readonly HabitTrackDBContext _habitTrackDbContext;

        public HabitRepository(HabitTrackDBContext habitTrackDBContext)
        {
            _habitTrackDbContext = habitTrackDBContext;
        }

        public async Task AddAsync(HabitEntity habit)
        {
            await _habitTrackDbContext.Habits.AddAsync(habit);
            await _habitTrackDbContext.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var habit = await _habitTrackDbContext.Habits.FindAsync(id);
            if (habit != null)
                _habitTrackDbContext.Remove(habit);

            await _habitTrackDbContext.SaveChangesAsync();
        }

        public async Task<HabitEntity?> GetByIdAsync(Guid id)
        {
            var result =  await _habitTrackDbContext.Habits.FirstOrDefaultAsync(h => h.Id == id);
            return result;
        }

        public async Task<List<HabitEntity>> GetHabitsByUserIdAsync(Guid userId)
        {
            var habits = await _habitTrackDbContext.Habits.Where(x => x.UserId == userId).ToListAsync();
            return habits;
        }

        public async Task UpdateAsync(HabitEntity habit)
        {
            _habitTrackDbContext.Update(habit);
            await _habitTrackDbContext.SaveChangesAsync();
        }
    }
}
