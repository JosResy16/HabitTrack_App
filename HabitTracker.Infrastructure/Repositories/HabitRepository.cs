using HabitTracker.Application.Common.Interfaces;
using HabitTracker.Application.DTOs;
using HabitTracker.Domain;
using HabitTracker.Domain.Entities;
using HabitTracker.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace HabitTracker.Infrastructure.Repositories;
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

    public async Task<List<HabitEntity>> GetActiveHabits(Guid userId, Priority? priority = null)
    {
        var query = _habitTrackDbContext.Habits
            .Where(h => h.UserId == userId &&
                    !h.IsPaused);

        if (priority.HasValue)
            query = query.Where(h => h.Priority == priority.Value);

        return await query.ToListAsync();
    }

    public async Task<HabitEntity?> GetByTitleAsync(Guid userId, string title)
    {
        return await _habitTrackDbContext.Habits
            .FirstOrDefaultAsync(x => x.UserId == userId && x.Title == title && !x.IsDeleted);
    }

    public async Task<int> GetTotalActiveHabitsAsync(Guid userId)
    {
        return await _habitTrackDbContext.Habits
            .Where(h => h.UserId == userId &&
                    !h.IsPaused)
            .CountAsync();
    }

    public async Task<IEnumerable<HabitEntity>> GetPausedHabitsAsync(Guid userId)
    {
        return await _habitTrackDbContext.Habits
            .Where(h => h.UserId == userId &&
                    h.IsPaused)
            .ToListAsync();
    }

    public async Task<List<HabitPerformanceDTO>> GetHabitPerformanceAsync(Guid userId, DateOnly startDate, DateOnly endDate)
    {
        var daysInRange = endDate.Day;

        var habits = await _habitTrackDbContext.Habits
            .Where(h => h.UserId == userId &&
                    !h.IsPaused)
            .Select(h => new
            {
                h.Id,
                h.Title,
                CompletedDays = h.Logs
                    .Where(l => l.Date >= startDate && l.Date <= endDate)
                    .GroupBy(l => new { l.HabitId, l.Date })
                    .Select(g => new
                    {
                        g.Key.HabitId,
                        g.Key.Date,
                        MaxCreatedAt = g.Max(x => x.CreatedAtUtc)
                    })
                    .Join(
                        h.Logs,
                        g => new { g.HabitId, g.Date, g.MaxCreatedAt },
                        l => new { l.HabitId, l.Date, MaxCreatedAt = l.CreatedAtUtc },
                        (g, l) => l
                    )
                    .Count(l => l.ActionType == ActionType.Completed)
            })
            .ToListAsync();

        var result = habits.Select(h => new HabitPerformanceDTO
        {
            HabitId = h.Id,
            HabitTitle = h.Title,
            TotalCompletionsThisMonth = h.CompletedDays,
            CompletionRateThisMonth = daysInRange == 0
                ? 0
                : Math.Round((double)h.CompletedDays / daysInRange * 100, 1)
        }).ToList();

        return result;
    }
}

