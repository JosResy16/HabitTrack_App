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
        public Task<CategoryEntity> GetCategoryByIdAsync(Guid id);
    }
}
