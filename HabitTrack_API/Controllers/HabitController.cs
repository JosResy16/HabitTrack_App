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
        private readonly IUserContextService _userContextService;

        public HabitController(IHabitsService habitsService, IUserContextService userContextService)
        {
            _habitService = habitsService;
            _userContextService = userContextService;
        }

        [HttpGet("me/habits")]
        public async Task<IActionResult> GetUserHabits()
        {
            var userId = _userContextService.GetCurrentUserId();
            var habits = await _habitService.GetHabitsByUserIdAsync(userId);

            return Ok(habits);
        }

        [HttpGet("{habitId}")]
        public async Task<IActionResult> GetHabitById(Guid habitId)
        {
            var userId = _userContextService.GetCurrentUserId();
            var habit = await _habitService.GetHabitByIdAsync(habitId, userId);
            return Ok();
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreateNewHabit([FromBody] HabitDTO habit)
        {
            var userId = _userContextService.GetCurrentUserId();
            var createHabit = await _habitService.AddNewHabitAsync(userId, habit);
            return Ok(createHabit);
        }

        [HttpPost("update/{habitId}")]
        public async Task<IActionResult> UpdateAnExistingHabit([FromBody] HabitDTO habitDto, Guid habitId)
        {
            var userId = _userContextService.GetCurrentUserId();
            var updateHabit = await _habitService.UpdateHabit(habitId, habitDto, userId);

            if (updateHabit == null)
                return Forbid();

            return Ok(updateHabit);
        }

        [HttpDelete("{habitId}")]
        public async Task<IActionResult> RemoveHabitAsync(Guid habitId)
        {
            var userId = _userContextService.GetCurrentUserId();
            var habit = await _habitService.GetHabitByIdAsync(habitId, userId);

            if (habit == null || habit.Id == userId)
                return NotFound();

            await _habitService.RemoveHabitAsync(habit);
            return Ok();
        }
    }
}
