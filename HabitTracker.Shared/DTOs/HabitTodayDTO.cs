using HabitTracker.Shared.Enums;

namespace HabitTracker.Shared.DTOs
{
    public class HabitTodayDTO
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public Guid? CategoryId { get; set; }
        public HabitPriority? Priority { get; set; }
        public bool IsCompletedToday { get; set; }
        public DateTime? LastTimeDoneAt { get; set; }
    }
}
