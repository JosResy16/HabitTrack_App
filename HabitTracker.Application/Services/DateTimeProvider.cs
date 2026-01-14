
using HabitTracker.Application.Common.Interfaces;

namespace HabitTracker.Application.Services
{
    public class DateTimeProvider : IDateTimeProvider
    {
        public DateTime UtcNow => DateTime.UtcNow;
    }
}
