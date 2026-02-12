

using HabitTracker.Application.DTOs;
using HabitTracker.Application.Services;

namespace HabitTracker.Application.UseCases.Habits
{
    public interface IHabitsService
    {
        Task<Result<HabitResponseDTO>> AddNewHabitAsync(CreateHabitDTO habit);
        Task<Result> RemoveHabitAsync(Guid habitId);        
        Task<Result<HabitResponseDTO?>> UpdateHabitAsync(Guid habitId, UpdateHabitDTO habitDto);
        Task<Result> MarkHabitAsDone(Guid habitId);
        Task<Result> UndoHabitCompletion(Guid habitId);
    }
}
