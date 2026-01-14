using HabitTracker.Application.Services;
using HabitTracker.Domain;
using HabitTracker.Domain.Entities;
using HabitTracker.Application.DTOs;
using HabitTracker.Shared.DTOs;

namespace HabitTracker.Application.UseCases.Habits
{
    public interface IHabitQueryService
    {
        Task<Result<HabitResponseDTO>> GetHabitByIdAsync(Guid habitId);
        Task<Result<IEnumerable<HabitResponseDTO>>> GetUserHabitsAsync(Priority? priority = null);
        Task<Result<IEnumerable<HabitResponseDTO>>> GetHabitsByCategoryAsync(Guid categoryId);
        Task<Result<IEnumerable<HabitTodayDTO>>> GetTodayHabitsAsync(DateOnly day);
        Task<Result<IEnumerable<HabitHistoryDTO>>> GetHabitHistoryAsync(Guid habitId);
        Task<Result<IEnumerable<HabitHistoryDTO>>> GetHabitsBetweenDatesAsync(DateOnly startDate, DateOnly endDate);
        Task<Result<IEnumerable<HabitTodayDTO>>> GetHabitsByActionTypeAsync(ActionType actionType, DateOnly day);

    }
}
