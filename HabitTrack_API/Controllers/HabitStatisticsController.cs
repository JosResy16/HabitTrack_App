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

    public HabitStatisticsController(IHabitStatisticsService statisticsService)
    {
        _statisticsService = statisticsService;
    }

    [HttpGet("summary")]
    public async Task<IActionResult> GetSummaryAsync()
    {
        var result = await _statisticsService.GetSummaryAsync();
        if (result.IsSuccess)
        {
            return Ok(result.Value);
        }

        return BadRequest(result.ErrorMessage);
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
}
