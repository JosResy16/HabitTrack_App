using HabitTracker.Application.Common.Interfaces;
using HabitTracker.Application.DTOs;
using HabitTracker.Application.Services;

namespace HabitTracker.Application.UseCases.Habits;
public class HabitStatisticsService : IHabitStatisticsService
{
    private readonly IHabitLogRepository _habitLogRepository;
    private readonly IUserContextService _userContextService;
    private readonly IHabitRepository _habitRepository;
    private readonly IUserDataTimeService _userTimeService;

    public HabitStatisticsService(IHabitLogRepository habitLogRepository, IUserContextService userContextService, IHabitRepository habitRepository, IUserDataTimeService userTimeService)
    {
        _habitLogRepository = habitLogRepository;
        _userContextService = userContextService;
        _habitRepository = habitRepository;
        _userTimeService = userTimeService;
    }

    public async Task<Result<double>> GetCompletionRateAsync(DateOnly start, DateOnly end)
    {
        if (start > end)
            return Result<double>.Failure("Start date cannot be greater than end date.");

        var userId = _userContextService.GetCurrentUserId();
        var logs = await _habitLogRepository.GetLogsBetweenDatesAsync(userId.Value, start, end);
            
        return Result<double>.Success(CalculateCompletionRate(logs, start, end));
    }

    public async Task<Result<double>> GetCompletionRateForHabitAsync(Guid habitId, DateOnly start, DateOnly end)
    {
        if (start > end)
            return Result<double>.Failure("Start date cannot be greater than end date.");

        var userId = _userContextService.GetCurrentUserId();
        var logs = await _habitLogRepository.GetLogsByHabitIdAsync(userId.Value, habitId);

        var logsInRange = logs.Where(l => l.Date >= start && l.Date <= end);

        return Result<double>.Success(CalculateCompletionRate(logsInRange, start, end));
    }

    public async Task<Result<int>> GetCurrentStreakAsync(Guid habitId)
    {
        //Returns logs ordered by descending
        var userId = _userContextService.GetCurrentUserId();
        var logs = await _habitLogRepository.GetLogsByHabitIdAsync(userId.Value, habitId);

        var completedLogs = logs
            .Where(l => l.ActionType == ActionType.Completed);

        return Result<int>.Success(CalculateCurrentStreak(completedLogs));
    }

    public async Task<Result<int>> GetLongestStreakAsync(Guid habitId)
    {
        //Returns logs ordered by descending
        var userId = _userContextService.GetCurrentUserId();
        var logs = await _habitLogRepository.GetLogsByHabitIdAsync(userId.Value, habitId);

        //Only logs completed and reorder to ascending
        var completedLogs = logs
            .Where(l => l.ActionType == ActionType.Completed)
            .OrderBy(l => l.Date)
            .ThenBy(l => l.CreatedAtUtc);

        return Result<int>.Success(CalculateLongestStreak(completedLogs));
    }

    public async Task<Result<HabitStatsDTO>> GetHabtitStatsAsync(Guid habitId)
    {
        var userId = _userContextService.GetCurrentUserId();
        var logs = await _habitLogRepository.GetLogsByHabitIdAsync(userId.Value, habitId);

        if (!logs.Any())
            return Result<HabitStatsDTO>.Success(new HabitStatsDTO());

        var today = await _userTimeService.GetTodayAsync();

        var completedLogs = logs
            .GroupBy(l => l.Date)
            .Select(g =>
            {
                var stateLog = g
                    .Where(l => l.ActionType == ActionType.Completed
                        || l.ActionType == ActionType.Undone)
                    .OrderBy(l => l.CreatedAtUtc)
                    .LastOrDefault();

                return stateLog;
            })
            .Where(l => l != null && l.ActionType == ActionType.Completed)
            .Select(l => l!)
            .OrderBy(l => l.Date)
            .ToList();

        var currentStreak = CalculateCurrentStreak(completedLogs);
        var longestStreak = CalculateLongestStreak(completedLogs);

        //CompletionRate
        var end = DateOnly.FromDateTime(_userTimeService.UtcNow);
        var start = end.AddDays(-6);
        var completionRate = CalculateCompletionRate(
                logs.Where(l => l.Date >= start && l.Date <= end), 
                start, end);


        //TotalTrackedDays
        var habit = await _habitRepository.GetByIdAsync(habitId);
        var createdDate = DateOnly.FromDateTime(habit.CreatedAt);
        var totalTrackedDays = today.DayNumber - createdDate.DayNumber + 1;

        //DaysSinceLastCompletion
        var lastCompletion = logs
            .Where(l => l.ActionType == ActionType.Completed)
            .OrderByDescending(l => l.Date)
            .FirstOrDefault();

        int daysSinceLastCompletion;

        if (lastCompletion == null)
        {
            daysSinceLastCompletion = -1;
        }
        else
        {
            daysSinceLastCompletion = today.DayNumber - lastCompletion.Date.DayNumber;
        }

        return Result<HabitStatsDTO>.Success(new HabitStatsDTO
        {
            TotalCompletion = completedLogs.Count,
            CurrentStreak = currentStreak,
            LongestStreak = longestStreak,
            CompletionRate = completionRate,
            TotalTrackedDays = totalTrackedDays,
            DaysSinceLastCompletion = daysSinceLastCompletion,
        });
    }

