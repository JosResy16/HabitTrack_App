using HabitTracker.Application.DTOs;
using HabitTracker.Domain.Entities;


namespace HabitTracker.Application.Common.Interfaces
{
    public interface IHabitRepository
    {
        Task<Habit?> GetByIdAsync(Guid id);
        Task<List<Habit>> GetHabitsByUserIdAsync(Guid userId);
        Task AddAsync(Habit habit);
        Task UpdateAsync(Habit habit);
        Task DeleteAsync(Guid id);
    }
}
