using HabitTracker.Application.Common.Interfaces;
using HabitTracker.Application.DTOs;
using HabitTracker.Application.UseCases.Habits;
using HabitTracker.Domain;
using HabitTracker.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HabitTrack_API.Controllers
{
    [Route("api/[controller]")]
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

        [HttpGet("me/habits")]
        public async Task<IActionResult> GetUserHabits()
        {
            var habits = await _habitQueryService.GetUserHabitsAsync();

            if (!habits.IsSuccess || !habits.Value.Any())
                return NoContent();

            return Ok(habits);
        }

        [HttpGet("{habitId}")]
        public async Task<IActionResult> GetHabitById(Guid habitId)
        {
            var habit = await _habitQueryService.GetHabitByIdAsync(habitId);
            if (!habit.IsSuccess)
                return NoContent();

            return Ok(habit);
        }

        [HttpPost]
        [ProducesResponseType(typeof(CreateHabitDTO), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateNewHabit([FromBody] CreateHabitDTO habitDto)
        {
            if (habitDto == null)
                return BadRequest("Habit data is required.");

            var result = await _habitService.AddNewHabitAsync(habitDto);

            if (!result.IsSuccess)
                return BadRequest(result.ErrorMessage);

            return CreatedAtAction(nameof(GetHabitById), new { habitId = result.Value?.Id }, result.Value);
        }

        [HttpPut("{habitId}")]
        public async Task<IActionResult> UpdateAnExistingHabit([FromBody] CreateHabitDTO habitDto, Guid habitId)
        {
            var updateHabit = await _habitService.UpdateHabitAsync(habitId, habitDto);

            if (!updateHabit.IsSuccess)
                return BadRequest(updateHabit.ErrorMessage);

            return Ok(updateHabit.Value);
        }

        [HttpPost("{habitId}/done")]
        public async Task<IActionResult> MarkHabitAsDone(Guid habitId)
        {
            var response = await _habitService.MarkHabitAsDone(habitId);

            if (!response.IsSuccess)
                return BadRequest(response.ErrorMessage);

            return Ok(response.IsSuccess);
        }

        [HttpDelete("{habitId}/done")]
        public async Task<IActionResult> UndoHabitCompletion(Guid habitId)
        {
            var response = await _habitService.UndoHabitCompletion(habitId);
            if (!response.IsSuccess)
                return BadRequest(response.ErrorMessage);

            return Ok(response.IsSuccess);
        }

        [HttpDelete("{habitId}")]
        public async Task<IActionResult> RemoveHabitAsync(Guid habitId)
        {
            var response = await _habitService.RemoveHabitAsync(habitId);
            if (!response.IsSuccess)
                return BadRequest(response.ErrorMessage);

            return Ok(response.IsSuccess);
        }

        [HttpGet]
        public async Task<IActionResult> GetHabitsByPriority([FromQuery] Priority? priority = null)
        {
            var habits = await _habitQueryService.GetUserHabitsAsync(priority);

            if (!habits.IsSuccess || !habits.Value.Any())
                return NoContent();

            return Ok(habits.Value);
        }

        [HttpGet("category/{categoryId}")]
        public async Task<IActionResult> GetHabitsByCategory(Guid categoryId)
        {
            var habits = await _habitQueryService.GetHabitsByCategoryAsync(categoryId);
            if (!habits.IsSuccess || !habits.Value.Any())
                return NoContent();
            return Ok(habits.Value);
        }

        [HttpGet("today")]
        public async Task<IActionResult> GetTodaysHabit([FromBody] DateTime? day = null)
        {
            var habits = await _habitQueryService.GetTodayHabitsAsync(day ?? DateTime.UtcNow);
            if (!habits.IsSuccess || !habits.Value.Any())
                return NoContent();
            return Ok(habits.Value);
        }

        [HttpGet("history/{habitId}")]
        public async Task<IActionResult> GetHabitHistory(Guid habitId)
        {
            var habits = await _habitQueryService.GetHabitHistoryAsync(habitId);
            if (!habits.IsSuccess || !habits.Value.Any())
                return NoContent();
            return Ok(habits.Value);
        }
    }
}
