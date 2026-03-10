namespace HabitTracker.Application.Services;
public interface IUserDataTimeService
{
    public DateTime UtcNow { get;}
    Task<DateTime> GetLocalNowAsync();
    Task<DateOnly> GetTodayAsync();
    public DateTime GetUserLocalTime(string timeZoneId);
    public DateTime ConvertToUserTime(DateTime utcDateTime, string timeZoneId);
    Task<DateTime> ConvertToLocal(DateTime utcDateTime);
}
