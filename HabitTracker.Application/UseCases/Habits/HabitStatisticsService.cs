using HabitTracker.Application.Common.Interfaces;
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

        public HabitStatisticsService(IHabitLogRepository habitLogRepository, IUserContextService userContextService, IDateTimeProvider dateTimeProvider)
        {
            _habitLogRepository = habitLogRepository;
            _userContextService = userContextService;
            _dateTimeProvider = dateTimeProvider;
        }

        public async Task<Result<double>> GetCompletionRateAsync(DateOnly start, DateOnly end)
        {
            if (start > end)
                return Result<double>.Failure("Start date cannot be greater than end date.");

            var userId = _userContextService.GetCurrentUserId();
            var logs = await _habitLogRepository.GetLogsBetweenDatesAsync(userId, start, end);
            
            return Result<double>.Success(CalculateCompletionRate(logs));
        }

        public async Task<Result<double>> GetCompletionRateForHabitAsync(Guid habitId, DateOnly start, DateOnly end)
        {
            if (start > end)
                return Result<double>.Failure("Start date cannot be greater than end date.");

            var userId = _userContextService.GetCurrentUserId();
            var logs = await _habitLogRepository.GetLogsByHabitIdAsync(userId, habitId);

            var logsInRange = logs.Where(l => l.Date >= start && l.Date <= end);

            return Result<double>.Success(CalculateCompletionRate(logsInRange));
        }

        public async Task<Result<int>> GetCurrentStreakAsync(Guid habitId)
        {
            //Returns logs ordered by descending
            var userId = _userContextService.GetCurrentUserId();
            var logs = await _habitLogRepository.GetLogsByHabitIdAsync(userId, habitId);

            var completedLogs = logs
                .Where(l => l.ActionType == ActionType.Completed);

            return Result<int>.Success(CalculateCurrentStreak(completedLogs));
        }

        public async Task<Result<int>> GetLongestStreakAsync(Guid habitId)
        {
            //Returns logs ordered by descending
            var userId = _userContextService.GetCurrentUserId();
            var logs = await _habitLogRepository.GetLogsByHabitIdAsync(userId, habitId);

            //Only logs completed and reorder to ascending
            var completedLogs = logs
                .Where(l => l.ActionType == ActionType.Completed)
                .OrderBy(l => l.Date)
                .ThenBy(l => l.CreatedAt);

            return Result<int>.Success(CalculateLongestStreak(completedLogs));
        }

        private double CalculateCompletionRate(IEnumerable<HabitLog> logs)
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
        private int CalculateLongestStreak(IEnumerable<HabitLog> completedLogsAsc)
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
