using HabitTracker.Application.Common.Interfaces;
using HabitTracker.Application.Services;
using HabitTracker.Domain;
using HabitTracker.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HabitTracker.Application.UseCases.Habits
{
    public class HabitLogService : IHabitLogService
    {
        private readonly IHabitLogRepository _habitLogRepository;
        private readonly IUserContextService _userContextService;

        public HabitLogService(IHabitLogRepository habitLogRepository, IUserContextService userContextService)
        {
            _habitLogRepository = habitLogRepository;
            _userContextService = userContextService;
        }

        public async Task<Result> AddLogAsync(Guid habitId, ActionType actionType)
        {
            var log = new HabitLog(habitId, DateTime.UtcNow, ActionType.Created);
            await _habitLogRepository.AddAsync(log);
            return Result.Success();
        }

        public async Task<Result<IEnumerable<HabitLog>>> GetLogsByDateAsync(DateTime date)
        {
            var userId = _userContextService.GetCurrentUserId();
            var logs = await _habitLogRepository.GetLogsByDateAsync(userId, date);
            return Result<IEnumerable<HabitLog>>.Success(logs);
        }

        public async Task<Result<IEnumerable<HabitLog>>> GetLogsByHabitAsync(Guid habitId)
        {
            var logs = await _habitLogRepository.GetLogsByHabitIdAsync(habitId);
            return Result<IEnumerable<HabitLog>>.Success(logs);
        }

        public async Task<Result<IEnumerable<HabitLog>>> GetLogsByUserAsync(Guid userId)
        {
            var logs = await _habitLogRepository.GetLogsByUserIdAsync(userId);
            return Result<IEnumerable<HabitLog>>.Success(logs);
        }
    }
}
