using HabitTracker.Application.DTOs;

namespace HabitTrack_UI.Services.Api;
public class StatsApiClient
{
    private readonly ApiClient _api;

    public StatsApiClient(ApiClient api)
    {
        _api = api;
    }

    public async Task<HabitStatsSummaryDTO> GetStatsSummary()
    {
        return await _api.GetAsync<HabitStatsSummaryDTO>("api/habit-stats/summary");
    }

    public async Task<TodaySummaryDTO> GetTodaySummary()
    {
        return await _api.GetAsync<TodaySummaryDTO>("api/habit-stats/today-summary");
    }
}

