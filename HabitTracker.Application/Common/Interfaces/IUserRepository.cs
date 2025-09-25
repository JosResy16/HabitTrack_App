using HabitTracker.Domain.Entities;


namespace HabitTracker.Application.Common.Interfaces
{
    public interface IUserRepository
    {
        Task<UserEntity?> GetByUsernameAsync(string username);
        Task AddUserAsync(UserEntity user);
        Task<UserEntity?> GetById(Guid id);
    }
}
