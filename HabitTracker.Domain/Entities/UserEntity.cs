

namespace HabitTracker.Domain.Entities
{
    public class UserEntity
    {
        public Guid Id { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string PasswordHashed { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string? RefreshToken { get; set; } = string.Empty;
        public DateTime RefreshTokenExpiryTime { get; set; }
        public List<HabitEntity> Habits { get; set; } = new();

        public void AddHabit(HabitEntity habit)
        {
            Habits.Add(habit);
        }
    }
}
