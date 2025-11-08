using HabitTracker.Application.Services;
using HabitTracker.Domain.Entities;


namespace HabitTracker.Application.Common.Interfaces
{
    public interface IHabitLogRepository
    {
        Task AddAsync(HabitLog log);
        Task<IEnumerable<HabitLog>> GetLogsByHabitIdAsync(Guid habitId);
        Task<IEnumerable<HabitLog>> GetLogsByUserIdAsync(Guid userId);
        Task<IEnumerable<HabitLog>> GetLogsByDateAsync(Guid userId, DateTime date);
        Task<IEnumerable<HabitLog>> GetLogsBetweenDatesAsync(Guid userId, DateTime startDate, DateTime endDate);
        Task<IEnumerable<HabitLog>> GetCompletedLogsAsync(Guid userId, DateTime? date = null);
        Task<IEnumerable<HabitLog>> GetPendingLogsAsync(Guid userId, DateTime? date = null);
        Task<HabitLog?> GetLogForHabitAndDayAsync(Guid habitId, DateTime date);

    }
}