    public async Task<Result<TodaySummaryDTO>> GetTodaySummaryAsync()
    {
        var userId = _userContextService.GetCurrentUserId().Value;
        var today = await _userTimeService.GetTodayAsync();

        var habits = await _habitRepository.GetActiveHabits(userId);
        var todayHabits = new List<HabitEntity>();

        foreach (var habit in habits)
        {
            if (await HabitAppliesToDate(habit, today))
                todayHabits.Add(habit);
        }

        var todayLogs = await _habitLogRepository.GetLogsByDateAsync(userId, today);

        var completedToday = todayLogs
            .GroupBy(l => l.HabitId)
            .Select(g => g.OrderByDescending(l => l.CreatedAtUtc).First())
            .Count(l => l.ActionType == ActionType.Completed);

        var firstCompletion = todayLogs
            .Where(l => l.ActionType == ActionType.Completed)
            .OrderBy(l => l.CreatedAtUtc)
            .FirstOrDefault();

        var streak = await CalculateCurrentStreakAsync(userId);

        return Result<TodaySummaryDTO>.Success(new TodaySummaryDTO
        {
            TotalHabitsToday = todayHabits.Count,
            CompletedHabitsToday = completedToday,
            CurrentStreak = streak,
            FirstCompletionAt = firstCompletion == null ? null : await _userTimeService.ConvertToLocal(firstCompletion.CreatedAtUtc),
        });
    }

    public async Task<Result<UserStatsDTO>> GetUserStatsAsync(DateOnly startDate, DateOnly endDate)
    {
        var userId = _userContextService.GetCurrentUserId().Value;

        var today = await _userTimeService.GetTodayAsync();

        var totalDaysThisMonth = DateTime.DaysInMonth(today.Year, today.Month);

        var totalActiveHabits = await _habitRepository.GetTotalActiveHabitsAsync(userId);

        var totalCompletions = await _habitLogRepository.GetTotalCompletionsAsync(userId);

        var activeDaysThisMonth = await _habitLogRepository.GetActiveDaysCountAsync(userId, startDate, today);

        var activeDates = await _habitLogRepository.GetDistinctActiveDatesAsync(userId);

        var monthlyActivityRaw = await _habitLogRepository.GetDailyActivityAsync(userId, startDate, endDate);
        var normalizedMonthlyActivity = NormalizeMonthlyActivity(monthlyActivityRaw, startDate, endDate);

        //habitBreakdown current month
        var habitBreakdown = await _habitRepository.GetHabitPerformanceAsync(userId, DateOnly.FromDateTime(new DateTime(today.Year, today.Month, 1)), today);

        var currentStreak = CalculateCurrentStreak(activeDates, today);
        var longestStreak = CalculateLongestStreak(activeDates);

        var pausedHabits = await _habitRepository.GetPausedHabitsAsync(userId);

        var daysSoFar = today.Day;
        var activeDaysPercentage =
            daysSoFar == 0
            ? 0
            : Math.Round(
                (double)activeDaysThisMonth / daysSoFar * 100,
                1);

        var dto = new UserStatsDTO
        {
            TotalActiveHabits = totalActiveHabits,
            TotalCompletions = totalCompletions,
            LongestStreak = longestStreak,
            CurrentStreak = currentStreak,
            ActiveDaysPercentageThisMonth = activeDaysPercentage,
            ActiveDaysThisMonth = activeDaysThisMonth,
            TotalDaysThisMonth = totalDaysThisMonth,
            MonthlyActivity = normalizedMonthlyActivity,
            HabitBreakdown = habitBreakdown,
            TotalPausedHabits = pausedHabits.Count(),
        };

        return Result<UserStatsDTO>.Success(dto);
    }

    public async Task<Result<List<DailyActivityDTO>>> GetHabitActivityAsync(Guid habitId, DateOnly startDate, DateOnly endDate)
    {
        var userId = _userContextService.GetCurrentUserId().Value;

        var monthlyActivityRaw = await _habitLogRepository.GetHabitActivityAsync(userId, habitId, startDate, endDate);
        var normalizedMonthlyActivity = NormalizeMonthlyActivity(monthlyActivityRaw, startDate, endDate);

        return Result<List<DailyActivityDTO>>.Success(normalizedMonthlyActivity);
    }



