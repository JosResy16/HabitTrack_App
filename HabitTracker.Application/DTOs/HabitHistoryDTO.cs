

using HabitTracker.Domain;

namespace HabitTracker.Application.DTOs
{
    public class HabitHistoryDTO
    {
        public Guid HabitId { get; set; }
        public string HabitTitle { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public ActionType ActionType { get; set; }
    }
}
