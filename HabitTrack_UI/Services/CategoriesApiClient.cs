using HabitTracker.Application.DTOs;
using System.Net.Http.Json;

namespace HabitTrack_UI.Services
{
    public class CategoriesApiClient
    {
        private readonly HttpClient _http;

        public CategoriesApiClient(HttpClient httpClient)
        {
            _http = httpClient;
        }

        public async Task<List<CategoryResponseDTO>> GetCategories()
        {
            return await _http.GetFromJsonAsync<List<CategoryResponseDTO>>("api/category") ?? [];
        }

        public async Task Create(CategoryRequestDTO request)
        {
            var response = await _http.PostAsJsonAsync("api/category", request);
            await EnsureSuccess(response);
        }

        public async Task Update(Guid id, CategoryRequestDTO request)
        {
            var response = await _http.PutAsJsonAsync($"api/category/{id}", request);
            await EnsureSuccess(response);
        }

        public async Task Remove(Guid id)
        {
            var response = await _http.DeleteAsync($"api/category/{id}");
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
