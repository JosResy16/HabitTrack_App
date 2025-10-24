using HabitTracker.Application.Services;
using HabitTracker.Domain;
using HabitTracker.Domain.Entities;

namespace HabitTracker.Application.UseCases.Habits
{
    public interface IHabitQueryService
    {
        Task<Result<HabitEntity>> GetHabitByIdAsync(Guid habitId);
        Task<Result<IEnumerable<HabitEntity>>> GetHabitsAsync(Priority? priority = null);
        Task<Result<IEnumerable<HabitEntity>>> GetHabitsByPriorityAsync(Priority? priority = null);
        Task<Result<IEnumerable<HabitEntity>>> GetHabitsByCategoryAsync(Guid categoryId);
        Task<Result<IEnumerable<HabitEntity>>> GetTodayHabitsAsync(DateTime day);
        Task<Result<IEnumerable<HabitEntity>>> GetHabitHistoryAsync();
    }
}
