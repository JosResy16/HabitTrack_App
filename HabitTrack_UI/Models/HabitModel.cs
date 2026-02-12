using HabitTracker.Domain;

namespace HabitTrack_UI.Models;
public class HabitModel
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Guid? CategoryId { get; set; }
    public Priority? Priority { get; set; }
    public int RepeatCount { get; set; }
    public Period? RepeatPeriod { get; set; }
    public DateTime? LastTimeDoneAt { get; set; }
    public DateTime CreatedAt { get; set; }
}

