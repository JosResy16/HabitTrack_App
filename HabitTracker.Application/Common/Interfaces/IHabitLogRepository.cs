using HabitTracker.Application.DTOs;
using HabitTracker.Domain;
using HabitTracker.Domain.Entities;


namespace HabitTracker.Application.Common.Interfaces
{
    public interface IHabitLogRepository
    {
        Task SaveChangesAsync();
        Task AddAsync(HabitLog log);
        Task<IEnumerable<HabitLog>> GetLogsByHabitIdAsync(Guid userId, Guid habitId);
        Task<IEnumerable<HabitLog>> GetLogsByUserIdAsync(Guid userId);
        Task<IEnumerable<HabitLog>> GetLogsByDateAsync(Guid userId, DateOnly date);
        Task<IEnumerable<HabitLog>> GetLogsBetweenDatesAsync(Guid userId, DateOnly startDate, DateOnly endDate);
        Task<IEnumerable<HabitLog>> GetCompletedLogsAsync(Guid userId, DateOnly? date = null);
        Task<IEnumerable<HabitLog>> GetPendingLogsAsync(Guid userId, DateOnly? date = null);
        Task<HabitLog?> GetLogForHabitAndDayAsync(Guid habitId, DateOnly date);
        Task<IEnumerable<HabitLog>> GetLogsByActionTypeAsync(Guid userId, ActionType actionType, DateOnly date);
        Task<HabitLog?> GetLastLogForDateAsync(Guid userId, Guid habitId, DateOnly date);

        Task<int> GetTotalCompletionsAsync(Guid userId);
        Task<int> GetActiveDaysCountAsync(Guid userId, DateOnly startDate, DateOnly endDate);
        Task<List<DateOnly>> GetDistinctActiveDatesAsync(Guid userId);
        Task<List<DailyActivityDTO>> GetDailyActivityAsync(Guid userId, DateOnly startDate, DateOnly endDate);
        Task<List<DailyActivityDTO>> GetHabitActivityAsync(Guid userId, Guid habitId, DateOnly startDate, DateOnly endDate);
        Task<List<HabitLog>> GetLogsBetweenDatesForHabitAsync(Guid userId, Guid habitId, DateOnly day);

    }
}
