using System.Security.Claims;

namespace HabitTrack_API.Common
{
    public static class ClaimsPrincipalExtensions
    {
        public static Guid GetUserId(this ClaimsPrincipal user)
        {
            if (!user.Identity?.IsAuthenticated ?? true)
                throw new UnauthorizedAccessException("User is not authenticated");

            var value = user.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!Guid.TryParse(value, out var userId))
                throw new UnauthorizedAccessException("Invalid user id claim");

            return userId;
        }
    }
}
