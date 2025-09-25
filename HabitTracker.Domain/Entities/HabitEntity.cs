namespace HabitTracker.Domain.Entities
{
    public class HabitEntity
    {
        public Guid Id { get; set; } 
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;

        public Guid UserId { get; set; }
        public UserEntity User { get; set; }

        public Guid? CategoryId { get; set; }
        public CategoryEntity? Category { get; set; }
        public Priority? Priority { get; set; }
        public bool IsDeleted { get; set; }
        public int RepeatCount { get; set; }
        public int RepeatInterval { get; set; }
        public Period RepeatPeriod { get; set; }
        public TimeOnly? Duration { get; set; }
        public DateTime? LastTimeDoneAt { get; set; }


    }
}
