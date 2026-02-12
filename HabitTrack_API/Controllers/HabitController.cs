using HabitTracker.Application.DTOs;
using HabitTracker.Application.Services;
using HabitTracker.Application.UseCases.Habits;
using HabitTracker.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HabitTrack_API.Controllers
{
    [Route("api/habits")]
    [ApiController]
    [Authorize]
    public class HabitController : ControllerBase
    {
        private readonly IHabitsService _habitService;
        private readonly IHabitQueryService _habitQueryService;

        public HabitController(IHabitsService habitsService, IHabitQueryService habitQueryService)
        {
            _habitService = habitsService;
            _habitQueryService = habitQueryService;
        }

        [HttpGet]
        public async Task<IActionResult> GetHabitsAsync([FromQuery] Priority? priority)
        {
            var result = await _habitQueryService.GetUserHabitsAsync(priority);
            return FromResult(result);
        }

        [HttpGet("{habitId}")]
        public async Task<IActionResult> GetByIdAsync(Guid habitId)
        {
            var result = await _habitQueryService.GetHabitByIdAsync(habitId);
            return FromResult(result);
        }

        [HttpGet("today")]
        public async Task<IActionResult> GetTodayHabitsAsync([FromQuery]DateOnly? day)
        {
            var habits = await _habitQueryService.GetTodayHabitsAsync(day ?? DateOnly.FromDateTime(DateTime.UtcNow));
            return FromResult(habits);
        }

        [HttpGet("{habitId}/history")]
        public async Task<IActionResult> GetHistory(Guid habitId)
        {
            var result = await _habitQueryService.GetHabitHistoryAsync(habitId);
            return FromResult(result);
        }

        [HttpGet("category/{categoryId}")]
        public async Task<IActionResult> GetByCategoryAsync(Guid categoryId)
        {
            var result = await _habitQueryService.GetHabitsByCategoryAsync(categoryId);
            return FromResult(result);
        }

        [HttpPost]
        [ProducesResponseType(typeof(CreateHabitDTO), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create([FromBody] CreateHabitDTO habitDto)
        {
            var result = await _habitService.AddNewHabitAsync(habitDto);

            if (!result.IsSuccess)
                return BadRequest(result.ErrorMessage);

            return Ok(result);
        }

        [HttpPut("{habitId}")]
        public async Task<IActionResult> UpdateAsync(Guid habitId, [FromBody] UpdateHabitDTO dto)
        {
            var result = await _habitService.UpdateHabitAsync(habitId, dto);
            return FromResult(result);
        }

        [HttpPost("{habitId}/complete")]
        public async Task<IActionResult> MarkCompletedAsync(Guid habitId)
        {
            var response = await _habitService.MarkHabitAsDone(habitId);
            return FromResult(response);
        }

        [HttpDelete("{habitId}/complete")]
        public async Task<IActionResult> UndoCompletionAsync(Guid habitId)
        {
            var result = await _habitService.UndoHabitCompletion(habitId);
            return FromResult(result);
        }

        [HttpDelete("{habitId}")]
        public async Task<IActionResult> RemoveAsync(Guid habitId)
        {
            var result = await _habitService.RemoveHabitAsync(habitId);
            return FromResult(result);
        }

        private IActionResult FromResult<T>(Result<T> result)
        {
            if (result.IsSuccess)
            {
                return Ok(result.Value);
            }

            return BadRequest(result.ErrorMessage);
        }

        private IActionResult FromResult(Result result)
        {
            if (result.IsSuccess)
            {
                return Ok();
            }

            return BadRequest(result.ErrorMessage);
        }

    }
}
