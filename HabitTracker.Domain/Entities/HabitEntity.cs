using System.ComponentModel.DataAnnotations;

namespace HabitTracker.Domain.Entities
{
    public class HabitEntity
    {
        public Guid Id { get; set; }

        [Required(ErrorMessage= "Title is necesary")]
        [MaxLength(100, ErrorMessage = "Title can't be longer than 100 characters")]
        public string Title { get; set; } = string.Empty;

        [MaxLength(500, ErrorMessage = "Description can't be longer than 500 characters")]
        public string Description { get; set; } = string.Empty;

        public Guid UserId { get; set; }
        public UserEntity User { get; set; }
        public bool IsCompleted { get; private set; } = false;
        public Guid? CategoryId { get; set; }
        public CategoryEntity? Category { get; set; }
        public Priority? Priority { get; set; }
        public bool IsDeleted { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Repeat count shuld be a positive number")]
        public int RepeatCount { get; set; }
        public int RepeatInterval { get; set; }
        public Period RepeatPeriod { get; set; }
        public TimeOnly? Duration { get; set; }
        public DateTime? LastTimeDoneAt { get; private set; }

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
