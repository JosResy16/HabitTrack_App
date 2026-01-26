using Microsoft.AspNetCore.Http;
using System.Security.Claims;


namespace HabitTracker.Application.Services
{
    public class UserContextService : IUserContextService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public UserContextService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public Result<Guid> GetCurrentUserId()
        {
            var claim = _httpContextAccessor.HttpContext?.User?
                .FindFirst(ClaimTypes.NameIdentifier);

            if (claim == null)
                return Result<Guid>.Failure("User not authenticated");

            if (!Guid.TryParse(claim.Value, out var userId))
                return Result<Guid>.Failure("Invalid user id");

            return Result<Guid>.Success(userId);
        }
    }
}
