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

        public HabitStatisticsService(IHabitLogRepository habitLogRepository, IUserContextService userContextService)
        {
            _habitLogRepository = habitLogRepository;
            _userContextService = userContextService;
        }

        public async Task<Result<double>> GetCompletionRateAsync(DateTime start, DateTime end)
        {
            var userId = _userContextService.GetCurrentUserId();
            var logs = await _habitLogRepository.GetLogsBetweenDatesAsync(userId, start, end);
            
            return Result<double>.Success(CalculateCompletionRate(logs));
        }

        public async Task<Result<double>> GetCompletionRateForHabitAsync(Guid habitId, DateTime start, DateTime end)
        {
            var logs = await _habitLogRepository.GetLogsByHabitIdAsync(habitId);

            var logsInRange = logs.Where(l => l.Date >= start && l.Date <= end);

            return Result<double>.Success(CalculateCompletionRate(logsInRange));
        }

        public async Task<Result<int>> GetCurrentStreakAsync(Guid habitId)
        {
            //Returns logs ordered by descending
            var logs = await _habitLogRepository.GetLogsByHabitIdAsync(habitId);

            var completedLogs = logs
                .Where(l => l.ActionType == ActionType.Completed);

            return Result<int>.Success(CalculateCurrentStreak(completedLogs));
        }

        public async Task<Result<int>> GetLongestStreakAsync(Guid habitId)
        {
            //Returns logs ordered by descending
            var logs = await _habitLogRepository.GetLogsByHabitIdAsync(habitId);

            //Only logs completed and reorder to ascending
            var completedLogs = logs
                .Where(l => l.ActionType == ActionType.Completed)
                .OrderBy(l => l.Date);

            return Result<int>.Success(CalculateLongestStreak(completedLogs));
        }

        private double CalculateCompletionRate(IEnumerable<HabitLog> logs)
        {
            var total = logs.Count();
            if (total == 0) return 0;

            var completed = logs.Count(l => l.ActionType == ActionType.Completed);
            return (double)completed / total * 100.0;
        }
        private int CalculateCurrentStreak(IEnumerable<HabitLog> completedLogs)
        {
            int streak = 0;
            var expectedDay = DateTime.UtcNow.Date;

            foreach (var log in completedLogs.OrderByDescending(l => l.Date))
            {
                var logDay = log.Date.Date;

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
            DateTime? previousDate = null;

            foreach (var log in completedLogsAsc)
            {
                var day = log.Date.Date;

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
