using HabitTracker.Domain.Entities;


namespace HabitTracker.Application.Common.Interfaces
{
    public interface IUserRepository
    {
        Task<User?> GetByUsernameAsync(string username);
        Task AddUserAsync(User user);
        Task<User?> GetById(Guid id);
    }
}
