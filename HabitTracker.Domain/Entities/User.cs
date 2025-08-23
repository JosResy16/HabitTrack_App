

namespace HabitTracker.Domain.Entities
{
    public class User
    {
        public Guid Id { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string PasswordHashed { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string? RefreshToken { get; set; } = string.Empty;
        public DateTime RefreshTokenExpiryTime { get; set; }
        public List<Habit> Habits { get; set; } = new();

        public void AddHabit(Habit habit)
        {
            Habits.Add(habit);
        }
    }
}
