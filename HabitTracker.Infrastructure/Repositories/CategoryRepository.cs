using HabitTracker.Application.Common.Interfaces;
using HabitTracker.Domain.Entities;
using HabitTracker.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;



namespace HabitTracker.Infrastructure.Repositories
{
    public class CategoryRepository : ICategoryRepository
    {
        private readonly HabitTrackDBContext _dbContext;
        public CategoryRepository(HabitTrackDBContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task SaveChangesAsync()
        {
            await _dbContext.SaveChangesAsync();
        }

        public async Task AddCategoryAsync(CategoryEntity category)
        {
            await _dbContext.AddAsync(category);
        }

        public async Task<bool> ExistByTitleAsync(Guid userId, string title)
        {
            return await _dbContext.Categories
                .AnyAsync(c => c.UserId == userId && 
                c.Title == title && 
                !c.IsDeleted);
        }

        public async Task<IEnumerable<CategoryEntity>> GetCategoriesByUserIdAsync(Guid userId)
        {
            var categories = await _dbContext.Categories.
                        Where(c => c.UserId == userId && !c.IsDeleted).
                        OrderBy(c => c.Title).
                        ToListAsync();

            return categories;
        }

        public async Task<CategoryEntity?> GetCategoryByIdAsync(Guid userId, Guid id)
        {
            return await _dbContext.Categories
                .FirstOrDefaultAsync(c => c.UserId == userId && 
                c.Id == id &&
                !c.IsDeleted);
        }
    }
}
