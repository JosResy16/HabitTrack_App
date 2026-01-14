using HabitTracker.Application.Common.Interfaces;
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

        public Guid GetCurrentUserId()
        {
            var claims = _httpContextAccessor.HttpContext?.User?.Claims;

            if (claims == null || !claims.Any())
                throw new UnauthorizedAccessException("No claims found.");

            foreach (var claim in claims)
            {
                Console.WriteLine($"{claim.Type} = {claim.Value}");
            }

            var userClaim = claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
            if (userClaim == null)
                throw new UnauthorizedAccessException("User ID claim not found!");

            return Guid.Parse(userClaim.Value);
        }
    }
}
