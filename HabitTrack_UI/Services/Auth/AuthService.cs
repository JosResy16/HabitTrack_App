using HabitTrack_UI.Utils;
using HabitTracker.Application.DTOs;
using Microsoft.AspNetCore.Components.Authorization;

namespace HabitTrack_UI.Services.Auth;
public class AuthService
{
    private readonly AuthApiClient _authApiClient;
    private readonly JwtAuthenticationStateProvider _authenticationStateProvider;
    private readonly TokenStorageService _tokenStorageService;
    private readonly UserSession _userSession;

    public AuthService(
        AuthApiClient authApiClient,
        JwtAuthenticationStateProvider authenticationStateProvider,
        TokenStorageService tokenStorageService,
        UserSession userSession)
    {
        _authApiClient = authApiClient;
        _authenticationStateProvider = authenticationStateProvider;
        _tokenStorageService = tokenStorageService;
        _userSession = userSession;
    }

    public async Task LoginAsync(LoginRequest request)
    {
        var response = await _authApiClient.Login(request);

        await _tokenStorageService.SetTokens(
            response.AccessToken,
            response.RefreshToken);

        ((JwtAuthenticationStateProvider)_authenticationStateProvider)
            .NotifyUserAuthentication(response.AccessToken);

        await _userSession.Initialize(response.AccessToken);
    }

    public async Task LogoutAsync()
    {
        await _tokenStorageService.Clear();
        _userSession.Clear();

        ((JwtAuthenticationStateProvider)_authenticationStateProvider)
            .NotifyUserLogout();
    }

    public async Task RefreshTokenAsync()
    {
        throw new NotImplementedException();
    }

    public async Task<bool> TryRefreshTokenAsync()
    {
        throw new NotImplementedException();
    }

    public async Task InitializeAsync()
    {
        var token = await _tokenStorageService.GetToken();

        if (string.IsNullOrEmpty(token))
            return;

        var principal = JwtUtils.TryCreatePrincipal(token);

        if (principal == null)
        {
            await LogoutAsync();
            return;
        }

        _authenticationStateProvider.NotifyUserAuthenticated(principal);
        await _userSession.LoadAsync();
    }

}

