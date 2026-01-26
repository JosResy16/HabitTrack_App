namespace HabitTracker.Application.Services
{
    public interface IUserContextService
    {
        Result<Guid> GetCurrentUserId();
    }
}
