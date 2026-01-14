

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

        public async Task SaveChangesAsync()
        {
            await _habitTrackDbContext.SaveChangesAsync();
        }

        public async Task AddAsync(HabitEntity habit)
        {
            await _habitTrackDbContext.Habits.AddAsync(habit);
        }

        public async Task<HabitEntity?> GetByIdAsync(Guid id)
        {
            return await _habitTrackDbContext.Habits.FirstOrDefaultAsync(h => h.Id == id && !h.IsDeleted);
        }

        public async Task<IEnumerable<HabitEntity>> GetHabitsByCategoryIdAsync(Guid categoryId, Guid userId)
        {
            var habits = await _habitTrackDbContext.Habits.
                Where(h => h.CategoryId == categoryId &&
                h.UserId == userId && !h.IsDeleted).
                ToListAsync();

            return habits;
        }

        public async Task<IEnumerable<HabitEntity>> GetHabitsAsync(Guid userId, Priority? priority = null)
        {
            var query = _habitTrackDbContext.Habits
                .Where(h => h.UserId == userId && !h.IsDeleted);
            
            if (priority.HasValue)
                query = query.Where(h => h.Priority == priority.Value);

            return await query.ToListAsync();
        }

        public async Task<HabitEntity?> GetByTitleAsync(Guid userId, string title)
        {
            return await _habitTrackDbContext.Habits
                .FirstOrDefaultAsync(x => x.UserId == userId && x.Title == title && !x.IsDeleted);
        }
    }
}
