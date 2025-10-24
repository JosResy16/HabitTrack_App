using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HabitTracker.Domain.Entities
{
    public class HabitLog
    {
        public Guid Id { get; private set; }
        public Guid HabitId { get; private set; }
        public DateTime Date { get; private set; }
        public bool IsCompleted { get; private set; }

        public HabitEntity Habit {  get; private set; }

        private HabitLog() { }


        public HabitLog(Guid habitId, DateTime date, bool isCompleted)
        {
            Id = Guid.NewGuid();
            HabitId = habitId;
            Date = date;
            IsCompleted = isCompleted;
        }
    }
}
