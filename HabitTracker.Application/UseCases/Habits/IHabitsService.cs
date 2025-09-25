

using HabitTracker.Application.DTOs;
using HabitTracker.Domain.Entities;

namespace HabitTracker.Application.UseCases.Habits
{
    public interface IHabitsService
    {
        Task<HabitEntity> AddNewHabitAsync(HabitDTO habit);
        Task RemoveHabitAsync(HabitEntity habit);
        Task<HabitEntity> GetHabitByIdAsync(Guid habitId);
        Task<HabitEntity?> UpdateHabit(Guid habitId, HabitDTO habitDto);
        Task<List<HabitEntity>> GetHabitsByUserIdAsync();
    }
}
