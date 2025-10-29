

using HabitTracker.Application.Common.Interfaces;
using HabitTracker.Application.DTOs;
using HabitTracker.Application.Services;
using HabitTracker.Domain;
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

        public async Task<bool> AddAsync(HabitEntity habit)
        {
            await _habitTrackDbContext.Habits.AddAsync(habit);
            var rowsAffected = await _habitTrackDbContext.SaveChangesAsync();
            return rowsAffected > 0;
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var habit = await _habitTrackDbContext.Habits.FindAsync(id);
            if (habit == null)
                return false;

            habit.IsDeleted = true;
            _habitTrackDbContext.Update(habit);

            var rowsAffected = await _habitTrackDbContext.SaveChangesAsync();
            return rowsAffected > 0;
        }

        public async Task<HabitEntity?> GetByIdAsync(Guid id)
        {
            var result =  await _habitTrackDbContext.Habits.FirstOrDefaultAsync(h => h.Id == id);
            if (result == null)
                return null;

            return result;
        }

        public async Task<IEnumerable<HabitEntity>> GetHabitsByCategoryIdAsync(Guid categoryId, Guid userId)
        {
            var habits = await _habitTrackDbContext.Habits.
                Where(h => h.CategoryId == categoryId &&
                h.UserId == userId).
                ToListAsync();

            if (!habits.Any())
                return new List<HabitEntity>();

            return habits;
        }

        public async Task<IEnumerable<HabitEntity>> GetHabitsAsync(Guid userId, Priority? priority = null)
        {
            var query = _habitTrackDbContext.Habits
                .Where(h => h.UserId == userId && !h.IsDeleted);
            
            if (priority.HasValue)
                query.Where(h => h.Priority == priority.Value);

            return await query.ToListAsync();
        }

        public async Task<List<HabitEntity>> GetHabitsByUserIdAsync(Guid userId)
        {
            var habits = await _habitTrackDbContext.Habits.Where(x => x.UserId == userId).ToListAsync();
            
            if(!habits.Any())
                return new List<HabitEntity>();

            return habits;
        }

        public async Task<bool> UpdateAsync(HabitEntity habit)
        {
            _habitTrackDbContext.Update(habit);
            var rowsAffected = await _habitTrackDbContext.SaveChangesAsync();
            return rowsAffected > 0 ;
        }
    }
}
