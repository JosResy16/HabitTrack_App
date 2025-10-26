using HabitTracker.Application.Services;
using HabitTracker.Domain.Entities;


namespace HabitTracker.Application.UseCases.Categories
{
    public interface ICategoryService
    {     
        Task<Result<IEnumerable<CategoryEntity>>> GetCategories();
        Task<Result<CategoryEntity>> CreateNewCategory(CategoryEntity category);
        Task<Result> UpdateCategory(Guid categoryId, CategoryEntity category);
        Task<Result> DeleteCategory(Guid categoryId);        
    }
}
