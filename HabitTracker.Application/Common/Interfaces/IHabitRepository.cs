using HabitTracker.Application.DTOs;
using HabitTracker.Domain;
using HabitTracker.Domain.Entities;
using HabitTracker.Application.Services;


namespace HabitTracker.Application.Common.Interfaces
{
    public interface IHabitRepository
    {
        Task<HabitEntity?> GetByIdAsync(Guid id);
        Task<List<HabitEntity>> GetHabitsByUserIdAsync(Guid userId);
        Task AddAsync(HabitEntity habit);
        Task<bool> UpdateAsync(HabitEntity habit);
        Task<bool> DeleteAsync(Guid id);
        Task<IEnumerable<HabitEntity>> GetHabitsByCategoryIdAsync(Guid categoryId, Guid userId);
        Task<IEnumerable<HabitEntity>> GetHabitsAsync(Guid userId, Priority? priority = null);
    }
}
