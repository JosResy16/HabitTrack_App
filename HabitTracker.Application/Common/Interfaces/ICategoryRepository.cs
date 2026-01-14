using HabitTracker.Application.Services;
using HabitTracker.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HabitTracker.Application.Common.Interfaces
{
    public interface ICategoryRepository
    {
        Task SaveChangesAsync();
        Task<CategoryEntity?> GetCategoryByIdAsync(Guid userId, Guid id);
        Task<IEnumerable<CategoryEntity>> GetCategoriesByUserIdAsync(Guid userId);
        Task AddCategoryAsync(CategoryEntity category);
        Task<bool> ExistByTitleAsync(Guid userId, string title);
    }
}
