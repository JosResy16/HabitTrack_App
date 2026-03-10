
using HabitTracker.Application.DTOs;
using HabitTracker.Domain;
using HabitTracker.Domain.Entities;



namespace HabitTracker.Application.Common.Interfaces;
public interface IHabitRepository
{
    Task SaveChangesAsync();
    Task<HabitEntity?> GetByIdAsync(Guid id);
    Task AddAsync(HabitEntity habit);
    Task<IEnumerable<HabitEntity>> GetHabitsByCategoryIdAsync(Guid categoryId, Guid userId);
    Task<IEnumerable<HabitEntity>> GetHabitsAsync(Guid userId, Priority? priority = null);
    Task<List<HabitEntity>> GetActiveHabits(Guid userId, Priority? priority = null);
    Task<IEnumerable<HabitEntity>> GetPausedHabitsAsync(Guid userId);
    Task<HabitEntity?> GetByTitleAsync(Guid userId, string title);

    Task<int> GetTotalActiveHabitsAsync(Guid userId);
    Task<List<HabitPerformanceDTO>> GetHabitPerformanceAsync(Guid userId, DateOnly startDate, DateOnly endDate);
}