    private List<DailyActivityDTO> NormalizeMonthlyActivity(List<DailyActivityDTO> rawData, DateOnly startDate, DateOnly endDate)
    {
        var normalizedData = new List<DailyActivityDTO>();

        for (var date = startDate; date <= endDate; date = date.AddDays(1))
        {
            var existing = rawData.FirstOrDefault(x => x.Date == date);

            normalizedData.Add(new DailyActivityDTO
            {
                Date = date,
                CompletionsCount = existing?.CompletionsCount ?? 0,
            });
        }

        return normalizedData;
    }

    private async Task<bool> HabitAppliesToDate(HabitEntity habit, DateOnly date)
    {
        var createdLocal = await _userTimeService.ConvertToLocal(habit.CreatedAt);
        var createdDate = DateOnly.FromDateTime(createdLocal);

        if (date < createdDate)
            return false;

        if (habit.RepeatPeriod == null || habit.RepeatInterval == null)
            return true;

        var daysDifference = date.DayNumber - createdDate.DayNumber;

        return habit.RepeatPeriod switch
        {
            Period.Daily =>
                daysDifference % habit.RepeatInterval.Value == 0,

            Period.Weekly =>
                (daysDifference / 7) % habit.RepeatInterval.Value == 0
                && date.DayOfWeek == createdDate.DayOfWeek,

            _ => false
        };
    }

    private static double CalculateCompletionRate(IEnumerable<HabitLog> logs, DateOnly start, DateOnly end)
    {
        var logsByDay = logs
            .GroupBy(l => l.Date)
            .ToDictionary(
                g => g.Key,
                g => g
                    .Where(l => l.ActionType == ActionType.Completed
                                || l.ActionType == ActionType.Undone)
                    .OrderByDescending(l => l.CreatedAtUtc)
                    .ThenByDescending(l => l.Id)
                    .FirstOrDefault()
            );

        var totalDays = (end.DayNumber - start.DayNumber) + 1;
        var completedDays = 0;

        for (var date = start; date <= end; date = date.AddDays(1))
        {
            if (logsByDay.TryGetValue(date, out var stateLog) &&
                stateLog?.ActionType == ActionType.Completed)
            {
                completedDays++;
            }
        }

        return totalDays == 0
            ? 0
            : Math.Round((double)completedDays / totalDays * 100.0);
    }

    private static int CalculateCurrentStreak(List<DateOnly> activeDates, DateOnly today)
    {
        if (!activeDates.Any())
            return 0;

        var datesSet = activeDates.ToHashSet();

        if (!datesSet.Contains(today))
        {
            today = today.AddDays(-1);
        }

        int streak = 0;
        var currentDate = today;

        while (datesSet.Contains(currentDate))
        {
            streak++;
            currentDate = currentDate.AddDays(-1);
        }

        return streak;
    }

    private static int CalculateLongestStreak(List<DateOnly> activeDates)
    {
        if (!activeDates.Any())
            return 0;

        activeDates = activeDates.OrderBy(d => d).ToList();

        int longest = 1;
        int current = 1;

        for (int i = 1; i < activeDates.Count; i++)
        {
            if (activeDates[i] == activeDates[i - 1].AddDays(1))
            {
                current++;
                longest = Math.Max(longest, current);
            }
            else
            {
                current = 1;
            }
        }

        return longest;
    }


    private int CalculateCurrentStreak(IEnumerable<HabitLog> completedLogs)
    {
        int streak = 0;

        var today = DateOnly.FromDateTime(_userTimeService.UtcNow);
        var expectedDay = today;

        var orderedLogs = completedLogs
            .OrderByDescending(l => l.Date)
            .ToList();

        if (!orderedLogs.Any())
            return 0;

        if (orderedLogs.First().Date < today)
            expectedDay = today.AddDays(-1);

        foreach (var log in orderedLogs)
        {
            if (log.Date == expectedDay)
            {
                streak++;
                expectedDay = expectedDay.AddDays(-1);
            }
            else if (log.Date < expectedDay)
            {
                break;
            }
        }

        return streak;
    }

    private async Task<int> CalculateCurrentStreakAsync(Guid userId)
    {
        var today = await _userTimeService.GetTodayAsync();
        var streak = 0;

        for (var day = today; ; day = day.AddDays(-1))
        {
            var logs = await _habitLogRepository.GetLogsByDateAsync(userId, day);

            var hasCompletion = logs
                .GroupBy(l => l.HabitId)
                .Select(g => g.OrderByDescending(l => l.CreatedAtUtc).First())
                .Any(l => l.ActionType == ActionType.Completed);

            if (!hasCompletion)
                break;

            streak++;
        }

        return streak;
    }

    private static int CalculateLongestStreak(IEnumerable<HabitLog> completedLogsAsc)
    {
        int current = 0;
        int longest = 0;
        DateOnly? previousDate = null;

        foreach (var log in completedLogsAsc)
        {
            var day = log.Date;

            if (previousDate == null)
            {
                current = 1;
            }
            else if (day == previousDate.Value.AddDays(1))
            {
                current++;
            }
            else
            {
                current = 1;
            }

            previousDate = day;
            longest = Math.Max(longest, current);
        }

        return longest;
    }
}

