using HabitTracker.Application.Common.Interfaces;
using HabitTracker.Application.Services;
using HabitTracker.Domain.Entities;
using HabitTracker.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection.Metadata;


namespace HabitTracker.Infrastructure.Repositories
{
    public class CategoryRepository : ICategoryRepository
    {
        private readonly HabitTrackDBContext _dbContext;
        public CategoryRepository(HabitTrackDBContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Result<CategoryEntity>> AddCategoryAsync(CategoryEntity category)
        {
            await _dbContext.AddAsync(category);
            await _dbContext.SaveChangesAsync();
            return Result<CategoryEntity>.Success(category);
        }

        public async Task<Result> DeleteCategoryAsync(Guid id)
        {
            var category = await _dbContext.Categories.FindAsync(id);
            if (category != null)
            {
                category.IsDeleted = true;
                _dbContext.Categories.Update(category);
                await _dbContext.SaveChangesAsync();
                return Result.Success();
            }
            return Result.Failure("category don't match with this value (id)");
        }

        public Result ExistByTitleAsync(Guid userId, string title)
        {
            var existingCategory = _dbContext.Categories.FindAsync(title);
             if (existingCategory.Result != null)
                return Result.Failure("Category already exists with the same title");

            return Result.Success();
        }

        public async Task<Result<IEnumerable<CategoryEntity>>> GetCategoriesByUserIdAsync(Guid id)
        {
            var categories = await _dbContext.Categories.
                        Where(c => c.UserId == id && !c.IsDeleted).
                        OrderBy(c => c.Title).
                        ToListAsync();

            return Result<IEnumerable<CategoryEntity>>.Success(categories);
        }

        public async Task<Result<CategoryEntity>> GetCategoryByIdAsync(Guid id)
        {
            var category = await _dbContext.Categories.FirstAsync(c => c.Id == id);
            if (category == null)
                return Result<CategoryEntity>.Failure("category do not found");

            return Result<CategoryEntity>.Success(category);
        }

        public async Task<Result> UpdateCategoryAsync(CategoryEntity category, CategoryEntity categoryNewData)
        {
            var categoryToUpdate = await _dbContext.Categories.FirstAsync(c => c.Id == category.Id);
            if (categoryToUpdate == null)
                return Result.Failure("category do not found");
            
            categoryToUpdate.Title = category.Title;

            _dbContext.Categories.Update(categoryToUpdate);
            await _dbContext.SaveChangesAsync();

            return Result.Success();
        }
    }
}
