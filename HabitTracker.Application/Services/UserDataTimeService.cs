


using HabitTracker.Application.Common.Interfaces;

namespace HabitTracker.Application.Services;

public class UserDataTimeService : IUserDataTimeService
{
    private readonly IUserRepository _userRepository;
    private readonly IUserContextService _userContextService;

    public UserDataTimeService(IUserRepository userRepository, IUserContextService userContextService)
    {
        _userRepository = userRepository;
        _userContextService = userContextService;
    }

    public DateTime UtcNow => DateTime.UtcNow;

    public async Task<DateOnly> GetTodayAsync()
    {
        var localNow = await GetLocalNowAsync();
        return DateOnly.FromDateTime(localNow);
    }

    public async Task<DateTime> GetLocalNowAsync()
    {
        var userId = _userContextService.GetCurrentUserId();
        if (userId == null)
            return DateTime.UtcNow;

        var user = await _userRepository.GetById(userId.Value);

        if (string.IsNullOrWhiteSpace(user.TimeZoneId))
            return DateTime.UtcNow;

        try
        {
            var timeZone = TimeZoneInfo.FindSystemTimeZoneById(user.TimeZoneId);
            return TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, timeZone);
        }
        catch (Exception ex)
        {
            throw new Exception($"TimeZone ERROR: {user.TimeZoneId} | {ex.Message}");
        }
    }

    public async Task<DateTime> ConvertToLocal(DateTime utcDateTime)
    {
        var userId = _userContextService.GetCurrentUserId();
        if (userId == null)
            return utcDateTime;

        var user = await _userRepository.GetById(userId.Value);

        if (string.IsNullOrWhiteSpace(user.TimeZoneId))
            return utcDateTime;

        try
        {
            var timeZone = TimeZoneInfo.FindSystemTimeZoneById(user.TimeZoneId);

            var utc = utcDateTime.Kind == DateTimeKind.Utc
                ? utcDateTime
                : DateTime.SpecifyKind(utcDateTime, DateTimeKind.Utc);

            return TimeZoneInfo.ConvertTimeFromUtc(utc, timeZone);
        }
        catch (TimeZoneNotFoundException)
        {
            return utcDateTime;
        }
    }

    public DateTime ConvertToUserTime(DateTime utcDateTime, string timeZoneId)
    {
        var timeZone = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
        return TimeZoneInfo.ConvertTimeFromUtc(utcDateTime, timeZone);
    }

    public DateTime GetUserLocalTime(string timeZoneId)
    {
        var timeZone = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
        return TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, timeZone);
    }
}
