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
        private readonly IDateTimeProvider _dateTimeProvider;

        public HabitLogService(IHabitLogRepository habitLogRepository, IUserContextService userContextService, IDateTimeProvider dateTimeProvider)
        {
            _habitLogRepository = habitLogRepository;
            _userContextService = userContextService;
            _dateTimeProvider = dateTimeProvider;
        }

        public async Task<Result> AddLogAsync(Guid habitId, ActionType actionType)
        {
            var now = _dateTimeProvider.UtcNow;
            var log = new HabitLog(
                habitId, 
                DateOnly.FromDateTime(now), 
                actionType,
                now);

            await _habitLogRepository.AddAsync(log);

            return Result.Success();
        }

        public async Task<Result<IEnumerable<HabitLog>>> GetLogsByDateAsync(DateOnly date)
        {
            var userId = _userContextService.GetCurrentUserId();
            var logs = await _habitLogRepository.GetLogsByDateAsync(userId.Value, date);
            return Result<IEnumerable<HabitLog>>.Success(logs);
        }

        public async Task<Result<IEnumerable<HabitLog>>> GetLogsByHabitAsync(Guid habitId)
        {
            var userId = _userContextService.GetCurrentUserId();
            var logs = await _habitLogRepository.GetLogsByHabitIdAsync(userId.Value, habitId);
            return Result<IEnumerable<HabitLog>>.Success(logs);
        }

        public async Task<Result<IEnumerable<HabitLog>>> GetLogsByUserAsync(Guid userId)
        {
            var logs = await _habitLogRepository.GetLogsByUserIdAsync(userId);
            return Result<IEnumerable<HabitLog>>.Success(logs);
        }
        public async Task<Result<HabitLog?>> GetLogForHabitAndDayAsync(Guid habitId, DateOnly day)
        {
            var log = await _habitLogRepository.GetLogForHabitAndDayAsync(habitId, day);
            return Result<HabitLog?>.Success(log);
        }

        public async Task<Result<IEnumerable<HabitLog>>> GetLogsBetweenDatesAsync(DateOnly startDate, DateOnly endDate)
        {
            var userId = _userContextService.GetCurrentUserId();
            var logs = await _habitLogRepository.GetLogsBetweenDatesAsync(userId.Value, startDate, endDate);

            return Result<IEnumerable<HabitLog>>.Success(logs);
        }

        public async Task<Result<IEnumerable<HabitLog>>> GetLogsByActionTypeAsync(ActionType actionType, DateOnly day)
        {
            var userId = _userContextService.GetCurrentUserId();
            IEnumerable<HabitLog> logs = actionType switch
            {
                ActionType.Completed => await _habitLogRepository.GetCompletedLogsAsync(userId.Value, day),
                ActionType.Undone => await _habitLogRepository.GetPendingLogsAsync(userId.Value, day),
                _ => Enumerable.Empty<HabitLog>()
            };

            return Result<IEnumerable<HabitLog>>.Success(logs);
        }

        public async Task<Result<HabitLog?>> GetLastLogForDateAsync(Guid userId, Guid habitId, DateOnly date)
        {
            var id = _userContextService.GetCurrentUserId();
            var log = await _habitLogRepository.GetLastLogForDateAsync(id.Value, habitId, date);

            return Result<HabitLog?>.Success(log);
        }
    }
}
