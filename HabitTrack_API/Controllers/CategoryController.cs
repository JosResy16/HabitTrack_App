using HabitTracker.Application.DTOs;
using HabitTracker.Application.UseCases.Categories;
using HabitTracker.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HabitTrack_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class CategoryController : ControllerBase
    {
        private readonly ICategoryService _categoryService;

        public CategoryController(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        [HttpGet("me/categories")]
        public async Task<IActionResult> GetUserCategories()
        {
            var response = await _categoryService.GetCategories();
            if (!response.IsSuccess)
                return BadRequest(response.ErrorMessage);

            if (!response.Value.Any())
                return NoContent();

            return Ok(response.Value);
        }

        [HttpPost]
        public async Task<IActionResult> CreateNewCategory([FromBody] CategoryDTO category)
        {
            var categoryEntity = new CategoryEntity
            {
                Title = category.Title
            };
            var response = await _categoryService.CreateNewCategory(categoryEntity);
            if (!response.IsSuccess)
                return BadRequest(response.ErrorMessage);
            return CreatedAtAction(nameof(GetUserCategories), new { categoryId = response.Value.Id }, response.Value);
        }

        [HttpDelete("{categoryId}")]
        public async Task<IActionResult> RemoveCategoryAsync(Guid categoryId)
        {
            var response = await _categoryService.DeleteCategory(categoryId);
            if (!response.IsSuccess)
                return BadRequest(response.ErrorMessage);
            return Ok(response.IsSuccess);
        }

        [HttpPut("{categoryId}")]
        public async Task<IActionResult> UpdateCategoryAsync(Guid categoryId, [FromBody] CategoryDTO category)
        {
            var categoryEntity = new CategoryEntity
            {
                Title = category.Title
            };
            var response = await _categoryService.UpdateCategory(categoryId, categoryEntity);
            if (!response.IsSuccess)
                return BadRequest(response.ErrorMessage);
            return Ok(response.IsSuccess);
        }

    }
}
