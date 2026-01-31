using System.Net;
using System.Net.Http.Headers;

namespace HabitTrack_UI.Services.Auth;
public class AuthorizationHeaderHandler : DelegatingHandler
{
    private readonly TokenStorageService _tokenStorage;
    private readonly AuthService _authService;

    public AuthorizationHeaderHandler(TokenStorageService tokenStorage, AuthService authService)
    {
        _tokenStorage = tokenStorage;
        _authService = authService;
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        if (request.Options.TryGetValue(
        new HttpRequestOptionsKey<bool>("Retry"), out var alreadyRetried)
        && alreadyRetried)
        {
            return await base.SendAsync(request, cancellationToken);
        }

        var token = await _tokenStorage.GetToken();
        if (!string.IsNullOrWhiteSpace(token))
        {
            request.Headers.Authorization =
                new AuthenticationHeaderValue("Bearer", token);
        }

        var response = await base.SendAsync(request, cancellationToken);
        if (response.StatusCode != HttpStatusCode.Unauthorized)
            return response;

        request.Options.Set(new HttpRequestOptionsKey<bool>("Retry"), true);

        var refreshed = await _authService.TryRefreshTokenAsync();
        if (!refreshed)
            return response;

        var newRequest = await CloneHttpRequestMessageAsync(request);

        var newToken = await _tokenStorage.GetToken();
        newRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", newToken);

        return await base.SendAsync(newRequest, cancellationToken);
    }

    private static async Task<HttpRequestMessage> CloneHttpRequestMessageAsync(HttpRequestMessage request)
    {
        var clone = new HttpRequestMessage(request.Method, request.RequestUri);

        foreach (var header in request.Headers)
        {
            clone.Headers.TryAddWithoutValidation(header.Key, header.Value);
        }

        if (request.Content != null)
        {
            var ms = new MemoryStream();
            await request.Content.CopyToAsync(ms);
            ms.Position = 0;

            clone.Content = new StreamContent(ms);

            foreach (var header in request.Content.Headers)
                clone.Content.Headers.TryAddWithoutValidation(header.Key, header.Value);
        }

        return clone;
    }
}
