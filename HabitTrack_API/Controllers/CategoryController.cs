using HabitTracker.Application.DTOs;
using HabitTracker.Application.UseCases.Categories;
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

        [HttpGet]
        public async Task<IActionResult> GetCategoriesAsync()
        {
            var response = await _categoryService.GetCategories();
            if (!response.IsSuccess)
                return BadRequest(response.ErrorMessage);

            if (!response.Value!.Any())
                return Ok(response.Value);

            return Ok(response.Value);
        }

        [HttpPost]
        public async Task<IActionResult> CreateAsync([FromBody] CategoryResponseDTO category)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var response = await _categoryService.CreateNewCategory(category.Title);

            if (!response.IsSuccess)
                return BadRequest(response.ErrorMessage);

            return CreatedAtAction(nameof(GetCategoriesAsync), new { categoryId = response.Value!.Id }, response.Value);
        }

        [HttpDelete("{categoryId}")]
        public async Task<IActionResult> RemoveAsync(Guid categoryId)
        {
            var response = await _categoryService.DeleteCategory(categoryId);
            if (!response.IsSuccess)
                return BadRequest(response.ErrorMessage);

            return NoContent();
        }

        [HttpPut("{categoryId}")]
        public async Task<IActionResult> UpdateAsync(Guid categoryId, [FromBody] CategoryResponseDTO category)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var response = await _categoryService.UpdateCategory(categoryId, category.Title);

            if (!response.IsSuccess)
                return BadRequest(response.ErrorMessage);

            return NoContent();
        }
    }
}
