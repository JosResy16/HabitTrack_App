using HabitTracker.Application.DTOs;
using HabitTracker.Application.Services;

namespace HabitTracker.Application.UseCases.Habits
{
    public interface IHabitStatisticsService
    {
        Task<Result<double>> GetCompletionRateAsync(DateOnly start, DateOnly end);
        Task<Result<double>> GetCompletionRateForHabitAsync(Guid habitId, DateOnly start, DateOnly end);
        Task<Result<int>> GetCurrentStreakAsync(Guid habitId);
        Task<Result<int>> GetLongestStreakAsync(Guid habitId);

        Task<Result<UserStatsDTO>> GetSummaryAsync();
        Task<Result<TodaySummaryDTO>> GetTodaySummaryAsync();
        Task<Result<HabitStatsDTO>> GetHabtitStatsAsync(Guid habitId);
    }
}
