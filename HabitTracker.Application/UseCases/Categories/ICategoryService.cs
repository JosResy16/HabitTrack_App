using HabitTracker.Application.Services;
using HabitTracker.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
