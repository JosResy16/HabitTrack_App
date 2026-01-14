

namespace HabitTracker.Domain.Entities
{
    public class HabitLog
    {
        private HabitLog() { }

        public HabitLog(Guid habitId, DateOnly date, ActionType actionType, DateTime createdAt)
        {
            if (!Enum.IsDefined(typeof(ActionType), actionType))
                throw new ArgumentException("Invalid action type");

            Id = Guid.NewGuid();
            HabitId = habitId;
            Date = date;
            ActionType = actionType;
            CreatedAt = createdAt;
        }

        //used by tests
        internal HabitLog(Guid habitId, DateOnly date, ActionType actionType, HabitEntity habit, DateTime createdAt)
            : this(habitId, date, actionType, createdAt)
        {
            Habit = habit;
        }

        public Guid Id { get; private set; }
        public Guid HabitId { get; private set; }
        public DateOnly Date { get; private set; }
        public ActionType ActionType { get; private set; }
        public HabitEntity? Habit {  get; internal set; }
        public DateTime CreatedAt { get; private set; }
    }
}
