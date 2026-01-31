using HabitTracker.Application.DTOs;
using HabitTracker.Domain;

namespace HabitTrack_UI.Services.Api;
public class HabitApiClient
{
    private readonly ApiClient _api;

    public HabitApiClient(ApiClient apiClient)
    {
        _api = apiClient;
    }

    public async Task<List<HabitResponseDTO>> GetHabits(Priority? priority)
    {
        var url = "api/habits";

        if (priority.HasValue)
            url += $"?priority={priority.Value}";

        return await _api.GetAsync<List<HabitResponseDTO>>(url) ?? [];
    }

    public async Task<HabitResponseDTO?> GetHabitById(Guid id)
    {
        return await _api.GetAsync<HabitResponseDTO>($"api/habits/{id}");
    }

    public async Task<List<HabitResponseDTO>> GetTodayHabits(DateOnly? day)
    {
        var url = "api/habits/today";

        if (day.HasValue)
            url += $"?day={day:yyyy-MM-dd}";

        return await _api.GetAsync<List<HabitResponseDTO>>(url) ?? [];
    }

    public async Task<List<HabitHistoryDTO>> GetHabitHistory(Guid id)
    {
        return await _api.GetAsync<List<HabitHistoryDTO>>(
            $"api/habits/{id}/history") ?? [];
    }

    public async Task<HabitResponseDTO?> Create(CreateHabitDTO request)
    {
        return await _api.PostAsync<HabitResponseDTO>("api/habits", request);
    }
    public async Task Update(Guid id, CreateHabitDTO request)
    {
        await _api.PutAsync($"api/habits/{id}", request);
    }

    public async Task MarkCompleted(Guid id)
    {
        await _api.PostAsync<object>($"api/habits/{id}/complete", null);
    }

    public async Task UndoHabitCompletion(Guid id)
    {
        await _api.DeleteAsync($"api/habits/{id}/complete");
    }

    public async Task Remove(Guid id)
    {
        await _api.DeleteAsync($"api/habits/{id}");
    }
}

