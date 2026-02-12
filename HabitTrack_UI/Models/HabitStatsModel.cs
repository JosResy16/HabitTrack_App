namespace HabitTrack_UI.Models;
public class HabitStatsModel
{
    public int TotalCompletion { get; set; }
    public int CurrentStreak { get; set; }
    public int LongestStreak { get; set; }
    public double CompletitionRate { get; set; }

    public int TotalTrackedDays { get; set; }
    public int DaysSinceLastCompletion { get; set; }
}

