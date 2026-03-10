using HabitTracker.Application.Common.Interfaces;
using HabitTracker.Application.Services;
using HabitTracker.Domain;
using HabitTracker.Domain.Entities;

namespace HabitTracker.Application.UseCases.Habits;
public class HabitLogService : IHabitLogService
{
    private readonly IHabitLogRepository _habitLogRepository;
    private readonly IUserContextService _userContextService;
    private readonly IUserDataTimeService _userTimeService;

    public HabitLogService(IHabitLogRepository habitLogRepository, IUserContextService userContextService, IUserDataTimeService userTimeService)
    {
        _habitLogRepository = habitLogRepository;
        _userContextService = userContextService;
        _userTimeService = userTimeService;
    }

    public async Task<Result> AddLogAsync(Guid habitId, ActionType actionType)
    {
        var now = _userTimeService.UtcNow;
        var today = await _userTimeService.GetTodayAsync();

        var log = new HabitLog(
            habitId, 
            today, 
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

    public async Task<Result<HabitLog?>> GetLastLogForDateAsync(Guid habitId, DateOnly date)
    {
        var userId = _userContextService.GetCurrentUserId().Value;
        var log = await _habitLogRepository.GetLastLogForDateAsync(userId, habitId, date);

        return Result<HabitLog?>.Success(log);
    }

    public async Task<Result<HabitLog?>> GetFinalStateLogForDay(Guid habitId, DateOnly date)
    {
        var userId = _userContextService.GetCurrentUserId().Value;
        var logsForDay = await _habitLogRepository.GetLogsBetweenDatesForHabitAsync(userId, habitId, date);

        return Result<HabitLog?>.Success(logsForDay
            .Where(l => l.ActionType == ActionType.Completed
                        || l.ActionType == ActionType.Undone)
            .OrderByDescending(l => l.CreatedAtUtc)
            .ThenByDescending(l => l.Id)
            .FirstOrDefault()
        );
    }
}
