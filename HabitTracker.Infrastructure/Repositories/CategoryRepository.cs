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

        public async Task<CategoryEntity> AddCategoryAsync(CategoryEntity category)
        {
            await _dbContext.AddAsync(category);
            await _dbContext.SaveChangesAsync();
            return category;
        }

        public async Task<bool> DeleteCategoryAsync(Guid id)
        {
            var category = await _dbContext.Categories.FindAsync(id);
            if (category == null)
                return false;

            category.IsDeleted = true;
            _dbContext.Categories.Update(category);
            await _dbContext.SaveChangesAsync();
            return true;
        }

        public bool ExistByTitleAsync(Guid userId, string title)
        {
            var existingCategory = _dbContext.Categories.FindAsync(title);
             if (existingCategory.Result != null)
                return false;

            return true;
        }

        public async Task<IEnumerable<CategoryEntity>> GetCategoriesByUserIdAsync(Guid id)
        {
            var categories = await _dbContext.Categories.
                        Where(c => c.UserId == id && !c.IsDeleted).
                        OrderBy(c => c.Title).
                        ToListAsync();

            return categories;
        }

        public async Task<CategoryEntity?> GetCategoryByIdAsync(Guid id)
        {
            var category = await _dbContext.Categories.FirstAsync(c => c.Id == id);
            if (category == null)
                return null;

            return category;
        }

        public async Task<bool> UpdateCategoryAsync(CategoryEntity category, CategoryEntity categoryNewData)
        {
            var categoryToUpdate = await _dbContext.Categories.FirstAsync(c => c.Id == category.Id);
            if (categoryToUpdate == null)
                return false;
            
            categoryToUpdate.Title = category.Title;

            _dbContext.Categories.Update(categoryToUpdate);
            await _dbContext.SaveChangesAsync();

            return true;
        }
    }
}
