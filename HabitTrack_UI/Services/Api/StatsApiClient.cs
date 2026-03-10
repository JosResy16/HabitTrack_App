using HabitTrack_UI.Models;
using HabitTracker.Application.DTOs;

namespace HabitTrack_UI.Services.Api;
public class StatsApiClient
{
    private readonly ApiClient _api;

    public StatsApiClient(ApiClient api)
    {
        _api = api;
    }

    public async Task<HabitStatsDTO> GetStatsSummary()
    {
        return await _api.GetAsync<HabitStatsDTO>("api/habit-stats/summary");
    }

    public async Task<TodaySummaryDTO> GetTodaySummary()
    {
        return await _api.GetAsync<TodaySummaryDTO>("api/habit-stats/today-summary");
    }

    public async Task<HabitStatsDTO> GetHabitStatsAsync(Guid habtiId)
    {
        return await _api.GetAsync<HabitStatsDTO>($"api/habit-stats/{habtiId}");
    }

    public async Task<UserStatsDTO> GetPastThreeMonthsStats()
    {
        return await _api.GetAsync<UserStatsDTO>("api/habit-stats/past-three-months");
    }

    public async Task<List<DailyActivityDTO>> GetHabitActivityAsync(Guid habitId, DateOnly startDate, DateOnly endDate)
    {
        var url = $"api/habit-stats/{habitId}/activity" +
              $"?startDate={startDate:yyyy-MM-dd}" +
              $"&endDate={endDate:yyyy-MM-dd}";

        return await _api.GetAsync<List<DailyActivityDTO>>(url);
    }

    public async Task<double> GetCompletionRateForHabitAsync(Guid habitId, DateOnly startDate, DateOnly endDate)
    {
        var url = $"api/habit-stats/{habitId}/completion-rate" +
            $"?startDate={startDate:yyyy-MM-dd}" +
            $"?endDate={endDate:yyyy-MM-dd}";

        return await _api.GetAsync<double>(url);
    }
}

