using HabitTracker.Application.Services;
using HabitTracker.Domain;
using HabitTracker.Domain.Entities;
using HabitTracker.Application.DTOs;

namespace HabitTracker.Application.UseCases.Habits
{
    public interface IHabitQueryService
    {
        Task<Result<HabitEntity>> GetHabitByIdAsync(Guid habitId);
        Task<Result<IEnumerable<HabitEntity>>> GetUserHabitsAsync(Priority? priority = null);
        Task<Result<IEnumerable<HabitEntity>>> GetHabitsByCategoryAsync(Guid categoryId);
        Task<Result<IEnumerable<HabitTodayDTO>>> GetTodayHabitsAsync(DateTime day);
        Task<Result<IEnumerable<HabitHistoryDTO>>> GetHabitHistoryAsync(Guid habitId);
        Task<Result<IEnumerable<HabitHistoryDTO>>> GetHabitsBetweenDatesAsync(DateTime startDate, DateTime endDate);

    }
}
