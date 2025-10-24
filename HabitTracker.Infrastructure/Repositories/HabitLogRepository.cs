using HabitTracker.Application.Common.Interfaces;
using HabitTracker.Application.Services;
using HabitTracker.Domain.Entities;
using HabitTracker.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HabitTracker.Infrastructure.Repositories
{
    public class HabitLogRepository : IHabitLogRepository
    {
        private readonly HabitTrackDBContext _DbContext;

        public HabitLogRepository(HabitTrackDBContext dbContext)
        {
            _DbContext = dbContext;
        }

        public async Task<Result> AddAsync(HabitLog log)
        {
            await _DbContext.Logs.AddAsync(log);
            await _DbContext.SaveChangesAsync();
            return Result.Success();
        }

        public async Task<Result<IEnumerable<HabitLog>>> GetLogsByDateAsync(Guid userId, DateTime date)
        {
            var list = await _DbContext.Logs
                .Include(l => l.Habit)
                .Where(l => l.Habit.UserId == userId && l.Date.Date == date.Date)
                .ToListAsync();

            return Result<IEnumerable<HabitLog>>.Success(list);
        }

        public async Task<Result<IEnumerable<HabitLog>>> GetLogsByHabitIdAsync(Guid habitId)
        {
            return Result<IEnumerable<HabitLog>>.Success(
                await _DbContext.Logs
                .Where(l => l.HabitId == habitId)
                .OrderByDescending(l => l.Date)
                .ToListAsync());
        }

        public async Task<Result<IEnumerable<HabitLog>>> GetLogsByUserIdAsync(Guid userId)
        {
            return Result<IEnumerable<HabitLog>>.Success(
                await _DbContext.Logs
                .Include(l => l.Habit)
                .Where(l => l.Habit.UserId == userId)
                .OrderByDescending(l => l.Date)
                .ToListAsync());
        }
    }
}
