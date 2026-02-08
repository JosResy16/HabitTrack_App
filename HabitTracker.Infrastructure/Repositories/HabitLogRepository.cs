using HabitTracker.Application.Common.Interfaces;
using HabitTracker.Domain;
using HabitTracker.Domain.Entities;
using HabitTracker.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace HabitTracker.Infrastructure.Repositories
{
    public class HabitLogRepository : IHabitLogRepository
    {
        private readonly HabitTrackDBContext _dbContext;

        public HabitLogRepository(HabitTrackDBContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task SaveChangesAsync()
        {
            await _dbContext.SaveChangesAsync();
        }

        public async Task AddAsync(HabitLog log)
        {
            await _dbContext.Logs.AddAsync(log);
        }

        public async Task<IEnumerable<HabitLog>> GetLogsByDateAsync(Guid userId, DateOnly date)
        {
            var day = date;

            return await _dbContext.Logs
            .Include(l => l.Habit)
            .Where(l => l.Habit.UserId == userId &&
                        !l.Habit.IsDeleted &&
                        l.Date == day)
            .ToListAsync();
        }

        public async Task<IEnumerable<HabitLog>> GetLogsByHabitIdAsync(Guid userId, Guid habitId)
        {
            return await _dbContext.Logs
                .Include(l => l.Habit)
                .Where(l => l.Habit.UserId == userId &&
                    l.HabitId == habitId)
                .OrderByDescending(l => l.Date)
                .ThenByDescending(l => l.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<HabitLog>> GetLogsByUserIdAsync(Guid userId)
        {
            return await _dbContext.Logs
                .Include(l => l.Habit)
                .Where(l => l.Habit.UserId == userId)
                .OrderByDescending(l => l.Date)
                .ToListAsync();
        }

        public async Task<IEnumerable<HabitLog>> GetLogsBetweenDatesAsync(Guid userId, DateOnly startDate, DateOnly endDate)
        {
            var start = startDate;
            var end = endDate;

            return await _dbContext.Logs
                .Include(l => l.Habit)
                .Where(l => l.Habit.UserId == userId &&
                            !l.Habit.IsDeleted &&
                            l.Date >= start &&
                            l.Date <= end)
                .OrderBy(l => l.Date)
                .ToListAsync();
        }

        public async Task<IEnumerable<HabitLog>> GetCompletedLogsAsync(Guid userId, DateOnly? date = null)
        {
            var query = _dbContext.Logs
                .Include(l => l.Habit)
                .Where(l => l.Habit.UserId == userId &&
                            !l.Habit.IsDeleted &&
                            l.ActionType == ActionType.Completed);

            if (date.HasValue)
                query = query.Where(l => l.Date == date);

            return await query.OrderBy(l => l.Date).ToListAsync();
        }
        public async Task<IEnumerable<HabitLog>> GetPendingLogsAsync(Guid userId, DateOnly? date = null)
        {
            var query = _dbContext.Logs
                .Include(l => l.Habit)
                .Where(l => l.Habit.UserId == userId &&
                            !l.Habit.IsDeleted &&
                            l.ActionType == ActionType.Undone);

            if (date.HasValue)
                query = query.Where(l => l.Date == date);

            return await query.OrderBy(l => l.Date).ToListAsync();
        }

        public async Task<HabitLog?> GetLogForHabitAndDayAsync(Guid habitId, DateOnly date)
        {
            var day = date;

            return await _dbContext.Logs
                .Include(l => l.Habit)
                .FirstOrDefaultAsync(l => l.HabitId == habitId && l.Date == day);
        }

        public async Task<IEnumerable<HabitLog>> GetLogsByActionTypeAsync(Guid userId, ActionType actionType, DateOnly date)
        {
            var start = date;
            var end = start.AddDays(1);

            return await _dbContext.Logs
                .Include(l => l.Habit)
                .Where(l =>
                l.ActionType == actionType &&
                l.Date >= start &&
                l.Date < end &&
                l.Habit.UserId == userId
            )
            .ToListAsync();
        }

        public async Task<HabitLog?> GetLastLogForDateAsync(Guid userId, Guid habitId, DateOnly date)
        {
            return await _dbContext.Logs
                .Include(l => l.Habit)
                .Where(l =>
                    l.Habit.UserId == userId &&
                    l.HabitId == habitId &&
                    l.Date == date)
                .OrderByDescending(l => l.CreatedAt)
                .FirstOrDefaultAsync();
        }
    }
}
