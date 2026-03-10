using HabitTracker.Application.Common.Interfaces;
using HabitTracker.Application.Services;
using HabitTracker.Application.UseCases.Habits;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HabitTrack_API.Controllers;

[Route("api/habit-stats")]
[ApiController]
[Authorize]
public class HabitStatisticsController : ControllerBase
{
    private readonly IHabitStatisticsService _statisticsService;
    private readonly IDateTimeProvider _dateTimeProvider;

    public HabitStatisticsController(IHabitStatisticsService statisticsService, IDateTimeProvider dateTimeProvider)
    {
        _statisticsService = statisticsService;
        _dateTimeProvider = dateTimeProvider;
    }

    [HttpGet("today-summary")]
    public async Task<IActionResult> GetTodaySummary()
    {
        var result = await _statisticsService.GetTodaySummaryAsync();
        if (result.IsSuccess)
        {
            return Ok(result.Value);
        }

        return BadRequest(result.ErrorMessage);
    }

    [HttpGet("past-three-months")]
    public async Task<IActionResult> GetPastThreeMonthsStats()
    {
        var today = DateOnly.FromDateTime(_dateTimeProvider.UtcNow);
        var start = today.AddDays(-90);
        var end = today;

        var result = await _statisticsService.GetUserStatsAsync(start, end);
        if (result.IsSuccess)
        {
            return Ok(result.Value);
        }
        return BadRequest(result.ErrorMessage);
    }

    [HttpGet("{habitId}/activity")]
    public async Task<IActionResult> GetHabitActivityAsync(Guid habitId, [FromQuery] DateOnly startDate, [FromQuery] DateOnly endDate)
    {
        var today = DateOnly.FromDateTime(_dateTimeProvider.UtcNow);
        startDate = today.AddDays(-179);
        endDate = today;

        var result = await _statisticsService.GetHabitActivityAsync(habitId, startDate, endDate);
        if (result.IsSuccess)
        {
            return Ok(result.Value);
        }
        return BadRequest(result.ErrorMessage);
    }

    [HttpGet("{habitId}")]
    public async Task<IActionResult> GetHabitStatsAsync(Guid habitId)
    {
        var result = await _statisticsService.GetHabtitStatsAsync(habitId);
        if (result.IsSuccess)
        {
            return Ok(result.Value);
        }

        return BadRequest(result.ErrorMessage);
    }

    [HttpGet("{habitId}/completion-rate")]
    public async Task<IActionResult> GetCompletionRateForHabit(Guid habitId, [FromQuery] DateOnly startDate, [FromQuery] DateOnly endDate)
    {
        var result = await _statisticsService.GetCompletionRateForHabitAsync(habitId, startDate, endDate);

        if (result.IsSuccess)
        {
            return Ok(result.Value);
        }

        return BadRequest(result.ErrorMessage);
    }
}
