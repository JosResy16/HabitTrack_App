

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

        public async Task<Result> AddAsync(HabitEntity habit)
        {
            await _habitTrackDbContext.Habits.AddAsync(habit);
            await _habitTrackDbContext.SaveChangesAsync();
            return Result.Success();
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

        public async Task<Result<HabitEntity?>> GetByIdAsync(Guid id)
        {
            var result =  await _habitTrackDbContext.Habits.FirstOrDefaultAsync(h => h.Id == id);
            return Result<HabitEntity>.Success(result);
        }

        public async Task<Result<IEnumerable<HabitEntity>>> GetHabitsByCategoryIdAsync(Guid categoryId, Guid userId)
        {
            var habits = await _habitTrackDbContext.Habits.
                Where(h => h.CategoryId == categoryId &&
                h.UserId == userId).
                ToListAsync();

            if (!habits.Any())
                return Result<IEnumerable<HabitEntity>>.Success(new List<HabitEntity>());

            return Result<IEnumerable<HabitEntity>>.Success(habits);
        }

        public async Task<Result<IEnumerable<HabitEntity>>> GetHabitsAsync(Guid userId, Priority? priority = null)
        {
            var query = _habitTrackDbContext.Habits
                .Where(h => h.UserId == userId && !h.IsDeleted);
            
            if (priority.HasValue)
                query.Where(h => h.Priority == priority.Value);

            return Result<IEnumerable<HabitEntity>>.Success(await query.ToListAsync());
        }

        public async Task<Result<List<HabitEntity>>> GetHabitsByUserIdAsync(Guid userId)
        {
            var habits = await _habitTrackDbContext.Habits.Where(x => x.UserId == userId).ToListAsync();
            return Result<List<HabitEntity>>.Success(habits);
        }

        public async Task<bool> UpdateAsync(HabitEntity habit)
        {
            _habitTrackDbContext.Update(habit);
            var rowsAffected = await _habitTrackDbContext.SaveChangesAsync();
            return rowsAffected > 0 ;
        }
    }
}
