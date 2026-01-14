using HabitTracker.Domain;
using System.ComponentModel.DataAnnotations;

namespace HabitTracker.Application.DTOs
{
    public class CreateHabitDTO
    {
        [Required(ErrorMessage="Title can not be empty")]
        [MaxLength(50, ErrorMessage="Title too long max 50 chars")]

        public string Title { get; set; } = string.Empty;
        [MaxLength(150, ErrorMessage = "Description too long (max 150 chars)")]
        public string Description { get; set; } = string.Empty;

        public Guid? CategoryId { get; set; }
        public Priority? Priority { get; set; }

        [Range(1, 365, ErrorMessage = "RepeatCount must be between 1 and 365")]
        public int RepeatCount { get; set; }

        [Range(1, 30, ErrorMessage = "RepeatInterval must be between 1 and 30")]
        public int RepeatInterval { get; set; }
        public Period? RepeatPeriod { get; set; }
        public TimeOnly? Duration { get; set; }
    }
}
