

using HabitTracker.Application.DTOs;
using HabitTracker.Application.Services;
using HabitTracker.Domain;
using HabitTracker.Domain.Entities;

namespace HabitTracker.Application.UseCases.Habits
{
    public interface IHabitsService
    {
        Task<Result<HabitEntity>> AddNewHabitAsync(HabitDTO habit);
        Task<Result> RemoveHabitAsync(HabitEntity habit);        
        Task<Result<HabitEntity?>> UpdateHabitAsync(Guid habitId, HabitDTO habitDto);
        Task<Result> MarkHabitAsDone(Guid habitId);
        Task<Result> UndoHabitCompletion(Guid habitId);
    }
}
