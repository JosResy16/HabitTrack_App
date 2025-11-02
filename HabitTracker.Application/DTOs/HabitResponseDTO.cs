

using HabitTracker.Domain;

namespace HabitTracker.Application.DTOs
{
    public class HabitResponseDTO
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public Guid? CategoryId { get; set; }
        public Priority? Priority { get; set; }
        public int RepeatCount { get; set; }
        public int RepeatInterval { get; set; }
        public Period RepeatPeriod { get; set; }
        public TimeOnly? Duration { get; set; }
        public DateTime? LastTimeDoneAt { get; set; }
    }
}
