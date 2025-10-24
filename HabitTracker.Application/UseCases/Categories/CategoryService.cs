using HabitTracker.Application.Common.Interfaces;
using HabitTracker.Application.Services;
using HabitTracker.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HabitTracker.Application.UseCases.Categories
{
    public class CategoryService : ICategoryService
    {
        private readonly ICategoryRepository _categoryRepository;
        private readonly UserContextService _userContext;

        public CategoryService(ICategoryRepository categoryRepository, UserContextService userContext)
        {
            _categoryRepository = categoryRepository;
            _userContext = userContext;
        }

        public async Task<Result<CategoryEntity>> CreateNewCategory(CategoryEntity category)
        {
            var userId = _userContext.GetCurrentUserId();

            if (userId == Guid.Empty)
                return Result<CategoryEntity>.Failure("User not found/Not loged in");

            await _categoryRepository.AddCategoryAsync(category);

            return Result<CategoryEntity>.Success(category);
        }

        public async Task<Result> DeleteCategory(Guid categoryId)
        {
            var category = await _categoryRepository.GetCategoryByIdAsync(categoryId);
            var userId = _userContext.GetCurrentUserId();

            if (category.Value?.UserId != userId)
                return Result.Failure("User not found/Not loged in");

            if (category.Value.Id == Guid.Empty)
                return Result.Failure("category not found");


            var result = await _categoryRepository.DeleteCategoryAsync(category.Value.Id);

            return result.IsSucces
                ? Result.Success()
                : Result.Failure("Could not delete the category");
        }

        public async Task<Result<IEnumerable<CategoryEntity>>> GetCategories()
        {
            var userId = _userContext.GetCurrentUserId();
            var categories = await _categoryRepository.GetCategoriesByUserIdAsync(userId);

            if (!categories.Value.Any())
                Result.Failure("categories do not found");

            return Result<IEnumerable<CategoryEntity>>.Success(categories.Value);
        }

        public async Task<Result> UpdateCategory(Guid categoryId, CategoryEntity category)
        {
            var userId = _userContext.GetCurrentUserId();

            var categoryFromDb = await _categoryRepository.GetCategoryByIdAsync(categoryId);
            if (categoryFromDb == null)
                return Result.Failure("category do not found");

            if (categoryFromDb.Value?.UserId != userId)
                return Result.Failure("category does not belong to this user");

            var result = await _categoryRepository.UpdateCategoryAsync(categoryFromDb.Value, category);

            return result.IsSucces
                ? Result.Success()
                : Result.Failure("Category could not be updated.");
        }
    }
}
