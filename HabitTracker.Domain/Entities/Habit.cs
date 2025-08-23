

namespace HabitTracker.Domain.Entities
{
    public class Habit
    {
        public Guid Id { get; set; } 
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;

        public Guid UserId { get; set; }
        public User User { get; set; }

    }
}
