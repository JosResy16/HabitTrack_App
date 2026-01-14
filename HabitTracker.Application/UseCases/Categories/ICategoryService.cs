using HabitTracker.Application.DTOs;
using HabitTracker.Application.Services;
using HabitTracker.Domain.Entities;


namespace HabitTracker.Application.UseCases.Categories
{
    public interface ICategoryService
    {     
        Task<Result<IEnumerable<CategoryResponseDTO>>> GetCategories();
        Task<Result<CategoryResponseDTO>> CreateNewCategory(string title);
        Task<Result> UpdateCategory(Guid categoryId, string name);
        Task<Result> DeleteCategory(Guid categoryId);        
    }
}
