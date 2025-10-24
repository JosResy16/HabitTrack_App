using HabitTracker.Application.Services;
using HabitTracker.Domain.Entities;


namespace HabitTracker.Application.Common.Interfaces
{
    public interface IHabitLogRepository
    {
        Task<Result> AddAsync(HabitLog log);
        Task<Result<IEnumerable<HabitLog>>> GetLogsByHabitIdAsync(Guid habitId);
        Task<Result<IEnumerable<HabitLog>>> GetLogsByUserIdAsync(Guid userId);
        Task<Result<IEnumerable<HabitLog>>> GetLogsByDateAsync(Guid userId, DateTime date);
    }
}
