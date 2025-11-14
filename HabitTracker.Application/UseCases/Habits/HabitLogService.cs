using HabitTracker.Application.Common.Interfaces;
using HabitTracker.Application.Services;
using HabitTracker.Domain;
using HabitTracker.Domain.Entities;

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
        public async Task<Result<HabitLog?>> GetLogForHabitAndDayAsync(Guid habitId, DateTime day)
        {
            var log = await _habitLogRepository.GetLogForHabitAndDayAsync(habitId, day);
            return Result<HabitLog?>.Success(log);
        }

        public async Task<Result<IEnumerable<HabitLog?>>> GetLogsBetweenDatesAsync(DateTime startDate, DateTime endDate)
        {
            var userId = _userContextService.GetCurrentUserId();
            var logs = await _habitLogRepository.GetLogsBetweenDatesAsync(userId, startDate, endDate);
            return Result<IEnumerable<HabitLog?>>.Success(logs);
        }
        public async Task<Result<IEnumerable<HabitLog?>>> GetLogsByActionTypeAsync(ActionType actionType, DateTime day)
        {
            var userId = _userContextService.GetCurrentUserId();
            IEnumerable<HabitLog?> logs = null;
            if(actionType == ActionType.Completed)
                logs = await _habitLogRepository.GetCompletedLogsAsync(userId, day);
            if(actionType == ActionType.Undone)
                logs = await _habitLogRepository.GetPendingLogsAsync(userId, day);
            else
                logs = new List<HabitLog?>();

            return Result<IEnumerable<HabitLog?>>.Success(logs);
        }
    }
}
