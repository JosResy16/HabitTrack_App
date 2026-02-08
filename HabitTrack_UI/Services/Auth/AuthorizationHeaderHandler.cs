using System.Net;
using System.Net.Http.Headers;

namespace HabitTrack_UI.Services.Auth;
public class AuthorizationHeaderHandler : DelegatingHandler
{
    private readonly TokenStorageService _tokenStorage;

    public AuthorizationHeaderHandler(TokenStorageService tokenStorage)
    {
        _tokenStorage = tokenStorage;
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var token = await _tokenStorage.GetToken();

        if (!string.IsNullOrWhiteSpace(token))
        {
            request.Headers.Authorization =
                new AuthenticationHeaderValue("Bearer", token);
        }

        return await base.SendAsync(request, cancellationToken);
    }
}
