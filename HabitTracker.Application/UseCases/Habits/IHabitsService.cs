

using HabitTracker.Application.DTOs;
using HabitTracker.Domain.Entities;

namespace HabitTracker.Application.UseCases.Habits
{
    public interface IHabitsService
    {
        Task<Habit> AddNewHabitAsync(HabitDTO habit);
        Task RemoveHabitAsync(Habit habit);
        Task<Habit> GetHabitByIdAsync(Guid habitId);
        Task<Habit?> UpdateHabit(Guid habitId, HabitDTO habitDto);
        Task<List<Habit>> GetHabitsByUserIdAsync();
    }
}
