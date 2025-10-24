using HabitTracker.Application.DTOs;
using HabitTracker.Domain;
using HabitTracker.Domain.Entities;
using HabitTracker.Application.Services;


namespace HabitTracker.Application.Common.Interfaces
{
    public interface IHabitRepository
    {
        Task<Result<HabitEntity?>> GetByIdAsync(Guid id);
        Task<Result<List<HabitEntity>>> GetHabitsByUserIdAsync(Guid userId);
        Task<Result> AddAsync(HabitEntity habit);
        Task<bool> UpdateAsync(HabitEntity habit);
        Task<bool> DeleteAsync(Guid id);
        Task<Result<IEnumerable<HabitEntity>>> GetHabitsByCategoryIdAsync(Guid categoryId, Guid userId);
        Task<Result<IEnumerable<HabitEntity>>> GetHabitsAsync(Guid userId, Priority? priority = null);
    }
}
