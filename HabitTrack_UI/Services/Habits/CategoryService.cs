using HabitTrack_UI.Models;
using HabitTrack_UI.Services.Api;

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
}

