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
    }
}
