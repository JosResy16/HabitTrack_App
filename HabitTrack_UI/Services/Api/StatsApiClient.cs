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
}

