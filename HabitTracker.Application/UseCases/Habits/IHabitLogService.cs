using HabitTracker.Application.Services;
using HabitTracker.Domain.Entities;

namespace HabitTracker.Application.UseCases.Habits
{
    public interface IHabitLogService
    {
        Task<Result> AddLogAsync(Guid habitId, bool isCompleted);
        Task<Result<IEnumerable<HabitLog>>> GetLogsByUserAsync(Guid userId);
        Task<Result<IEnumerable<HabitLog>>> GetLogsByHabitAsync(Guid habitId);
        Task<Result<IEnumerable<HabitLog>>> GetLogsByDateAsync(DateTime date);
    }
}
