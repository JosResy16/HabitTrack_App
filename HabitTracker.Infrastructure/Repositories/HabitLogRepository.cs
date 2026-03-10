using HabitTracker.Application.Common.Interfaces;
using HabitTracker.Application.DTOs;
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
                .ThenByDescending(l => l.CreatedAtUtc)
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
                .OrderByDescending(l => l.CreatedAtUtc)
                .FirstOrDefaultAsync();
        }

        public async Task<int> GetTotalCompletionsAsync(Guid userId)
        {
            return await _dbContext.Logs
                .Where(l => l.Habit.UserId == userId &&
                    l.ActionType == ActionType.Completed)
                .GroupBy(l => new { l.HabitId, l.Date })
                .CountAsync();
        }

        public async Task<int> GetActiveDaysCountAsync(Guid userId, DateOnly startDate, DateOnly endDate)
        {
            var logs = await _dbContext.Logs
                .Where(l =>
                    l.Habit.UserId == userId &&
                    l.Date >= startDate &&
                    l.Date <= endDate)
                .OrderByDescending(l => l.CreatedAtUtc)
                .ToListAsync();

            var activeDays = logs
                .GroupBy(l => l.Date)
                .Select(g => g.First())
                .Count(l => l.ActionType == ActionType.Completed);

            return activeDays;
        }

        public async Task<List<DateOnly>> GetDistinctActiveDatesAsync(Guid userId)
        {
            var latestLogsQuery = _dbContext.Logs
                .Where(l => l.Habit.UserId == userId &&
                            (l.ActionType == ActionType.Completed ||
                             l.ActionType == ActionType.Undone))
                .GroupBy(l => new { l.HabitId, l.Date })
                .Select(g => new
                {
                    g.Key.HabitId,
                    g.Key.Date,
                    MaxCreatedAt = g.Max(x => x.CreatedAtUtc)
                });

            var activeDates = await (
                from l in _dbContext.Logs
                join latest in latestLogsQuery
                    on new { l.HabitId, l.Date, l.CreatedAtUtc }
                    equals new { latest.HabitId, latest.Date, CreatedAtUtc = latest.MaxCreatedAt }
                where l.ActionType == ActionType.Completed
                select l.Date
            )
            .Distinct()
            .ToListAsync();

            return activeDates;
        }

        public async Task<List<DailyActivityDTO>> GetDailyActivityAsync(Guid userId, DateOnly startDate, DateOnly endDate)
        {
            var latestLogsQuery = _dbContext.Logs
                .Where(l => l.Habit.UserId == userId &&
                            l.Date >= startDate &&
                            l.Date <= endDate)
                .GroupBy(l => new { l.HabitId, l.Date })
                .Select(g => new
                {
                    g.Key.HabitId,
                    g.Key.Date,
                    MaxCreatedAt = g.Max(x => x.CreatedAtUtc)
                });

            var result = await (
                from l in _dbContext.Logs
                join latest in latestLogsQuery
                    on new { l.HabitId, l.Date, l.CreatedAtUtc }
                    equals new { latest.HabitId, latest.Date, CreatedAtUtc = latest.MaxCreatedAt }
                where l.ActionType == ActionType.Completed
                group l by l.Date into g
                select new DailyActivityDTO
                {
                    Date = g.Key,
                    CompletionsCount = g.Count()
                })
            .OrderBy(x => x.Date)
            .ToListAsync();

            return result;
        }

        public async Task<List<DailyActivityDTO>> GetHabitActivityAsync(Guid userId, Guid habitId, DateOnly startDate, DateOnly endDate)
        {
            return await _dbContext.Logs
                .Where(l => l.Habit.UserId == userId &&
                            l.HabitId == habitId &&
                            l.Date >= startDate &&
                            l.Date <= endDate)
                .GroupBy(l => l.Date)
                .Select(g => new DailyActivityDTO
                {
                    Date = g.Key,
                    CompletionsCount = g
                        .Where(l => l.ActionType == ActionType.Completed
                                 || l.ActionType == ActionType.Undone)
                        .OrderByDescending(l => l.CreatedAtUtc)
                        .Select(l => l.ActionType == ActionType.Completed ? 1 : 0)
                        .FirstOrDefault()
                })
                .OrderBy(x => x.Date)
                .ToListAsync();
        }

        public async Task<List<HabitLog>> GetLogsBetweenDatesForHabitAsync(Guid userId, Guid habitId, DateOnly day)
        {
            return await _dbContext.Logs
                .Where(l => l.Habit.UserId == userId &&
                            l.HabitId == habitId &&
                            l.Date == day)
                .OrderBy(x => x.Date)
                .ToListAsync();
        }
    }
}
