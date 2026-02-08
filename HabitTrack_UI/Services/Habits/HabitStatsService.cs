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

    public async Task<HabitStatsSummaryDTO?> GetStatsSummary()
    {
        return await _api.GetStatsSummary();
    }

    public async Task<TodaySummaryDTO> GetTodaySummary()
    {
        return await _api.GetTodaySummary();
    }
}

