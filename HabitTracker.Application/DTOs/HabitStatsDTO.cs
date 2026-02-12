

namespace HabitTracker.Application.DTOs;
public class HabitStatsDTO
{
    public int TotalCompletion { get; set; }
    public int CurrentStreak { get; set; }
    public int LongestStreak { get; set; }
    public double CompletionRate { get; set; }

    public int TotalTrackedDays { get; set; }
    public int DaysSinceLastCompletion { get; set; }
}

