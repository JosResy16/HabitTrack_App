using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

        public async Task AddUserAsync(User user)
        {
            if (await _context.Users.AnyAsync(u => u.UserName == user.UserName))
                return;

            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();
        }

        public async Task<User?> GetByUsernameAsync(string username)
        {
            var response = await _context.Users.SingleOrDefaultAsync(u => u.UserName == username);
            if (response is null)
                return null;
            return response;
        }

        public async Task<User?> GetById(Guid id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
                return null;
            return user;
        }
    }
}
