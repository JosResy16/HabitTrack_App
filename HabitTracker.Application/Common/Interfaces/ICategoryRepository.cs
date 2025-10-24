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
        Task<CategoryEntity?> GetCategoryByIdAsync(Guid id);
        Task<IEnumerable<CategoryEntity>> GetCategoriesByUserIdAsync(Guid userId);
        Task<CategoryEntity> AddCategoryAsync(CategoryEntity category);
        Task<bool> UpdateCategoryAsync(CategoryEntity category, CategoryEntity categoryNewData);
        Task<bool> DeleteCategoryAsync(Guid id);
        bool ExistByTitleAsync(Guid userId, string title);
    }
}
