
using HabitTracker.Domain;
using HabitTracker.Domain.Entities;



namespace HabitTracker.Application.Common.Interfaces
{
    public interface IHabitRepository
    {
        Task SaveChangesAsync();
        Task<HabitEntity?> GetByIdAsync(Guid id);
        Task AddAsync(HabitEntity habit);
        Task<IEnumerable<HabitEntity>> GetHabitsByCategoryIdAsync(Guid categoryId, Guid userId);
        Task<IEnumerable<HabitEntity>> GetHabitsAsync(Guid userId, Priority? priority = null);
        Task<HabitEntity?> GetByTitleAsync(Guid userId, string title);
    }
}
