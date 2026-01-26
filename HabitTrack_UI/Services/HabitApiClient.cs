using HabitTracker.Application.DTOs;
using HabitTracker.Domain;
using System.Net.Http.Json;

namespace HabitTrack_UI.Services
{
    public class HabitApiClient
    {
        private readonly HttpClient _http;

        public HabitApiClient(HttpClient httpClient)
        {
            _http = httpClient;
        }

        public async Task<List<HabitResponseDTO>> GetHabits(Priority? priority)
        {
            var url = "api/habits";

            if (priority.HasValue)
                url += $"?priority={priority.Value}";

            return await _http.GetFromJsonAsync<List<HabitResponseDTO>>(url) ?? [];
        }

        public async Task<HabitResponseDTO> GetHabitById(Guid id)
        {
            return await _http.GetFromJsonAsync<HabitResponseDTO>($"api/habits/{id}") ?? new();
        }
        
        public async Task<List<HabitResponseDTO>> GetTodayHabits(DateOnly? day)
        {
            var url = "api/habits/today";

            if (day.HasValue)
                url += $"?day={day.Value:yyyy-MM-dd}";

            return await _http.GetFromJsonAsync<List<HabitResponseDTO>>(url) ?? [];
        }

        public async Task<List<HabitHistoryDTO>> GetHabitHistory(Guid id)
        {
            return await _http.GetFromJsonAsync<List<HabitHistoryDTO>>($"api/habits/{id}/history") ?? [];
        }

        public async Task<List<HabitResponseDTO>> GetHabitsByCategory(Guid id)
        {
            return await _http.GetFromJsonAsync<List<HabitResponseDTO>>($"api/habits/category/{id}") ?? [];
        }

        public async Task Create(CreateHabitDTO request)
        {
            var response = await _http.PostAsJsonAsync("api/habits", request);

            await EnsureSuccess(response);
        }

        public async Task Update(Guid id, CreateHabitDTO request)
        {
            var response = await _http.PutAsJsonAsync($"api/habits/{id}", request);

            await EnsureSuccess(response);
        }

        public async Task MarkCompleted(Guid id)
        {
            var response = await _http.PostAsync($"api/habits/{id}/complete", content: null);

            await EnsureSuccess(response);
        }

        public async Task UndoHabitCompletion(Guid id)
        {
            var response = await _http.DeleteAsync($"api/habits/{id}/complete");

            await EnsureSuccess(response);
        }

        public async Task Remove(Guid id)
        {
            var response = await _http.DeleteAsync($"api/habits/{id}");

            await EnsureSuccess(response);
        }

        private static async Task EnsureSuccess(HttpResponseMessage response)
        {
            if (!response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                throw new Exception(content);
            }
        }
    }
}
