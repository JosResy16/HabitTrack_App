using HabitTracker.Application.Common.Interfaces;
using HabitTracker.Application.DTOs;
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
        private readonly IUserContextService _userContext;

        public CategoryService(ICategoryRepository categoryRepository, IUserContextService userContext)
        {
            _categoryRepository = categoryRepository;
            _userContext = userContext;
        }

        public async Task<Result<CategoryResponseDTO>> CreateNewCategory(string title)
        {
            var userId = _userContext.GetCurrentUserId();

            var categoryEntity = new CategoryEntity(userId, title);


            await _categoryRepository.AddCategoryAsync(categoryEntity);
            await _categoryRepository.SaveChangesAsync();

            return Result<CategoryResponseDTO>.Success(MapToDto(categoryEntity));
        }

        public async Task<Result> DeleteCategory(Guid categoryId)
        {
            var userId = _userContext.GetCurrentUserId();
            var category = await _categoryRepository.GetCategoryByIdAsync(userId, categoryId);

            if (category == null)
                return Result.Failure("Category not found");

            category.SoftDelete();
            await _categoryRepository.SaveChangesAsync();

            return Result.Success();
        }

        public async Task<Result<IEnumerable<CategoryResponseDTO>>> GetCategories()
        {
            var userId = _userContext.GetCurrentUserId();
            var categories = await _categoryRepository.GetCategoriesByUserIdAsync(userId);

            var categoriesDto = categories.Select(MapToDto);

            return Result<IEnumerable<CategoryResponseDTO>>.Success(categoriesDto);
        }

        public async Task<Result> UpdateCategory(Guid categoryId, string name)
        {
            var userId = _userContext.GetCurrentUserId();

            var category = await _categoryRepository.GetCategoryByIdAsync(userId, categoryId);

            if (category == null)
                return Result.Failure("category do not found");

            category.Rename(name);
            await _categoryRepository.SaveChangesAsync();

            return Result.Success();
        }

        private static CategoryResponseDTO MapToDto(CategoryEntity entity)
        {
            return new CategoryResponseDTO
            {
                Id = entity.Id,
                Title = entity.Title!
            };
        }
    }
}
