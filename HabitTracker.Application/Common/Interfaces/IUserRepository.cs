using HabitTracker.Domain.Entities;


namespace HabitTracker.Application.Common.Interfaces
{
    public interface IUserRepository
    {
        Task<UserEntity?> GetByEmailAsync(string email);
        Task AddUserAsync(UserEntity user);
        Task<UserEntity?> GetById(Guid id);
        Task SaveChangesAsync();
        Task<UserEntity?> GetByRefreshTokenAsync(string refreshToken);
    }
}
