

namespace HabitTracker.Domain.Entities
{
    public class HabitLog
    {
        private HabitLog() { }

        public HabitLog(Guid habitId, DateOnly date, ActionType actionType, DateTime createdAt)
        {
            if (!Enum.IsDefined(typeof(ActionType), actionType))
                throw new ArgumentException("Invalid action type");

            HabitId = habitId;
            Date = date;
            ActionType = actionType;
            CreatedAt = createdAt;
        }

        [Obsolete("For testing only", true)]
        internal HabitLog(Guid habitId, DateOnly date, ActionType actionType, HabitEntity habit, DateTime createdAt)
            : this(habitId, date, actionType, createdAt)
        {
            Habit = habit;
        }

        public Guid Id { get; private set; } = Guid.NewGuid();
        public Guid HabitId { get; private set; }
        public DateOnly Date { get; private set; }
        public ActionType ActionType { get; private set; }
        public HabitEntity? Habit {  get; internal set; }
        public DateTime CreatedAt { get; private set; }
    }
}
