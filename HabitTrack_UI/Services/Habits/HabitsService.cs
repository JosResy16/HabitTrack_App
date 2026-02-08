using HabitTrack_UI.Services.Api;
using HabitTracker.Application.DTOs;
using HabitTracker.Domain;
using HabitTracker.Shared.DTOs;

namespace HabitTrack_UI.Services.Habits;
public class HabitsService
{
    private readonly HabitApiClient _api;

    public HabitsService(HabitApiClient habitApiClient)
    {
        _api = habitApiClient;
    }

    public async Task<HabitResponseDTO?> Create(CreateHabitDTO request)
    {
        return await _api.Create(request);
    }

    public async Task<List<HabitResponseDTO>> GetHabits(Priority? priority)
    {
        return await _api.GetHabits(priority);
    }

    public async Task<List<HabitTodayDTO>> GetTodayHabits(DateOnly? day)
    {
        return await _api.GetTodayHabits(day);
    }

    public async Task MarkCompleted(Guid id)
    {
        await _api.MarkCompleted(id);
    }

    public async Task UndoHabitCompletion(Guid id)
    {
        await _api.UndoHabitCompletion(id);
    }
}
