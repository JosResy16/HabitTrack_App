using HabitTracker.Application.Common.Interfaces;
using HabitTracker.Domain.Entities;
using HabitTracker.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace HabitTracker.Infrastructure.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly HabitTrackDBContext _context;

        public UserRepository(HabitTrackDBContext context)
        {
            _context = context;
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }

        public async Task AddUserAsync(UserEntity user)
        {
            await _context.Users.AddAsync(user);
        }

        public async Task<UserEntity?> GetByEmailAsync(string email)
        {
            return await _context.Users.SingleOrDefaultAsync(u => u.Email == email);
        }

        public async Task<UserEntity?> GetById(Guid id)
        {
            return await _context.Users.FindAsync(id).AsTask();
        }

        public async Task<UserEntity?> GetByRefreshTokenAsync(string refreshToken)
        {
            return await _context.Users
                .SingleOrDefaultAsync(u =>
                    u.RefreshToken == refreshToken &&
                    u.RefreshTokenExpiryTime > DateTime.UtcNow);
        }
    }
}
