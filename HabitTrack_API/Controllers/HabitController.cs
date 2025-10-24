using HabitTracker.Application.Common.Interfaces;
using HabitTracker.Application.DTOs;
using HabitTracker.Application.UseCases.Habits;
using HabitTracker.Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace HabitTrack_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HabitController : Controller
    {
        private readonly IHabitsService _habitService;
        private readonly IHabitQueryService _habitQueryService;
        private readonly IUserContextService _userContextService;

        public HabitController(IHabitsService habitsService, IHabitQueryService habitQueryService ,IUserContextService userContextService)
        {
            _habitService = habitsService;
            _habitQueryService = habitQueryService;
            _userContextService = userContextService;
        }

        [HttpGet("me/habits")]
        public async Task<IActionResult> GetUserHabits()
        {
            var habits = await _habitQueryService.GetHabitsAsync();

            if (habits == null || habits.Value.Any<HabitEntity>())
                return NoContent();

            return Ok(habits);
        }

        [HttpGet("{habitId}")]
        public async Task<IActionResult> GetHabitById(Guid habitId)
        {
            var habit = await _habitQueryService.GetHabitByIdAsync(habitId);
            return Ok(habit);
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreateNewHabit([FromBody] HabitDTO habitDto)
        {
            var habit = await _habitService.AddNewHabitAsync(habitDto);

            var response = new HabitDTO()
            {
                Title = habit.Value.Title,
                Description = habit.Value.Description
            };

            return CreatedAtAction(nameof(GetHabitById), new {habitId = habit.Value.Id}, response);
        }

        [HttpPut("update/{habitId}")]
        public async Task<IActionResult> UpdateAnExistingHabit([FromBody] HabitDTO habitDto, Guid habitId)
        {
            var updateHabit = await _habitService.UpdateHabit(habitId, habitDto);

            return Ok(updateHabit);
        }

        [HttpDelete("{habitId}")]
        public async Task<IActionResult> RemoveHabitAsync(Guid habitId)
        {
            var habit = await _habitQueryService.GetHabitByIdAsync(habitId);

            if (habit == null)
                return NotFound();

            await _habitService.RemoveHabitAsync(habit.Value);
            return Ok();
        }
    }
}
