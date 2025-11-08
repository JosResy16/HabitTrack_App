using HabitTracker.Application.Services;

namespace HabitTracker.Application.UseCases.Habits
{
    public interface IHabitStatisticsService
    {
        Task<Result<double>> GetCompletionRateAsync(DateTime start, DateTime end);
        Task<Result<double>> GetCompletionRateForHabitAsync(Guid habitId, DateTime start, DateTime end);
        Task<Result<int>> GetCurrentStreakAsync(Guid habitId);
        Task<Result<int>> GetLongestStreakAsync(Guid habitId);
    }
}
