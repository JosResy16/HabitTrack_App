using HabitTracker.Application.Services;
using HabitTracker.Domain;
using HabitTracker.Domain.Entities;

namespace HabitTracker.Application.UseCases.Habits
{
    public interface IHabitLogService
    {
        Task<Result> AddLogAsync(Guid habitId, ActionType actionType);
        Task<Result<IEnumerable<HabitLog>>> GetLogsByUserAsync(Guid userId);
        Task<Result<IEnumerable<HabitLog>>> GetLogsByHabitAsync(Guid habitId);
        Task<Result<IEnumerable<HabitLog>>> GetLogsByDateAsync(DateTime date);
        Task<Result<HabitLog?>> GetLogForHabitAndDayAsync(Guid habitId, DateTime day);
        Task<Result<IEnumerable<HabitLog?>>> GetLogsBetweenDatesAsync(DateTime startDate, DateTime endDate);
        Task<Result<IEnumerable<HabitLog?>>> GetLogsByActionTypeAsync(ActionType actionType, DateTime day);

    }
}
