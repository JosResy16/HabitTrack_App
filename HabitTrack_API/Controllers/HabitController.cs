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
            var habits = await _habitQueryService.GetHabitsAsync();

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

        [HttpPost("create")]
        public async Task<IActionResult> CreateNewHabit([FromBody] HabitDTO habitDto)
        {
            var habit = await _habitService.AddNewHabitAsync(habitDto);

            if (!habit.IsSuccess)
                return BadRequest(habit.ErrorMessage);

            var response = new HabitDTO()
            {
                Title = habit.Value.Title,
                Description = habit.Value.Description
            };

            return CreatedAtAction(nameof(GetHabitById), new { habitId = habit.Value.Id }, response);
        }

        [HttpPut("{habitId}")]
        public async Task<IActionResult> UpdateAnExistingHabit([FromBody] HabitDTO habitDto, Guid habitId)
        {
            var updateHabit = await _habitService.UpdateHabit(habitId, habitDto);

            if (!updateHabit.IsSuccess)
                return BadRequest(updateHabit.ErrorMessage);

            return Ok(updateHabit.Value);
        }

        [HttpPost("{habitId}/done")]
        public async Task<IActionResult> MarkHabitAsDone(Guid habitId)
        {
            var response = await _habitService.MarkHabitAsDone(habitId);

            if (!response.IsSucces)
                return BadRequest(response.ErrorMessage);

            return Ok(response.IsSucces);
        }

        [HttpDelete("{habitId}/done")]
        public async Task<IActionResult> UndoHabitCompletion(Guid habitId)
        {
            var response = await _habitService.UndoHabitCompletion(habitId);
            if (!response.IsSucces)
                return BadRequest(response.ErrorMessage);

            return Ok(response.IsSucces);
        }

        [HttpDelete("{habitId}")]
        public async Task<IActionResult> RemoveHabitAsync(Guid habitId)
        {
            var habit = await _habitQueryService.GetHabitByIdAsync(habitId);

            if (!habit.IsSuccess)
                return BadRequest();

            if (habit.Value != null)
                await _habitService.RemoveHabitAsync(habit.Value);

            return Ok(habit.Value);
        }

        [HttpGet]
        public async Task<IActionResult> GetHabitsByPriority([FromQuery] Priority? priority = null)
        {
            var habits = await _habitQueryService.GetHabitsByPriorityAsync(priority);

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

        [HttpGet("history")]
        public async Task<IActionResult> GetHabitHistory()
        {
            var habits = await _habitQueryService.GetHabitHistoryAsync();
            if (!habits.IsSuccess || !habits.Value.Any())
                return NoContent();
            return Ok(habits.Value);
        }
    }
}
