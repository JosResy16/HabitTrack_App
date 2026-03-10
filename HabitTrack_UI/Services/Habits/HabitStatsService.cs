using HabitTrack_UI.Services.Api;
using HabitTracker.Application.DTOs;

namespace HabitTrack_UI.Services.Habits;
public class HabitStatsService
{
    private readonly StatsApiClient _api;

    public HabitStatsService(StatsApiClient api)
    {
        _api = api;
    }

    public async Task<HabitStatsDTO?> GetStatsSummary()
    {
        return await _api.GetStatsSummary();
    }

    public async Task<TodaySummaryDTO> GetTodaySummary()
    {
        return await _api.GetTodaySummary();
    }

    public async Task<HabitStatsDTO> GetHabtitStatsAsync(Guid habitId)
    {
        return await _api.GetHabitStatsAsync(habitId);
    }

    public async Task<UserStatsDTO> GetPastThreeMonthsStats()
    {
        return await _api.GetPastThreeMonthsStats();
    }

    public async Task<List<DailyActivityDTO>> GetHabitActivityAsync(Guid habitId, DateOnly startDate, DateOnly endDate)
    {
        return await _api.GetHabitActivityAsync(habitId, startDate, endDate);
    }

    public async Task<double> GetCompletionRateForHabitAsync(Guid habitId, DateOnly startDate, DateOnly endDate)
    {
        return await _api.GetCompletionRateForHabitAsync(habitId, startDate, endDate);
    }
}

