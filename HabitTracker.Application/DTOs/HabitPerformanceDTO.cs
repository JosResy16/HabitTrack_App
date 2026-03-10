

namespace HabitTracker.Application.DTOs;
public class HabitPerformanceDTO
{
    public Guid HabitId { get; set; }

    public string HabitTitle { get; set; } = string.Empty;

    public int TotalCompletionsThisMonth { get; set; }
    public double CompletionRateThisMonth { get; set; }
}


