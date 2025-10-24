using System.ComponentModel.DataAnnotations;

namespace HabitTracker.Domain.Entities
{
    public class CategoryEntity
    {
        public Guid Id { get; set; }

        [Required(ErrorMessage = "Title can't be empty")]
        public string Title { get; set; } = string.Empty;
        public Guid UserId { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime CreatedAt { get; set; }
        public IEnumerable<HabitEntity> Habits { get; set; }
    }
}
