using HabitTracker.Application.DTOs;

namespace HabitTrack_UI.Services.Api
{
    public class CategoriesApiClient
    {
        private readonly ApiClient _apiClient;

        public CategoriesApiClient(ApiClient apiClient)
        {
            _apiClient = apiClient;
        }

        public async Task<List<CategoryResponseDTO>> GetCategories()
        {
            return await _apiClient.GetAsync<List<CategoryResponseDTO>>("api/category") ?? [];
        }

        public async Task<CategoryResponseDTO> Create(CategoryRequestDTO request)
        {
            return await _apiClient.PostAsync<CategoryResponseDTO>("api/category", request);
        }

        public async Task Update(Guid id, CategoryRequestDTO request)
        {
            await _apiClient.PutAsync($"api/category/{id}", request);
        }

        public async Task Remove(Guid id)
        {
            await _apiClient.DeleteAsync($"api/category/{id}");
        }
    }
}
