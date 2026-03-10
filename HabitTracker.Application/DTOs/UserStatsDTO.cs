

namespace HabitTracker.Application.DTOs;

public class UserStatsDTO
{
    public int TotalActiveHabits { get; set; }
    public int TotalCompletions { get; set; }

    public int LongestStreak { get; set; }
    public int CurrentStreak { get; set; }

    public double ActiveDaysPercentageThisMonth { get; set; }
    public int ActiveDaysThisMonth { get; set; }
    public int TotalDaysThisMonth { get; set; }
    public int TotalPausedHabits { get; set; }

    public List<DailyActivityDTO> MonthlyActivity { get; set; } = new();

    public List<HabitPerformanceDTO> HabitBreakdown { get; set; } = new();
}

