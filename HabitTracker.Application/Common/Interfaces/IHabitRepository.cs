using HabitTracker.Application.DTOs;
using HabitTracker.Domain.Entities;


namespace HabitTracker.Application.Common.Interfaces
{
    public interface IHabitRepository
    {
        Task<HabitEntity?> GetByIdAsync(Guid id);
        Task<List<HabitEntity>> GetHabitsByUserIdAsync(Guid userId);
        Task AddAsync(HabitEntity habit);
        Task UpdateAsync(HabitEntity habit);
        Task DeleteAsync(Guid id);
    }
}
