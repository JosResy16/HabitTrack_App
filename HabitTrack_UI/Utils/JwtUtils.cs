using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace HabitTrack_UI.Utils;
public static class JwtUtils
{
    public static IEnumerable<Claim> Parse(string jwt)
    {
        var handler = new JwtSecurityTokenHandler();
        var token = handler.ReadJwtToken(jwt);
        return token.Claims;
    }

    public static ClaimsPrincipal? TryCreatePrincipal(string jwt)
    {
        try
        {
            var handler = new JwtSecurityTokenHandler();
            var token = handler.ReadJwtToken(jwt);

            if (token.ValidTo < DateTime.UtcNow)
                return null;

            var identity = new ClaimsIdentity(token.Claims, "jwt");
            return new ClaimsPrincipal(identity);
        }
        catch
        {
            return null;
        }
    }
}

