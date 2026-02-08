

namespace HabitTracker.Application.DTOs;
public class TodaySummaryDTO
{
    public int TotalHabitsToday { get; set; }
    public int CompletedHabitsToday { get; set; }
    public int CurrentStreak { get; set; }
    public DateTime? FirstCompletionAt { get; set; }
}

