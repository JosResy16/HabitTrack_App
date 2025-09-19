using HabitTracker.Application.Common.Interfaces;
using HabitTracker.Application.DTOs;
using HabitTracker.Application.UseCases.Habits;
using Microsoft.AspNetCore.Mvc;

namespace HabitTrack_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HabitController : Controller
    {
        private readonly IHabitsService _habitService;
        private readonly IUserContextService _userContextService;

        public HabitController(IHabitsService habitsService, IUserContextService userContextService)
        {
            _habitService = habitsService;
            _userContextService = userContextService;
        }

        [HttpGet("me/habits")]
        public async Task<IActionResult> GetUserHabits()
        {
            var habits = await _habitService.GetHabitsByUserIdAsync();

            if (habits == null || habits.Count == 0)
                return NoContent();

            return Ok(habits);
        }

        [HttpGet("{habitId}")]
        public async Task<IActionResult> GetHabitById(Guid habitId)
        {
            var habit = await _habitService.GetHabitByIdAsync(habitId);
            return Ok(habit);
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreateNewHabit([FromBody] HabitDTO habitDto)
        {
            var habit = await _habitService.AddNewHabitAsync(habitDto);

            var response = new HabitDTO()
            {
                Title = habit.Title,
                Description = habit.Description
            };

            return CreatedAtAction(nameof(GetHabitById), new {habitId = habit.Id}, response);
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
            var habit = await _habitService.GetHabitByIdAsync(habitId);

            if (habit == null)
                return NotFound();

            await _habitService.RemoveHabitAsync(habit);
            return Ok();
        }
    }
}
