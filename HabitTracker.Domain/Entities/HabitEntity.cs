using System.ComponentModel.DataAnnotations;

namespace HabitTracker.Domain.Entities
{
    public class HabitEntity
    {
        private HabitEntity() { }

        public HabitEntity(
        Guid userId,
        string title,
        string? description,
        Period? repeatPeriod,
        int? repeatInterval)
        {
            if (string.IsNullOrWhiteSpace(title))
                throw new ArgumentException("Title is required");

            if (repeatInterval <= 0)
                throw new ArgumentException("Repeat interval must be greater than zero");

            UserId = userId;
            Title = title;
            Description = description;
            RepeatPeriod = repeatPeriod;
            RepeatInterval = repeatInterval;

            IsCompleted = false;
            IsDeleted = false;
        }

        public Guid Id { get; private set; } = Guid.NewGuid();

        [Required(ErrorMessage= "Title is necesary")]
        public string Title { get; set; } = string.Empty;

        public string? Description { get; set; }

        public Guid UserId { get; set; }
        public UserEntity? User { get; set; }
        public bool IsCompleted { get; private set; } = false;
        public Guid? CategoryId { get; set; }
        public CategoryEntity? Category { get; set; }
        public Priority? Priority { get; set; }
        public bool IsDeleted { get; private set; }

        public int RepeatCount { get; set; }
        public int? RepeatInterval { get; set; }
        public Period? RepeatPeriod { get; set; }
        public TimeOnly? Duration { get; set; }
        public DateTime? LastTimeDoneAt { get; private set; }

        public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;

        public IReadOnlyCollection<HabitLog> Logs => _logs;
        private readonly List<HabitLog> _logs = new();

        public void SoftDelete()
        {
            if (IsDeleted) return;
            IsDeleted = true;
        }

        public void UpdateDetails(
            string title,
            string description,
            Priority? priority)
        {
            if (string.IsNullOrWhiteSpace(title))
                throw new ArgumentException("Title is required");

            Title = title;
            Description = description;
            Priority = priority;
        }

        public void MarkHabitAsDone()
        {
            if (IsCompleted) return;

            IsCompleted = true;
            LastTimeDoneAt = DateTime.UtcNow;
        }

        public void UndoCompletion()
        {
            if (!IsCompleted) return;

            IsCompleted = false;
            LastTimeDoneAt = null;  
        }
    }
}
