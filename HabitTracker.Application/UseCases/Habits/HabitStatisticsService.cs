using HabitTracker.Application.Common.Interfaces;
using HabitTracker.Application.DTOs;
using HabitTracker.Application.Services;
using HabitTracker.Domain;
using HabitTracker.Domain.Entities;
namespace HabitTracker.Application.UseCases.Habits
{
    public class HabitStatisticsService : IHabitStatisticsService
    {
        private readonly IHabitLogRepository _habitLogRepository;
        private readonly IUserContextService _userContextService;
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly IHabitRepository _habitRepository;

        public HabitStatisticsService(IHabitLogRepository habitLogRepository, IUserContextService userContextService, IDateTimeProvider dateTimeProvider, IHabitRepository habitRepository)
        {
            _habitLogRepository = habitLogRepository;
            _userContextService = userContextService;
            _dateTimeProvider = dateTimeProvider;
            _habitRepository = habitRepository;
        }

        public async Task<Result<double>> GetCompletionRateAsync(DateOnly start, DateOnly end)
        {
            if (start > end)
                return Result<double>.Failure("Start date cannot be greater than end date.");

            var userId = _userContextService.GetCurrentUserId();
            var logs = await _habitLogRepository.GetLogsBetweenDatesAsync(userId.Value, start, end);
            
            return Result<double>.Success(CalculateCompletionRate(logs));
        }

        public async Task<Result<double>> GetCompletionRateForHabitAsync(Guid habitId, DateOnly start, DateOnly end)
        {
            if (start > end)
                return Result<double>.Failure("Start date cannot be greater than end date.");

            var userId = _userContextService.GetCurrentUserId();
            var logs = await _habitLogRepository.GetLogsByHabitIdAsync(userId.Value, habitId);

            var logsInRange = logs.Where(l => l.Date >= start && l.Date <= end);

            return Result<double>.Success(CalculateCompletionRate(logsInRange));
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
                .ThenBy(l => l.CreatedAt);

            return Result<int>.Success(CalculateLongestStreak(completedLogs));
        }

        public async Task<Result<HabitStatsDTO>> GetHabtitStatsAsync(Guid habitId)
        {
            var userId = _userContextService.GetCurrentUserId();
            var logs = await _habitLogRepository.GetLogsByHabitIdAsync(userId.Value, habitId);
            var today = DateOnly.FromDateTime(_dateTimeProvider.UtcNow);

            if (!logs.Any())
                return Result<HabitStatsDTO>.Success(new HabitStatsDTO());

            var completedLogs = logs
                .Where(l => l.ActionType == ActionType.Completed)
                .OrderBy(l => l.Date)
                .ThenBy(l => l.CreatedAt)
                .ToList();

            var currentStreak = CalculateCurrentStreak(completedLogs);
            var longestStreak = CalculateLongestStreak(completedLogs);

            //CompletionRate
            var end = DateOnly.FromDateTime(_dateTimeProvider.UtcNow);
            var start = end.AddDays(-6);
            var completionRate = CalculateCompletionRate(logs.Where(l => l.Date >= start && l.Date <= end));
            

            //TotalTrackedDays
            var createdDate = logs.FirstOrDefault(l => l.ActionType == ActionType.Created)?.Date;
            var totalTrackedDays = today.DayNumber - (createdDate.Value.DayNumber + 1);

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

        public async Task<Result<UserStatsDTO>> GetSummaryAsync()
        {
            var userId = _userContextService.GetCurrentUserId();

            var logs = await _habitLogRepository.GetLogsByUserIdAsync(userId.Value);

            if (!logs.Any())
            {
                return Result<UserStatsDTO>.Success(new UserStatsDTO());
            }

            var completedLogs = logs
                .Where(l => l.ActionType == ActionType.Completed)
                .OrderBy(l => l.Date)
                .ThenBy(l => l.CreatedAt)
                .ToList();

            var end = DateOnly.FromDateTime(_dateTimeProvider.UtcNow);
            var start = end.AddDays(-6);

            var weeklyRate = CalculateCompletionRate(logs.Where(l => l.Date >= start && l.Date <= end));

            return Result<UserStatsDTO>.Success(new UserStatsDTO
            {
                WeeklyAverage = weeklyRate,
                LongestStreak = CalculateLongestStreak(completedLogs),
                TotalCompletions = completedLogs.Count
            });
        }

        public async Task<Result<TodaySummaryDTO>> GetTodaySummaryAsync()
        {
            var userId = _userContextService.GetCurrentUserId();
            var today = _dateTimeProvider.UtcNow;

            var habits = await _habitRepository.GetHabitsAsync(userId.Value);
            var todayHabits = habits.Where(h  => HabitAppliesToDate(h, DateOnly.FromDateTime(today))).ToList();

            var todayLogs = await _habitLogRepository.GetLogsByDateAsync(userId.Value, DateOnly.FromDateTime(today));

            var completedToday = todayLogs
                .GroupBy(l => l.HabitId)
                .Select(g => g.OrderByDescending(l => l.CreatedAt).First())
                .Count(l => l.ActionType == ActionType.Completed);

            var firstCompletion = todayLogs
                .Where(l => l.ActionType == ActionType.Completed)
                .OrderBy(l => l.CreatedAt)
                .FirstOrDefault();

            var streak = await CalculateCurrentStreakAsync(userId.Value);

            return Result<TodaySummaryDTO>.Success(new TodaySummaryDTO
            {
                TotalHabitsToday = todayHabits.Count,
                CompletedHabitsToday = completedToday,
                CurrentStreak = streak,
                FirstCompletionAt = firstCompletion?.CreatedAt
            });
        }

        private static bool HabitAppliesToDate(HabitEntity habit, DateOnly date)
        {
            var createdDate = DateOnly.FromDateTime(habit.CreatedAt);

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

        private static double CalculateCompletionRate(IEnumerable<HabitLog> logs)
        {
            var lastLogPerDay = logs
                .GroupBy(l => l.Date)
                .Select(g => g
                    .OrderByDescending(l => l.CreatedAt)
                    .ThenByDescending(l => l.Id)
                    .First()
                ).ToList();


            var totalDays = lastLogPerDay.Count;

            if (totalDays == 0)
                return 0;

            var completedDays = lastLogPerDay
                .Count(l => l.ActionType == ActionType.Completed);

            return (double)completedDays / totalDays * 100.0;
        }

        private int CalculateCurrentStreak(IEnumerable<HabitLog> completedLogs)
        {
            int streak = 0;
            var expectedDay = DateOnly.FromDateTime(_dateTimeProvider.UtcNow);

            foreach (var log in completedLogs.OrderByDescending(l => l.Date))
            {
                var logDay = log.Date;

                if (logDay == expectedDay)
                {
                    streak++;
                    expectedDay = expectedDay.AddDays(-1);
                }
                else if (logDay < expectedDay)
                {
                    break;
                }
            }

            return streak;
        }

        private async Task<int> CalculateCurrentStreakAsync(Guid userId)
        {
            var today = DateOnly.FromDateTime(_dateTimeProvider.UtcNow);
            var streak = 0;

            for (var day = today; ; day = day.AddDays(-1))
            {
                var logs = await _habitLogRepository.GetLogsByDateAsync(userId, day);

                var hasCompletion = logs
                    .GroupBy(l => l.HabitId)
                    .Select(g => g.OrderByDescending(l => l.CreatedAt).First())
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
}
