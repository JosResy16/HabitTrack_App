

using HabitTracker.Domain;
using HabitTracker.Domain.Entities;

namespace HabitTracker.Application.DTOs
{
    public class CreateHabitDTO
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public Guid? CategoryId { get; set; }
        public Priority? Priority { get; set; }
        public int RepeatCount { get; set; }
        public int RepeatInterval { get; set; }
        public Period RepeatPeriod { get; set; }
        public TimeOnly? Duration { get; set; }
    }
}
