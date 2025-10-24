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
        Task<Result<CategoryEntity>> GetCategoryByIdAsync(Guid id);
        Task<Result<IEnumerable<CategoryEntity>>> GetCategoriesByUserIdAsync(Guid userId);
        Task<Result<CategoryEntity>> AddCategoryAsync(CategoryEntity category);
        Task<Result> UpdateCategoryAsync(CategoryEntity category, CategoryEntity categoryNewData);
        Task<Result> DeleteCategoryAsync(Guid id);
        Result ExistByTitleAsync(Guid userId, string title);
    }
}
