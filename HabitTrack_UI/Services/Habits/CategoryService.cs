using HabitTrack_UI.Models;
using HabitTrack_UI.Services.Api;
using HabitTracker.Application.DTOs;

namespace HabitTrack_UI.Services.Habits;
public class CategoryService
{
    private readonly CategoriesApiClient _api;

    public CategoryService(CategoriesApiClient categoryApiClient)
    {
        _api = categoryApiClient;
    }

    public async Task<List<CategoryModel>> GetCategories()
    {
        var response = await _api.GetCategories();
        return response.
            Select(c => new CategoryModel
            {
                Id = c.Id.ToString(),
                Title = c.Title,
            }).ToList();
    }

    public async Task<CategoryResponseDTO> Create(CategoryRequestDTO request)
    {
        return await _api.Create(request);
    }
}

