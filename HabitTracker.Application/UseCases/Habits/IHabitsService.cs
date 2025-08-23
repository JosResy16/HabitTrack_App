

using HabitTracker.Application.DTOs;
using HabitTracker.Domain.Entities;

namespace HabitTracker.Application.UseCases.Habits
{
    public interface IHabitsService
    {
        Task<Habit> AddNewHabitAsync(Guid userId, HabitDTO habit);
        Task RemoveHabitAsync(Habit habit);
        Task<Habit> GetHabitByIdAsync(Guid habitId, Guid userId);
        Task<Habit?> UpdateHabit(Guid habitId, HabitDTO habitDto, Guid userId);
        Task<List<Habit>> GetHabitsByUserIdAsync(Guid id);
    }
}
